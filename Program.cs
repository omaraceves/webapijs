using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<ActivationDb>(opt => opt.UseInMemoryDatabase("Activationdb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
const string idPath = "/todoitems/{id}";

//GET api/v1/userDevices/{deviceId}/{deviceType}
app.MapGet("/api/v1/userDevices/{deviceId}/{deviceType}", async (Guid deviceId, DeviceType deviceType, ActivationDb db) => 
{   
    UserDeviceResponse response;
    var result = await db.UserDevices
    .Include(x => x.User)
    .Include(x => x.Code)
    .FirstOrDefaultAsync(x => x.DeviceId == deviceId 
                            && x.DeviceType == deviceType);

    if(result == null)
    {
        return Results.NotFound();
    }
    else if(result.User != null)
    {
        response = new UserDeviceResponse(result, result.User);
        return Results.Ok(response);
    }
    else if(result.Code != null)
    {
        response = new UserDeviceResponse(result, result.Code);
        return Results.Ok(response);
    }

    return Results.NotFound();
});

//POST api/v1/userDevices
app.MapPost("/api/v1/userDevices", async(UserDeviceRequest request, ActivationDb db) => {
    UserDeviceResponse response;

    var result = await db.UserDevices
    .Include(x => x.User)
    .Include(x => x.Code)
    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId 
                            && x.DeviceType == request.DeviceType);

    //What to do when resource already exists? Conflict 409 code.
    if (result != null)
        return Results.Conflict(new UserDeviceResponse(result));

    //To create a userDevice, first we need a new code.
    var code = CodeGenerator.GetActivationCode();
    var userDevice = new UserDevice(request.DeviceId, request.DeviceType, code);

    db.UserDevices.Add(userDevice);
    response = new UserDeviceResponse(userDevice);

    return Results.Created($"api/v1/userDevices?deviceId={request.DeviceId}&deviceType={request.DeviceType}", 
    response);
});

//PUT api/v1/userDevices/{deviceId}/{deviceType}
app.MapPut("/api/v1/userDevices/{deviceId}/{deviceType}", async (
    Guid deviceId, DeviceType deviceType, UserDeviceRequest request, ActivationDb db) => 
{
    var result = await db.UserDevices
    .Include(x => x.User)
    .Include(x => x.Code)
    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId 
                            && x.DeviceType == request.DeviceType);

    if (result == null)
        return Results.NotFound();
    
    //refresh code
    var oldCode = result.Code.Code;
    var code = CodeGenerator.GetActivationCode();
    result.Code.Code = code;
    result.Code.ExpirationDate = TimeHelper.GetExpirationDate();
    db.UserDevices.Update(result);

    //recycle code
    CodeGenerator.PushCode(oldCode);

    return Results.NoContent();
});

//POST api/v1/userDevices/register
app.MapPost("/api/v1/userDevices/register", async(UserDeviceRegisterRequest request, ActivationDb db) => {
    UserDeviceResponse response;

    var result = await db.UserDevices
    .Include(x => x.User)
    .Include(x => x.Code)
    .FirstOrDefaultAsync(x => x.Code.Code == request.Code);

    //What to do when resource already exists? Conflict 409 code.
    if (result == null)
        return Results.NotFound();

    //Update User Device
    result.Code.ExpirationDate = TimeHelper.GetUnixTime(); //Expire code now.
    result.UserId = request.UserId;
    db.UserDevices.Update(result);

    //recycle code
    CodeGenerator.PushCode(result.Code.Code);

    //todo fix this
    //todo api to recycle all the codes.
    return Results.Created($"api/v1/userDevices?deviceId={request.DeviceId}&deviceType={request.DeviceType}", 
    response);
});

#region TodoItems apis
app.MapGet("/todoitems", async (TodoDb db) => 
    await db.Todos.Select(todo => new TodoDto(todo)).ToListAsync());
app.MapGet("/todoitems/complete", async (TodoDb db) => 
    await db.Todos.Where(x => x.IsComplete)
        .Select(todo => new TodoDto(todo)).ToListAsync());
app.MapGet(idPath, async (int id, TodoDb db) => 
    await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(new TodoDto(todo)) : Results.NotFound());
app.MapPost("/todoitems", async (TodoDto itemDto, TodoDb db) => {
    var todoItem = new Todo() 
    {
        Id = itemDto.Id,
        IsComplete = itemDto.IsComplete,
        Name = itemDto.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todoItem.Id}", new TodoDto(todoItem));
});
app.MapPut(idPath, async (int id, TodoDto item, TodoDb db) => {
    var result = await db.Todos.FindAsync(id);

    if(result is null) return Results.NotFound();

    result.IsComplete = item.IsComplete;
    result.Name = item.Name;
    db.Update(result);
    await db.SaveChangesAsync();

    return Results.NoContent();
});
app.MapDelete(idPath, async (int id, TodoDb db) => {
    var result = await db.Todos.FindAsync(id);
    
    if(result is null) return Results.NotFound();

    db.Todos.Remove(result);
    await db.SaveChangesAsync();
    return Results.Ok(new TodoDto(result));
});
#endregion
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();

#region Todoapi classes
class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
class Todo
{
    public int Id {get;set;}
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
}
class TodoDto 
{
    public int Id {get;set;}
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoDto(){ }
    public TodoDto(Todo todo) => (this.Id, this.Name, this.IsComplete) = (todo.Id, todo.Name, todo.IsComplete);

}
#endregion