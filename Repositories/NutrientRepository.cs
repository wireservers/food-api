using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface INutrientRepository
{
    Task<(List<Nutrient> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 100);
    Task<List<Nutrient>> GetAllNoCacheAsync();
    Task<Nutrient?> GetByIdAsync(string id);
    Task<Nutrient?> GetByNutrientNumberAsync(int nutrientNumber);
    Task<Nutrient> CreateAsync(Nutrient nutrient);
    Task<List<Nutrient>> CreateManyAsync(IEnumerable<Nutrient> nutrients);
    Task<bool> UpdateAsync(string id, Nutrient nutrient);
    Task<bool> DeleteAsync(string id);
    Task<List<Nutrient>> SearchByNameAsync(string searchTerm);
    Task<Dictionary<int, string>> GetNutrientNumberToIdMapAsync();
}

public class NutrientRepository : INutrientRepository
{
    private readonly IMongoCollection<Nutrient> _collection;
    private readonly SemaphoreSlim _countLock = new(1, 1);
    private readonly object _countCacheSync = new();
    private readonly TimeSpan _countCacheTtl = TimeSpan.FromMinutes(5);
    private long? _cachedTotalCount;
    private DateTime _countCacheUpdatedAt = DateTime.MinValue;

    public NutrientRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<Nutrient>(settings.Value.Collections.Nutrients);
    }

    public async Task<(List<Nutrient> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 100)
    {
        var skip = (page - 1) * pageSize;
        var itemsTask = _collection.Find(_ => true)
            .SortBy(n => n.SortOrder)
            .ThenBy(n => n.Name)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
        var countTask = GetTotalCountAsync();

        await Task.WhenAll(itemsTask, countTask);
        return (itemsTask.Result, countTask.Result);
    }

    public async Task<List<Nutrient>> GetAllNoCacheAsync()
    {
        return await _collection.Find(_ => true)
            .SortBy(n => n.SortOrder)
            .ThenBy(n => n.Name)
            .ToListAsync();
    }

    public async Task<Nutrient?> GetByIdAsync(string id)
    {
        return await _collection.Find(n => n.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Nutrient?> GetByNutrientNumberAsync(int nutrientNumber)
    {
        return await _collection.Find(n => n.NutrientNumber == nutrientNumber).FirstOrDefaultAsync();
    }

    public async Task<Nutrient> CreateAsync(Nutrient nutrient)
    {
        await _collection.InsertOneAsync(nutrient);
        AdjustCachedCount(1);
        return nutrient;
    }

    public async Task<List<Nutrient>> CreateManyAsync(IEnumerable<Nutrient> nutrients)
    {
        var nutrientList = nutrients.ToList();
        if (nutrientList.Count == 0) return nutrientList;

        await _collection.InsertManyAsync(nutrientList);
        AdjustCachedCount(nutrientList.Count);
        return nutrientList;
    }

    public async Task<bool> UpdateAsync(string id, Nutrient nutrient)
    {
        var result = await _collection.ReplaceOneAsync(n => n.Id == id, nutrient);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(n => n.Id == id);
        if (result.DeletedCount > 0)
        {
            AdjustCachedCount(-1);
        }
        return result.DeletedCount > 0;
    }

    public async Task<List<Nutrient>> SearchByNameAsync(string searchTerm)
    {
        var filter = Builders<Nutrient>.Filter.Regex(n => n.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Dictionary<int, string>> GetNutrientNumberToIdMapAsync()
    {
        var nutrients = await _collection.Find(_ => true).ToListAsync();
        return nutrients.Where(n => n.Id != null).ToDictionary(n => n.NutrientNumber, n => n.Id!);
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
