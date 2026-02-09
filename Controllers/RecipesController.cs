using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create([FromBody] CreateRecipeDto createDto)
    {
        try
        {
            var recipe = new Recipe
            {
                Title = createDto.Title,
                Image = createDto.Image,
                Diet = createDto.Diet,
                DietSlug = createDto.DietSlug ?? createDto.Diet?.ToLower().Replace(" ", "-"),
                PrepTime = createDto.PrepTime,
                Calories = createDto.Calories,
                IsFavorite = createDto.IsFavorite,
                Featured = createDto.Featured,
                Description = createDto.Description,
                Ingredients = createDto.Ingredients?.Select(i => new RecipeIngredient
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Notes = i.Notes
                }).ToList(),
                Instructions = createDto.Instructions,
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

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateRecipeDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Recipe with ID {id} not found");

            if (updateDto.Title != null) existing.Title = updateDto.Title;
            if (updateDto.Image != null) existing.Image = updateDto.Image;
            if (updateDto.Diet != null) existing.Diet = updateDto.Diet;
            if (updateDto.DietSlug != null) existing.DietSlug = updateDto.DietSlug;
            if (updateDto.PrepTime.HasValue) existing.PrepTime = updateDto.PrepTime;
            if (updateDto.Calories.HasValue) existing.Calories = updateDto.Calories;
            if (updateDto.IsFavorite.HasValue) existing.IsFavorite = updateDto.IsFavorite.Value;
            if (updateDto.Featured.HasValue) existing.Featured = updateDto.Featured.Value;
            if (updateDto.Description != null) existing.Description = updateDto.Description;
            if (updateDto.Ingredients != null) existing.Ingredients = updateDto.Ingredients.Select(i => new RecipeIngredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Notes = i.Notes
            }).ToList();
            if (updateDto.Instructions != null) existing.Instructions = updateDto.Instructions;

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

    [Authorize]
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
        Image = recipe.Image,
        Diet = recipe.Diet,
        DietSlug = recipe.DietSlug,
        PrepTime = recipe.PrepTime,
        Calories = recipe.Calories,
        IsFavorite = recipe.IsFavorite,
        Featured = recipe.Featured,
        Description = recipe.Description,
        Ingredients = recipe.Ingredients?.Select(i => new RecipeIngredientDto
        {
            Name = i.Name,
            Quantity = i.Quantity,
            Unit = i.Unit,
            Notes = i.Notes
        }).ToList(),
        Instructions = recipe.Instructions,
        CreatedAt = recipe.CreatedAt,
        UpdatedAt = recipe.UpdatedAt
    };
}
