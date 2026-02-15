using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : ControllerBase
{
    private readonly IBlogPostRepository _repository;
    private readonly ILogger<BlogPostsController> _logger;

    public BlogPostsController(IBlogPostRepository repository, ILogger<BlogPostsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<BlogPostDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, category);
            var response = new PaginatedResponse<BlogPostDto>
            {
                Items = items.Select(MapToDto).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog posts");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPostDto>> GetById(string id)
    {
        try
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                return NotFound($"Blog post with ID {id} not found");

            return Ok(MapToDto(post));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog post {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<BlogPostDto>> GetBySlug(string slug)
    {
        try
        {
            var post = await _repository.GetBySlugAsync(slug);
            if (post == null)
                return NotFound($"Blog post with slug '{slug}' not found");

            return Ok(MapToDto(post));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blog post by slug {Slug}", slug);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<BlogPostDto>>> Search([FromQuery] string term)
    {
        try
        {
            var posts = await _repository.SearchAsync(term);
            return Ok(posts.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching blog posts");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BlogPostDto>> Create([FromBody] CreateBlogPostDto createDto)
    {
        try
        {
            var post = new BlogPost
            {
                Title = createDto.Title,
                Slug = createDto.Slug ?? GenerateSlug(createDto.Title),
                Excerpt = createDto.Excerpt,
                Content = createDto.Content,
                Image = createDto.Image,
                Category = createDto.Category,
                Author = createDto.Author,
                ReadTime = createDto.ReadTime,
                Published = createDto.Published,
            };

            var created = await _repository.CreateAsync(post);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating blog post");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateBlogPostDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Blog post with ID {id} not found");

            if (updateDto.Title != null) existing.Title = updateDto.Title;
            if (updateDto.Slug != null) existing.Slug = updateDto.Slug;
            if (updateDto.Excerpt != null) existing.Excerpt = updateDto.Excerpt;
            if (updateDto.Content != null) existing.Content = updateDto.Content;
            if (updateDto.Image != null) existing.Image = updateDto.Image;
            if (updateDto.Category != null) existing.Category = updateDto.Category;
            if (updateDto.Author != null) existing.Author = updateDto.Author;
            if (updateDto.ReadTime.HasValue) existing.ReadTime = updateDto.ReadTime.Value;
            if (updateDto.Published.HasValue) existing.Published = updateDto.Published.Value;

            var success = await _repository.UpdateAsync(id, existing);
            if (!success)
                return StatusCode(500, "Failed to update blog post");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating blog post {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound($"Blog post with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static BlogPostDto MapToDto(BlogPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Excerpt = post.Excerpt,
        Content = post.Content,
        Image = post.Image,
        Category = post.Category,
        Author = post.Author,
        ReadTime = post.ReadTime,
        Published = post.Published,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
    };

    private static string GenerateSlug(string title) =>
        System.Text.RegularExpressions.Regex
            .Replace(title.ToLower().Trim(), @"[^a-z0-9\s-]", "")
            .Replace(" ", "-")
            .Trim('-');
}
