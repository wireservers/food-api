using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface IMealPlanRepository
{
    Task<(List<MealPlan> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20);
    Task<MealPlan?> GetByIdAsync(string id);
    Task<List<MealPlan>> GetByUserIdAsync(string userId);
    Task<MealPlan> CreateAsync(MealPlan mealPlan);
    Task<bool> UpdateAsync(string id, MealPlan mealPlan);
    Task<bool> DeleteAsync(string id);
}

public class MealPlanRepository : IMealPlanRepository
{
    private readonly IMongoCollection<MealPlan> _collection;
    private readonly SemaphoreSlim _countLock = new(1, 1);
    private readonly object _countCacheSync = new();
    private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);
    private long? _cachedTotalCount;
    private DateTime _countCacheUpdatedAt = DateTime.MinValue;

    public MealPlanRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<MealPlan>(settings.Value.Collections.MealPlans);
    }

    public async Task<(List<MealPlan> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20)
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

    public async Task<MealPlan?> GetByIdAsync(string id)
    {
        return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<MealPlan>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(m => m.UserId == userId).ToListAsync();
    }

    public async Task<MealPlan> CreateAsync(MealPlan mealPlan)
    {
        mealPlan.CreatedAt = DateTime.UtcNow;
        mealPlan.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(mealPlan);
        AdjustCachedCount(1);
        return mealPlan;
    }

    public async Task<bool> UpdateAsync(string id, MealPlan mealPlan)
    {
        mealPlan.UpdatedAt = DateTime.UtcNow;
        var result = await _collection.ReplaceOneAsync(m => m.Id == id, mealPlan);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(m => m.Id == id);
        if (result.DeletedCount > 0)
        {
            AdjustCachedCount(-1);
        }
        return result.DeletedCount > 0;
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
