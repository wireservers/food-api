using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DietsController : ControllerBase
{
    private readonly IDietTypeRepository _repository;
    private readonly ILogger<DietsController> _logger;

    public DietsController(IDietTypeRepository repository, ILogger<DietsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<DietTypeDto>>> GetAll()
    {
        try
        {
            var diets = await _repository.GetAllAsync();
            return Ok(diets.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diet types");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<DietTypeDto>> GetBySlug(string slug)
    {
        try
        {
            var diet = await _repository.GetBySlugAsync(slug);
            if (diet == null)
                return NotFound($"Diet with slug '{slug}' not found");

            return Ok(MapToDto(diet));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diet {Slug}", slug);
            return StatusCode(500, "Internal server error");
        }
    }

    private static DietTypeDto MapToDto(DietType diet) => new()
    {
        Id = diet.Id,
        Name = diet.Name,
        Icon = diet.Icon,
        RecipeCount = diet.RecipeCount,
        Color = diet.Color,
        Slug = diet.Slug,
        Description = diet.Description,
        Category = diet.Category,
        CreatedAt = diet.CreatedAt,
        UpdatedAt = diet.UpdatedAt
    };
}
