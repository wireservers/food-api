namespace BringTheDiet.Api.DTOs;

public class MealPlanDto
{
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? WeekStart { get; set; }
    public List<MealPlanEntryDto>? Entries { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MealPlanEntryDto
{
    public string? Day { get; set; }
    public string? MealType { get; set; }
    public string? RecipeId { get; set; }
    public int? Servings { get; set; }
    public string? Notes { get; set; }
}

public class CreateMealPlanDto
{
    public string UserId { get; set; } = string.Empty;
    public string? WeekStart { get; set; }
    public List<MealPlanEntryDto>? Entries { get; set; }
}

public class UpdateMealPlanDto
{
    public string? WeekStart { get; set; }
    public List<MealPlanEntryDto>? Entries { get; set; }
    public bool? Verified { get; set; }
}
