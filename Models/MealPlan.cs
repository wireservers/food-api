using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class MealPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("weekStart")]
    public string? WeekStart { get; set; }

    [BsonElement("entries")]
    public List<MealPlanEntry>? Entries { get; set; }

    [BsonElement("verified")]
    public bool Verified { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class MealPlanEntry
{
    [BsonElement("day")]
    public string? Day { get; set; }

    [BsonElement("mealType")]
    public string? MealType { get; set; }

    [BsonElement("recipeId")]
    public string? RecipeId { get; set; }

    [BsonElement("servings")]
    public int? Servings { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }
}
