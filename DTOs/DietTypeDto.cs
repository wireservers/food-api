namespace BringTheDiet.Api.DTOs;

public class DietTypeDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int RecipeCount { get; set; }
    public string? Color { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
