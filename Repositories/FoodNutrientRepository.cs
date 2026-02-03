using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface IFoodNutrientRepository
{
    Task<(List<FoodNutrient> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20);
    Task<FoodNutrient?> GetByIdAsync(string id);
    Task<List<FoodNutrient>> GetByFoodIdAsync(string foodId);
    Task<List<FoodNutrient>> GetByNutrientIdAsync(string nutrientId);
    Task<FoodNutrient> CreateAsync(FoodNutrient foodNutrient);
    Task<List<FoodNutrient>> CreateManyAsync(IEnumerable<FoodNutrient> foodNutrients);
    Task<bool> UpdateAsync(string id, FoodNutrient foodNutrient);
    Task<bool> DeleteAsync(string id);
    Task<long> DeleteByFoodIdAsync(string foodId);
    Task<bool> UpsertAsync(string foodId, string nutrientId, FoodNutrient foodNutrient);
}

public class FoodNutrientRepository : IFoodNutrientRepository
{
    private readonly IMongoCollection<FoodNutrient> _collection;
    private readonly SemaphoreSlim _countLock = new(1, 1);
    private readonly object _countCacheSync = new();
    private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);
    private long? _cachedTotalCount;
    private DateTime _countCacheUpdatedAt = DateTime.MinValue;

    public FoodNutrientRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<FoodNutrient>(settings.Value.Collections.FoodNutrients);
    }

    public async Task<(List<FoodNutrient> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20)
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

    public async Task<FoodNutrient?> GetByIdAsync(string id)
    {
        return await _collection.Find(fn => fn.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<FoodNutrient>> GetByFoodIdAsync(string foodId)
    {
        return await _collection.Find(fn => fn.FoodId == foodId).ToListAsync();
    }

    public async Task<List<FoodNutrient>> GetByNutrientIdAsync(string nutrientId)
    {
        return await _collection.Find(fn => fn.NutrientId == nutrientId).ToListAsync();
    }

    public async Task<FoodNutrient> CreateAsync(FoodNutrient foodNutrient)
    {
        await _collection.InsertOneAsync(foodNutrient);
        AdjustCachedCount(1);
        return foodNutrient;
    }

    public async Task<List<FoodNutrient>> CreateManyAsync(IEnumerable<FoodNutrient> foodNutrients)
    {
        var list = foodNutrients.ToList();
        if (list.Count == 0) return list;

        await _collection.InsertManyAsync(list);
        AdjustCachedCount(list.Count);
        return list;
    }

    public async Task<bool> UpdateAsync(string id, FoodNutrient foodNutrient)
    {
        var result = await _collection.ReplaceOneAsync(fn => fn.Id == id, foodNutrient);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(fn => fn.Id == id);
        if (result.DeletedCount > 0)
        {
            AdjustCachedCount(-1);
        }
        return result.DeletedCount > 0;
    }

    public async Task<long> DeleteByFoodIdAsync(string foodId)
    {
        var result = await _collection.DeleteManyAsync(fn => fn.FoodId == foodId);
        if (result.DeletedCount > 0)
        {
            AdjustCachedCount(-result.DeletedCount);
        }
        return result.DeletedCount;
    }

    public async Task<bool> UpsertAsync(string foodId, string nutrientId, FoodNutrient foodNutrient)
    {
        var filter = Builders<FoodNutrient>.Filter.And(
            Builders<FoodNutrient>.Filter.Eq(fn => fn.FoodId, foodId),
            Builders<FoodNutrient>.Filter.Eq(fn => fn.NutrientId, nutrientId)
        );

        var options = new ReplaceOptions { IsUpsert = true };
        var result = await _collection.ReplaceOneAsync(filter, foodNutrient, options);

        if (result.UpsertedId != null)
        {
            AdjustCachedCount(1);
        }

        return result.ModifiedCount > 0 || result.UpsertedId != null;
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
