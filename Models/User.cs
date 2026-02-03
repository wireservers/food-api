using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("oidcSub")]
    public string? OidcSub { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string? DisplayName { get; set; }

    [BsonElement("roles")]
    public List<string>? Roles { get; set; }

    [BsonElement("verified")]
    public bool Verified { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
