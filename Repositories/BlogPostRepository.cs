using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BringTheDiet.Api.Configuration;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Services;

namespace BringTheDiet.Api.Repositories;

public interface IBlogPostRepository
{
    Task<(List<BlogPost> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? category = null);
    Task<BlogPost?> GetByIdAsync(string id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<BlogPost> CreateAsync(BlogPost post);
    Task<bool> UpdateAsync(string id, BlogPost post);
    Task<bool> DeleteAsync(string id);
    Task<List<BlogPost>> SearchAsync(string searchTerm);
}

public class BlogPostRepository : IBlogPostRepository
{
    private readonly IMongoCollection<BlogPost> _collection;

    public BlogPostRepository(IDatabaseService databaseService, IOptions<MongoDbSettings> settings)
    {
        _collection = databaseService.GetCollection<BlogPost>(settings.Value.Collections.Blog);
    }

    public async Task<(List<BlogPost> Items, long TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? category = null)
    {
        var skip = (page - 1) * pageSize;
        var filterBuilder = Builders<BlogPost>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrEmpty(category))
        {
            filter = filterBuilder.Regex(p => p.Category, new MongoDB.Bson.BsonRegularExpression(category, "i"));
        }

        var itemsTask = _collection.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
        var countTask = _collection.CountDocumentsAsync(filter);

        await Task.WhenAll(itemsTask, countTask);
        return (itemsTask.Result, countTask.Result);
    }

    public async Task<BlogPost?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        return await _collection.Find(p => p.Slug == slug).FirstOrDefaultAsync();
    }

    public async Task<BlogPost> CreateAsync(BlogPost post)
    {
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(post);
        return post;
    }

    public async Task<bool> UpdateAsync(string id, BlogPost post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        var result = await _collection.ReplaceOneAsync(p => p.Id == id, post);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<BlogPost>> SearchAsync(string searchTerm)
    {
        var filter = Builders<BlogPost>.Filter.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
        return await _collection.Find(filter).SortByDescending(p => p.CreatedAt).ToListAsync();
    }
}
