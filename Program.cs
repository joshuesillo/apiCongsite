using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var connectionUri = builder.Configuration.GetConnectionString("MongoDBConnection");
    var settings = MongoClientSettings.FromConnectionString(connectionUri);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    var client = new MongoClient(settings);
    
    try
    {
        var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
    }
    return client;
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/health", () => "Todo bien pana!");

app.MapGet("/users", async (IMongoClient client) =>
{
    Console.WriteLine("Call to /users");
    try
    {
        var database = client.GetDatabase("data");
        var collection = database.GetCollection<User>("users");
        var users = await collection.Find(new BsonDocument()).ToListAsync();
        
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/users", async (User user, IMongoClient client) =>
{
    try
    {
        var database = client.GetDatabase("data");
        var collection = database.GetCollection<User>("users");

        await collection.insertOneAsync(user);
        
        return Results.Created($"/users/{user.Name}", user);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
