using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BringTheDiet.Api.Models;

[BsonIgnoreExtraElements]
public class Food
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("fdcId")]
    public int FdcId { get; set; }

    [BsonElement("foodClass")]
    public string? FoodClass { get; set; }

    [BsonElement("dataType")]
    public string? DataType { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("brandOwner")]
    public string? BrandOwner { get; set; }

    [BsonElement("brandedFoodCategory")]
    public string? BrandedFoodCategory { get; set; }

    [BsonElement("gtinUpc")]
    public string? GtinUpc { get; set; }

    [BsonElement("marketCountry")]
    public string? MarketCountry { get; set; }

    [BsonElement("ingredients")]
    public string? Ingredients { get; set; }

    [BsonElement("servingSize")]
    public double? ServingSize { get; set; }

    [BsonElement("servingSizeUnit")]
    public string? ServingSizeUnit { get; set; }

    [BsonElement("householdServingFullText")]
    public string? HouseholdServingFullText { get; set; }

    [BsonElement("publicationDate")]
    public string? PublicationDate { get; set; }

    [BsonElement("availableDate")]
    public string? AvailableDate { get; set; }

    [BsonElement("modifiedDate")]
    public string? ModifiedDate { get; set; }

    [BsonElement("tradeChannels")]
    public List<string>? TradeChannels { get; set; }
}
