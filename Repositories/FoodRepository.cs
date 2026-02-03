using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface IFoodRepository
{
    Task<(List<Food> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Food?> GetByIdAsync(string id);
    Task<Food> CreateAsync(Food food);
    Task<bool> UpdateAsync(string id, Food food);
    Task<bool> DeleteAsync(string id);
    Task<List<Food>> SearchByDescriptionAsync(string searchTerm);
}

public class FoodRepository : IFoodRepository
{
    private readonly IMongoCollection<Food> _collection;
    private readonly SemaphoreSlim _countLock = new(1, 1);
    private readonly object _countCacheSync = new();
    private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);
    private long? _cachedTotalCount;
    private DateTime _countCacheUpdatedAt = DateTime.MinValue;

    public FoodRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<Food>(settings.Value.Collections.Foods);
    }

    public async Task<(List<Food> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;
        var itemsTask = _collection.Find(_ => true)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
        var countTask = GetTotalCountAsync();

        await Task.WhenAll(itemsTask, countTask);
        return (itemsTask.Result, countTask.Result);
    }

    public async Task<Food?> GetByIdAsync(string id)
    {
        return await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Food> CreateAsync(Food food)
    {
        await _collection.InsertOneAsync(food);
        AdjustCachedCount(1);
        return food;
    }

    public async Task<bool> UpdateAsync(string id, Food food)
    {
        var result = await _collection.ReplaceOneAsync(f => f.Id == id, food);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(f => f.Id == id);
        if (result.DeletedCount > 0)
        {
            AdjustCachedCount(-1);
        }
        return result.DeletedCount > 0;
    }

    public async Task<List<Food>> SearchByDescriptionAsync(string searchTerm)
    {
        var filter = Builders<Food>.Filter.Regex(f => f.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _collection.Find(filter).ToListAsync();
    }

    private async Task<long> GetTotalCountAsync()
    {
        if (TryGetCachedCount(out var cached))
        {
            return cached;
        }

        await _countLock.WaitAsync();
        try
        {
            if (TryGetCachedCount(out cached))
            {
                return cached;
            }

            var count = await _collection.CountDocumentsAsync(_ => true);
            SetCachedCount(count);
            return count;
        }
        finally
        {
            _countLock.Release();
        }
    }

    private bool TryGetCachedCount(out long count)
    {
        lock (_countCacheSync)
        {
            if (_cachedTotalCount.HasValue && DateTime.UtcNow - _countCacheUpdatedAt < _countCacheTtl)
            {
                count = _cachedTotalCount.Value;
                return true;
            }
        }

        count = 0;
        return false;
    }

    private void SetCachedCount(long count)
    {
        lock (_countCacheSync)
        {
            _cachedTotalCount = count;
            _countCacheUpdatedAt = DateTime.UtcNow;
        }
    }

    private void AdjustCachedCount(long delta)
    {
        lock (_countCacheSync)
        {
            if (!_cachedTotalCount.HasValue)
            {
                return;
            }

            _cachedTotalCount = Math.Max(0, _cachedTotalCount.Value + delta);
            _countCacheUpdatedAt = DateTime.UtcNow;
        }
    }
}
