using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeRepository _repository;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(IRecipeRepository repository, ILogger<RecipesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<RecipeDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<RecipeDto>
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
            _logger.LogError(ex, "Error retrieving recipes");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeDto>> GetById(string id)
    {
        try
        {
            var recipe = await _repository.GetByIdAsync(id);
            if (recipe == null)
                return NotFound($"Recipe with ID {id} not found");

            return Ok(MapToDto(recipe));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipe {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<RecipeDto>>> Search([FromQuery] string term)
    {
        try
        {
            var recipes = await _repository.SearchByNameAsync(term);
            var recipeDtos = recipes.Select(MapToDto).ToList();
            return Ok(recipeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching recipes");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create([FromBody] CreateRecipeDto createDto)
    {
        try
        {
            var recipe = new Recipe
            {
                Title = createDto.Title,
                Slug = createDto.Slug ?? createDto.Title.ToLower().Replace(" ", "-"),
                Summary = createDto.Summary,
                DietTags = createDto.DietTags,
                Cuisine = createDto.Cuisine,
                PrepMinutes = createDto.PrepMinutes,
                CookMinutes = createDto.CookMinutes,
                Servings = createDto.Servings,
                Difficulty = createDto.Difficulty,
                Ingredients = createDto.Ingredients?.Select(i => new RecipeIngredient
                {
                    FdcId = i.FdcId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Notes = i.Notes
                }).ToList(),
                Steps = createDto.Steps,
                Images = createDto.Images,
                Status = createDto.Status ?? "draft",
                Nutrition = createDto.Nutrition != null ? new RecipeNutrition
                {
                    Calories = createDto.Nutrition.Calories,
                    Protein = createDto.Nutrition.Protein,
                    Carbs = createDto.Nutrition.Carbs,
                    Fat = createDto.Nutrition.Fat
                } : null
            };

            var created = await _repository.CreateAsync(recipe);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateRecipeDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Recipe with ID {id} not found");

            if (updateDto.Title != null) existing.Title = updateDto.Title;
            if (updateDto.Summary != null) existing.Summary = updateDto.Summary;
            if (updateDto.DietTags != null) existing.DietTags = updateDto.DietTags;
            if (updateDto.Cuisine != null) existing.Cuisine = updateDto.Cuisine;
            if (updateDto.PrepMinutes.HasValue) existing.PrepMinutes = updateDto.PrepMinutes;
            if (updateDto.CookMinutes.HasValue) existing.CookMinutes = updateDto.CookMinutes;
            if (updateDto.Servings.HasValue) existing.Servings = updateDto.Servings;
            if (updateDto.Difficulty != null) existing.Difficulty = updateDto.Difficulty;
            if (updateDto.Ingredients != null)
            {
                existing.Ingredients = updateDto.Ingredients.Select(i => new RecipeIngredient
                {
                    FdcId = i.FdcId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Notes = i.Notes
                }).ToList();
            }
            if (updateDto.Steps != null) existing.Steps = updateDto.Steps;
            if (updateDto.Images != null) existing.Images = updateDto.Images;
            if (updateDto.Status != null) existing.Status = updateDto.Status;

            var success = await _repository.UpdateAsync(id, existing);
            if (!success)
                return StatusCode(500, "Failed to update recipe");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipe {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound($"Recipe with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static RecipeDto MapToDto(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Title = recipe.Title,
        Slug = recipe.Slug,
        Summary = recipe.Summary,
        DietTags = recipe.DietTags,
        Cuisine = recipe.Cuisine,
        PrepMinutes = recipe.PrepMinutes,
        CookMinutes = recipe.CookMinutes,
        Servings = recipe.Servings,
        Difficulty = recipe.Difficulty,
        Ingredients = recipe.Ingredients?.Select(i => new RecipeIngredientDto
        {
            FdcId = i.FdcId,
            Name = i.Name,
            Quantity = i.Quantity,
            Unit = i.Unit,
            Notes = i.Notes
        }).ToList(),
        Steps = recipe.Steps,
        Images = recipe.Images,
        Status = recipe.Status,
        Nutrition = recipe.Nutrition != null ? new RecipeNutritionDto
        {
            Calories = recipe.Nutrition.Calories,
            Protein = recipe.Nutrition.Protein,
            Carbs = recipe.Nutrition.Carbs,
            Fat = recipe.Nutrition.Fat,
            Nutrients = recipe.Nutrition.Nutrients?.Select(n => new NutrientInfoDto
            {
                Name = n.Name,
                Amount = n.Amount,
                Unit = n.Unit
            }).ToList()
        } : null,
        PublishedAt = recipe.PublishedAt,
        Verified = recipe.Verified,
        CreatedAt = recipe.CreatedAt,
        UpdatedAt = recipe.UpdatedAt
    };
}
