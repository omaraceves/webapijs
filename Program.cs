using Microsoft.EntityFrameworkCore;
using UserDeviceApi.Context;
using UserDeviceApi.Helpers;
using UserDeviceApi.Model;
using UserDeviceApi.Model.RequestResponse;
using UserDeviceApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<UserDevicesDB>(opt => opt.UseInMemoryDatabase("UserDevicesDB"));
//builder.Services.AddDbContext<UserDevicesDB>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("UserDevicesConnection")));
builder.Services.AddTransient<UserDeviceService>();
builder.Services.AddTransient<UserDeviceCodeService>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
const string idPath = "/todoitems/{id}";

//GET api/v1/userDevices/{deviceType}/{deviceId}
app.MapGet("/api/v1/userDevices/{deviceType}/{deviceId}",async (Guid deviceId, DeviceType deviceType, 
    UserDeviceService service) => 
{   
    UserDeviceResponse response;
    var result = await service.GetUserDevices()
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
    else if(result.UserDeviceCodes.Any())
    {
        response = new UserDeviceResponse(result, result.UserDeviceCodes.First());
        return Results.Ok(response);
    }

    return Results.NotFound();
});

//GET api/v1/userDevices
app.MapGet("/api/v1/userDevices", async (UserDeviceService service) =>
{
    UserDeviceResponse response;
    var result = await service.GetUserDevices().ToListAsync();
    
    if (result == null || result.Count == 0)
    {
        return Results.NotFound();
    }

    return Results.Ok(result);
});

//GET api/v1/users
app.MapGet("/api/v1/users", async (UserDevicesDB context) =>
{
    return Results.Ok(context.Users);
});

//POST api/v1/userDevices
app.MapPost("/api/v1/userDevices", async(UserDeviceRequest request, UserDeviceService service) => {
    UserDeviceResponse response;

    var result = await service.GetAll()
    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId 
                            && x.DeviceType == request.DeviceType);

    //What to do when resource already exists? Conflict 409 code.
    if (result != null)
        return Results.Conflict("Device already exists");

    //To create a userDevice, first we need a new code.
    var code = CodeGenerator.GetActivationCode();
    var userDevice = new UserDevice(request.DeviceId, request.DeviceType, code);

    await service.AddAsync(userDevice);
    response = new UserDeviceResponse(userDevice, userDevice.UserDeviceCodes.FirstOrDefault());

    return Results.Created($"api/v1/userDevices/{request.DeviceId}/{request.DeviceType}", 
    response);
});

//PUT api/v1/userDevices
app.MapPut("/api/v1/userDevices", async (UserDeviceRequest request, UserDeviceService service) => 
{
    var result = await service.GetAll()
    .Include(x => x.UserDeviceCodes.OrderByDescending(code => code.ExpirationDate))
    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId 
                            && x.DeviceType == request.DeviceType);

    if (result == null)
        return Results.NotFound();

    //expire current code so it can be recycled
    if (result.UserDeviceCodes[0] != null)
        result.UserDeviceCodes[0].ExpirationDate = TimeHelper.GetUnixTime();

    //refresh code
    var code = CodeGenerator.GetActivationCode();
    var userDeviceCode = new UserDeviceCode(code);
    result.UserDeviceCodes.Add(userDeviceCode);
    service.Update(result);

    return Results.NoContent();
});

//PUT api/v1/userDevices/expire
app.MapPut("/api/v1/userDevices/expire", async (UserDeviceRequest request, UserDeviceService service) =>
{
    var result = await service.GetAll()
    .Include(x => x.UserDeviceCodes.OrderByDescending(code => code.ExpirationDate))
    .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId
                            && x.DeviceType == request.DeviceType);

    if (result == null)
        return Results.NotFound();

    //expire current code so it can be recycled
    if (result.UserDeviceCodes[0] == null)
    {
        return Results.NotFound();
        
    }

    result.UserDeviceCodes[0].ExpirationDate = TimeHelper.GetUnixTime();
    service.Update(result);
    return Results.NoContent();
});

//POST api/v1/userDevices/register
app.MapPost("/api/v1/userDevices/register", async(UserDeviceRegisterRequest request, UserDeviceService service) => {

    var result = await service.GetUserDevices()
    .FirstOrDefaultAsync(x => x.UserDeviceCodes.Any(y => y.Code == request.Code));

    //What to do when resource already exists? Conflict 409 code.
    if (result == null)
        return Results.NotFound();

    //Gets user device
    var userDeviceCode = result.UserDeviceCodes.First(x => x.Code == request.Code);

    //Expire code so it can be recycled
    userDeviceCode.ExpirationDate = TimeHelper.GetUnixTime();
    
    //Set userId
    result.UserId = request.UserId;
    result = service.Update(result);

    return Results.Ok(new UserDeviceResponse(result, result.User));
});

//GET api/v1/codes/recycle
app.MapGet("api/v1/codes/recycle", async(UserDeviceCodeService service) => {
    //select expired
    var expiredCodes = await service.GetUserDeviceCodes().Where(x => x.ExpirationDate <= TimeHelper.GetUnixTime()).ToListAsync();

    expiredCodes.ForEach(x => {
        var code = x.Code;
        CodeGenerator.PushCode(code);
    });

    return Results.Ok($"{expiredCodes.Count} codes recycled");
});

