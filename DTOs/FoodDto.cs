namespace BringTheDiet.Api.DTOs;

public class FoodDto
{
    public string? Id { get; set; }
    public int FdcId { get; set; }
    public string? FoodClass { get; set; }
    public string? DataType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? BrandOwner { get; set; }
    public string? BrandedFoodCategory { get; set; }
    public string? GtinUpc { get; set; }
    public string? MarketCountry { get; set; }
    public string? Ingredients { get; set; }
    public double? ServingSize { get; set; }
    public string? ServingSizeUnit { get; set; }
    public string? HouseholdServingFullText { get; set; }
    public string? PublicationDate { get; set; }
    public string? AvailableDate { get; set; }
    public string? ModifiedDate { get; set; }
    public List<string>? TradeChannels { get; set; }
}

public class CreateFoodDto
{
    public int FdcId { get; set; }
    public string? FoodClass { get; set; }
    public string? DataType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? BrandOwner { get; set; }
    public string? BrandedFoodCategory { get; set; }
    public string? GtinUpc { get; set; }
    public string? MarketCountry { get; set; }
    public string? Ingredients { get; set; }
    public double? ServingSize { get; set; }
    public string? ServingSizeUnit { get; set; }
    public string? HouseholdServingFullText { get; set; }
    public string? PublicationDate { get; set; }
    public List<string>? TradeChannels { get; set; }
}

public class UpdateFoodDto
{
    public string? Description { get; set; }
    public string? BrandOwner { get; set; }
    public string? Ingredients { get; set; }
    public double? ServingSize { get; set; }
    public string? ServingSizeUnit { get; set; }
}
