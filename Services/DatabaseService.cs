using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;

namespace BringTheDiet.Api.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IMongoDatabase _database;

    public DatabaseService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoDatabase GetDatabase() => _database;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}