//GET api/v1/codes/seed
app.MapGet("api/v1/codes/seed", async(UserDeviceService service) => {

    var seedIds = new List<Guid> { Guid.Parse("01b1e96d-4bb8-4793-b9fa-d29fa1d20b10"),
        Guid.Parse("4f931e15-3ad8-4a11-a1c9-67d45546d95d"), 
        Guid.Parse("c405c882-8c09-466a-9cc8-062b5467faf6") };

    if (service.GetAll().Where(x => seedIds.Contains(x.Id)).Any())
        return Results.Ok("DB was already seeded.");

    var userDevicesToAdd = new List<UserDevice>() {
        new UserDevice() {
            Id = Guid.Parse("01b1e96d-4bb8-4793-b9fa-d29fa1d20b10"),
            DeviceId = Guid.Parse("01b1e96d-4bb8-4793-b9fa-d29fa1d20b10"),
            DeviceType = DeviceType.AppleTV,
            UserDeviceCodes = new List<UserDeviceCode>(){ new UserDeviceCode() {
                Code = CodeGenerator.GetActivationCode(),
                ExpirationDate = TimeHelper.GetExpirationDate(),
                Id = Guid.Parse("ddc990b4-d7a7-4976-a3d5-71d47f96d2af"),
                UserDeviceId = Guid.Parse("01b1e96d-4bb8-4793-b9fa-d29fa1d20b10")
            } }
        },
        new UserDevice() {
            Id = Guid.Parse("4f931e15-3ad8-4a11-a1c9-67d45546d95d"),
            DeviceId = Guid.Parse("4f931e15-3ad8-4a11-a1c9-67d45546d95d"),
            DeviceType = DeviceType.AppleTV,
            UserDeviceCodes = new List<UserDeviceCode>(){ new UserDeviceCode() {
                UserDeviceId = Guid.Parse("4f931e15-3ad8-4a11-a1c9-67d45546d95d"),
                Code = CodeGenerator.GetActivationCode(),
                ExpirationDate = TimeHelper.GetExpirationDate(),
                Id = Guid.Parse("069847f7-cb49-46cb-927b-517e744bacd9")
            } }
        },
        new UserDevice() {
            Id = Guid.Parse("c405c882-8c09-466a-9cc8-062b5467faf6"),
            DeviceId = Guid.Parse("c405c882-8c09-466a-9cc8-062b5467faf6"),
            DeviceType = DeviceType.AppleTV,
            UserDeviceCodes = new List<UserDeviceCode>(){ new UserDeviceCode() {
                Code = CodeGenerator.GetActivationCode(),
                ExpirationDate = TimeHelper.GetExpirationDate(),
                Id = Guid.Parse("b877b9d1-0227-407f-911d-7f6c50bf412b"),
                UserDeviceId = Guid.Parse("c405c882-8c09-466a-9cc8-062b5467faf6")
            } }
        }
    };

    service.BulkAddAsync(userDevicesToAdd);

    var results = service.GetAll().ToList();
    return Results.Ok(results);
});

app.MapGet("api/v1/users/seed", async (UserDevicesDB context) => {

    var seedIds = new List<Guid> { Guid.Parse("cef45d4c-b4b9-4e36-bcf0-04b2ff042a8f"),
        Guid.Parse("7d86c22c-cafe-4331-a6f3-bc090a9f7d9c"),
        Guid.Parse("b4252290-5fc1-441e-8e14-f54af575bde3") };

    if (context.Users.Where(x => seedIds.Contains(x.Id)).Any())
        return Results.Ok("DB was already seeded.");

    var userDevicesToAdd = new List<User>() {
        new User() {
            Id = Guid.Parse("cef45d4c-b4b9-4e36-bcf0-04b2ff042a8f"),
            UserName = "user1"
        },
        new User() {
            Id = Guid.Parse("7d86c22c-cafe-4331-a6f3-bc090a9f7d9c"),
            UserName = "user2"
        },
        new User() {
            Id = Guid.Parse("b4252290-5fc1-441e-8e14-f54af575bde3"),
            UserName = "user3"
        }
    };

    await context.Users.AddRangeAsync(userDevicesToAdd);
    await context.SaveChangesAsync();

    var results = context.Users;
    return Results.Ok(results);
});

#region TodoItems apis
//app.MapGet("/todoitems", async (TodoDb db) => 
//    await db.Todos.Select(todo => new TodoDto(todo)).ToListAsync());
//app.MapGet("/todoitems/complete", async (TodoDb db) => 
//    await db.Todos.Where(x => x.IsComplete)
//        .Select(todo => new TodoDto(todo)).ToListAsync());
//app.MapGet(idPath, async (int id, TodoDb db) => 
//    await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(new TodoDto(todo)) : Results.NotFound());
//app.MapPost("/todoitems", async (TodoDto itemDto, TodoDb db) => {
//    var todoItem = new Todo() 
//    {
//        Id = itemDto.Id,
//        IsComplete = itemDto.IsComplete,
//        Name = itemDto.Name
//    };

//    db.Todos.Add(todoItem);
//    await db.SaveChangesAsync();

//    return Results.Created($"/todoitems/{todoItem.Id}", new TodoDto(todoItem));
//});
//app.MapPut(idPath, async (int id, TodoDto item, TodoDb db) => {
//    var result = await db.Todos.FindAsync(id);

//    if(result is null) return Results.NotFound();

//    result.IsComplete = item.IsComplete;
//    result.Name = item.Name;
//    db.Update(result);
//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});
//app.MapDelete(idPath, async (int id, TodoDb db) => {
//    var result = await db.Todos.FindAsync(id);

//    if(result is null) return Results.NotFound();

//    db.Todos.Remove(result);
//    await db.SaveChangesAsync();
//    return Results.Ok(new TodoDto(result));
//});
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