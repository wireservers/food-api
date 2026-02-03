using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

/// <summary>
/// Junction table linking foods to nutrients with their specific values.
/// Each record represents "Food X contains Y amount of Nutrient Z".
/// </summary>
public class FoodNutrient
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Reference to the food document
    /// </summary>
    [BsonElement("foodId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FoodId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the nutrient definition
    /// </summary>
    [BsonElement("nutrientId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string NutrientId { get; set; } = string.Empty;

    /// <summary>
    /// The amount of this nutrient in the food (per 100g or per serving)
    /// </summary>
    [BsonElement("amount")]
    public double Amount { get; set; }

    /// <summary>
    /// How the value was derived (e.g., "Analytical", "Calculated", "Assumed")
    /// </summary>
    [BsonElement("derivationDescription")]
    public string? DerivationDescription { get; set; }

    /// <summary>
    /// Data points used to derive the value (for statistical data)
    /// </summary>
    [BsonElement("dataPoints")]
    public int? DataPoints { get; set; }

    /// <summary>
    /// Minimum value (for range data)
    /// </summary>
    [BsonElement("min")]
    public double? Min { get; set; }

    /// <summary>
    /// Maximum value (for range data)
    /// </summary>
    [BsonElement("max")]
    public double? Max { get; set; }
}
