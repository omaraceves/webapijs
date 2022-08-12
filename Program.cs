using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<ActivationDb>(opt => opt.UseInMemoryDatabase("Activationdb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
const string idPath = "/todoitems/{id}";

//GET api/v1/userDevices
app.MapGet("/api/v1/userDevices", async (Guid deviceId, DeviceType deviceType, ActivationDb db) => 
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

});



//App Apis
//Api to get a new code - This Api generates a new code and stores it temporarily. The Api is capable of refresh the code
//Api to check for subscription - This api will check if DeviceId and DeviceType combination are subscribed.
//Took some notes on paper


//Web Page Api
//Api to register the code - When the code is registered a flag will change, allowing the activationCodes api to be able to login into the system


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