namespace BringTheDiet.Api.DTOs;

public class BlogPostDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
    public string Category { get; set; } = "Nutrition";
    public string Author { get; set; } = string.Empty;
    public int ReadTime { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateBlogPostDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
    public string Category { get; set; } = "Nutrition";
    public string Author { get; set; } = string.Empty;
    public int ReadTime { get; set; }
    public bool Published { get; set; } = true;
}

public class UpdateBlogPostDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public string? Author { get; set; }
    public int? ReadTime { get; set; }
    public bool? Published { get; set; }
}
