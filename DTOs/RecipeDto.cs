namespace BringTheDiet.Api.DTOs;

public class RecipeDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public List<string>? DietTags { get; set; }
    public string? Cuisine { get; set; }
    public int? PrepMinutes { get; set; }
    public int? CookMinutes { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Steps { get; set; }
    public List<string>? Images { get; set; }
    public string? Status { get; set; }
    public RecipeNutritionDto? Nutrition { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecipeIngredientDto
{
    public int? FdcId { get; set; }
    public string? Name { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
}

public class RecipeNutritionDto
{
    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Carbs { get; set; }
    public double? Fat { get; set; }
    public List<NutrientInfoDto>? Nutrients { get; set; }
}

public class NutrientInfoDto
{
    public string? Name { get; set; }
    public double? Amount { get; set; }
    public string? Unit { get; set; }
}

public class CreateRecipeDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public List<string>? DietTags { get; set; }
    public string? Cuisine { get; set; }
    public int? PrepMinutes { get; set; }
    public int? CookMinutes { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Steps { get; set; }
    public List<string>? Images { get; set; }
    public string? Status { get; set; }
    public RecipeNutritionDto? Nutrition { get; set; }
}

public class UpdateRecipeDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public List<string>? DietTags { get; set; }
    public string? Cuisine { get; set; }
    public int? PrepMinutes { get; set; }
    public int? CookMinutes { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Steps { get; set; }
    public List<string>? Images { get; set; }
    public string? Status { get; set; }
    public RecipeNutritionDto? Nutrition { get; set; }
}
