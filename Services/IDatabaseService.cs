using MongoDB.Driver;

namespace BringTheDiet.Api.Services;

public interface IDatabaseService
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string collectionName);
    IMongoDatabase GetDatabase();
}
