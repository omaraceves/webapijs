using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<ActivationDb>(opt => opt.UseInMemoryDatabase("Activationdb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
const string idPath = "/todoitems/{id}";

//POST api/v1/userDeviceCodes
app.MapPost("/api/v1/userDeviceCodes", async (UserDeviceCodeRequest request, ActivationDb db) => 
{
    var result = new UserDeviceCode(request);
    result.Code = CodeGenerator.GetActivationCode();

    db.UserDeviceCodes.Add(result);
    await db.SaveChangesAsync();

    return Results.Created($"api/v1/userDeviceCodes/{result.DeviceId}_{result.DeviceType}", result);
});

//App Apis
//Api to get a new code - This Api generates a new code and stores it temporarily. The Api is capable of refresh the code
//Api to check for subscription - This api will check if DeviceId and DeviceType combination are subscribed.
//Took some notes on paper


//Web Page Api
//Api to register the code - When the code is registered a flag will change, allowing the activationCodes api to be able to login into the system


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
    

app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();

static class CodeGenerator
{
    public static string GetActivationCode() 
    {
        return "ABCD";
    }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

class ActivationDb : DbContext
{
    public ActivationDb(DbContextOptions<ActivationDb> options) : base(options) { }
    public DbSet<UserDeviceCode> UserDeviceCodes => Set<UserDeviceCode>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
}


class UserDeviceCode
{
    public int Id { get; set; }
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Code { get; set; }

    public UserDeviceCode(UserDeviceCodeRequest source)
    {
        DeviceId = source.DeviceId;
        DeviceType = source.DeviceType;
    }
}

class UserDevice
{
    public int Id { get; set; }
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
    public int UserId { get; set; }
    public User Users { get; set; }
}

public enum DeviceType
{
    Unknown = -1,
    NotDefined = 0,
    AndroidTV,
    Roku,
    AppleTV
}

class UserDeviceCodeRequest
{
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
}

class UserDeviceCodeResponse
{
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Code { get; set; }

    public UserDeviceCodeResponse(UserDeviceCode source)
    {
        DeviceId = source.DeviceId;
        DeviceType = source.DeviceType;
        Code = source.Code;
    }
}

class User 
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
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