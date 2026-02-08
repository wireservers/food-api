namespace BringTheDiet.Api.DTOs;

public class RecipeDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Diet { get; set; }
    public string? DietSlug { get; set; }
    public int? PrepTime { get; set; }
    public int? Calories { get; set; }
    public bool IsFavorite { get; set; }
    public bool Featured { get; set; }
    public string? Description { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecipeIngredientDto
{
    public string Name { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
}

public class CreateRecipeDto
{
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Diet { get; set; }
    public string? DietSlug { get; set; }
    public int? PrepTime { get; set; }
    public int? Calories { get; set; }
    public bool IsFavorite { get; set; }
    public bool Featured { get; set; }
    public string? Description { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Instructions { get; set; }
}

public class UpdateRecipeDto
{
    public string? Title { get; set; }
    public string? Image { get; set; }
    public string? Diet { get; set; }
    public string? DietSlug { get; set; }
    public int? PrepTime { get; set; }
    public int? Calories { get; set; }
    public bool? IsFavorite { get; set; }
    public bool? Featured { get; set; }
    public string? Description { get; set; }
    public List<RecipeIngredientDto>? Ingredients { get; set; }
    public List<string>? Instructions { get; set; }
}
