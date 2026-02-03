namespace BringTheDiet.Api.DTOs;

public class NutrientDto
{
    public string? Id { get; set; }
    public int NutrientNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
}

public class CreateNutrientDto
{
    public int NutrientNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateNutrientDto
{
    public string? Name { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public int? SortOrder { get; set; }
}
