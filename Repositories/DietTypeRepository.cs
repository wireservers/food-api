using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface IDietTypeRepository
{
    Task<List<DietType>> GetAllAsync();
    Task<DietType?> GetBySlugAsync(string slug);
}

public class DietTypeRepository : IDietTypeRepository
{
    private readonly IMongoCollection<DietType> _collection;

    public DietTypeRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<DietType>(settings.Value.Collections.Diets);
    }

    public async Task<List<DietType>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<DietType?> GetBySlugAsync(string slug)
    {
        return await _collection.Find(d => d.Slug == slug).FirstOrDefaultAsync();
    }
}
