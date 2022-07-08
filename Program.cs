using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
const string idPath = "/todoitems/{id}";

//POST api/v1/activationCodes
app.MapPost("/api/v1/activationCodes"), async (ActivationCode activationCodeRequest, ActivationDb db) => 
{
    var result = activationCodeRequest;
    result.Code = CodeGenerator.GetActivationCode();

    db.ActivationCodes.Add(result);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todoItem.Id}", new TodoDto(todoItem));
}



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
    public DbSet<Activation> Activations => Set<Activation>();
    public DbSet<ActivationCode> ActivationCodes => Set<ActivationCode>();
}

//This will be stored on the db
class Activation
{
    public int Id { get; set; }
    public Guid DeviceId { get; set; }
    public int DeviceTypeId { get; set; }
    public string DeviceName { get; set; }
    public Guid UserId { get; set; }
}

//This will be volatile, probably stored in a cache
class ActivationCode
{
    public Guid DeviceId { get; set; }
    public int DeviceTypeId { get; set; }
    public string DeviceName { get; set; }
    public string Code { get; set; }
}

class ActivationCodeRequest
{
    public Guid DeviceId { get; set; }
    public int DeviceTypeId { get; set; }
    public string DeviceName { get; set; }
    public bool Refresh { get; set; }
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