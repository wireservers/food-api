using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

/// <summary>
/// Master list of nutrient types (Protein, Carbohydrates, Vitamin C, etc.)
/// Each nutrient is defined once and referenced by FoodNutrient junction records.
/// </summary>
public class Nutrient
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// FDA nutrient number (e.g., 1003 for Protein, 1004 for Total Fat)
    /// </summary>
    [BsonElement("nutrientNumber")]
    public int NutrientNumber { get; set; }

    /// <summary>
    /// Display name (e.g., "Protein", "Total lipid (fat)", "Vitamin C")
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measure (e.g., "g", "mg", "mcg", "kcal", "IU")
    /// </summary>
    [BsonElement("unit")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping (e.g., "Macronutrient", "Vitamin", "Mineral", "Energy")
    /// </summary>
    [BsonElement("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Sort order for consistent display
    /// </summary>
    [BsonElement("sortOrder")]
    public int SortOrder { get; set; }
}
