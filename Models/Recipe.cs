using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class Recipe
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string? Slug { get; set; }

    [BsonElement("summary")]
    public string? Summary { get; set; }

    [BsonElement("dietTags")]
    public List<string>? DietTags { get; set; }

    [BsonElement("cuisine")]
    public string? Cuisine { get; set; }

    [BsonElement("prepMinutes")]
    public int? PrepMinutes { get; set; }

    [BsonElement("cookMinutes")]
    public int? CookMinutes { get; set; }

    [BsonElement("servings")]
    public int? Servings { get; set; }

    [BsonElement("difficulty")]
    public string? Difficulty { get; set; }

    [BsonElement("ingredients")]
    public List<RecipeIngredient>? Ingredients { get; set; }

    [BsonElement("steps")]
    public List<string>? Steps { get; set; }

    [BsonElement("images")]
    public List<string>? Images { get; set; }

    [BsonElement("status")]
    public string? Status { get; set; }

    [BsonElement("nutrition")]
    public RecipeNutrition? Nutrition { get; set; }

    [BsonElement("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("verified")]
    public bool Verified { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class RecipeIngredient
{
    [BsonElement("fdcId")]
    public int? FdcId { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("quantity")]
    public double? Quantity { get; set; }

    [BsonElement("unit")]
    public string? Unit { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }
}

[BsonIgnoreExtraElements]
public class RecipeNutrition
{
    [BsonElement("calories")]
    public double? Calories { get; set; }

    [BsonElement("protein")]
    public double? Protein { get; set; }

    [BsonElement("carbs")]
    public double? Carbs { get; set; }

    [BsonElement("fat")]
    public double? Fat { get; set; }

    [BsonElement("nutrients")]
    public List<NutrientInfo>? Nutrients { get; set; }
}

[BsonIgnoreExtraElements]
public class NutrientInfo
{
    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("amount")]
    public double? Amount { get; set; }

    [BsonElement("unit")]
    public string? Unit { get; set; }
}
