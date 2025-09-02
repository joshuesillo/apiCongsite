using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMongoClient>(s =>
{
    // *** IMPORTANTE: Reemplaza esta cadena de conexión con la tuya. ***
    // Asegúrate de que no tenga espacios ni caracteres adicionales.
    var connectionUri = builder.Configuration.GetConnectionString("MongoDBConnection");
    var settings = MongoClientSettings.FromConnectionString(connectionUri);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    var client = new MongoClient(settings);
    
    // Opcional: Probar la conexión al iniciar la app.
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

app.MapGet("/users", async (IMongoClient client) =>
{
    var database = client.GetDatabase("data");
    var collection = database.GetCollection<User>("users");
    var users = await collection.Find(new BsonDocument()).ToListAsync();
    
    return Results.Ok(users);
});

app.Run();
