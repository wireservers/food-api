namespace BringTheDiet.Api.DTOs;

public class FoodNutrientDto
{
    public string? Id { get; set; }
    public string FoodId { get; set; } = string.Empty;
    public string NutrientId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? DerivationDescription { get; set; }
    public int? DataPoints { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }

    // Denormalized fields for convenience (populated via lookup)
    public string? NutrientName { get; set; }
    public string? NutrientUnit { get; set; }
    public int? NutrientNumber { get; set; }
}

public class CreateFoodNutrientDto
{
    public string FoodId { get; set; } = string.Empty;
    public string NutrientId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? DerivationDescription { get; set; }
    public int? DataPoints { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}

public class UpdateFoodNutrientDto
{
    public double? Amount { get; set; }
    public string? DerivationDescription { get; set; }
    public int? DataPoints { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}

/// <summary>
/// Bulk create food nutrients for a single food
/// </summary>
public class BulkCreateFoodNutrientsDto
{
    public string FoodId { get; set; } = string.Empty;
    public List<FoodNutrientValueDto> Nutrients { get; set; } = new();
}

public class FoodNutrientValueDto
{
    public string NutrientId { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? DerivationDescription { get; set; }
}

/// <summary>
/// Response DTO for food with all its nutrients
/// </summary>
public class FoodWithNutrientsDto
{
    public FoodDto Food { get; set; } = null!;
    public List<FoodNutrientDto> Nutrients { get; set; } = new();
}
