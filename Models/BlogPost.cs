using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class BlogPost
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("excerpt")]
    public string? Excerpt { get; set; }

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = "Nutrition";

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    [BsonElement("readTime")]
    public int ReadTime { get; set; }

    [BsonElement("published")]
    public bool Published { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
