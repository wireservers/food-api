using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

/// <summary>
/// Manages food items and nutritional data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FoodsController : ControllerBase
{
    private readonly IFoodRepository _repository;
    private readonly ILogger<FoodsController> _logger;

    public FoodsController(IFoodRepository repository, ILogger<FoodsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all food items with pagination
    /// </summary>
    /// <param name="page">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>A paginated list of foods</returns>
    /// <response code="200">Returns the paginated list of foods</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all foods", Description = "Retrieves a paginated list of all food items")]
    [ProducesResponseType(typeof(PaginatedResponse<FoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<FoodDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<FoodDto>
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
            _logger.LogError(ex, "Error retrieving foods");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message, stack = ex.StackTrace });
        }
    }

    /// <summary>
    /// Retrieves a specific food by ID
    /// </summary>
    /// <param name="id">The MongoDB ObjectId of the food item</param>
    /// <returns>The requested food item</returns>
    /// <response code="200">Returns the food item</response>
    /// <response code="404">Food not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get food by ID", Description = "Retrieves a specific food item by its unique identifier")]
    [ProducesResponseType(typeof(FoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FoodDto>> GetById(string id)
    {
        try
        {
            var food = await _repository.GetByIdAsync(id);
            if (food == null)
                return NotFound($"Food with ID {id} not found");

            return Ok(MapToDto(food));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving food {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Searches for foods by description
    /// </summary>
    /// <param name="term">Search term to match against food descriptions</param>
    /// <returns>List of matching food items</returns>
    /// <response code="200">Returns matching foods</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search foods", Description = "Searches for food items by description using partial text matching")]
    [ProducesResponseType(typeof(List<FoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<FoodDto>>> Search([FromQuery] string term)
    {
        try
        {
            var foods = await _repository.SearchByDescriptionAsync(term);
            var foodDtos = foods.Select(MapToDto).ToList();
            return Ok(foodDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching foods");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new food item
    /// </summary>
    /// <param name="createDto">Food data for creation</param>
    /// <returns>The newly created food item</returns>
    /// <response code="200">Food created successfully</response>
    /// <response code="500">Internal server error</response>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(Summary = "Create food", Description = "Creates a new food item in the database")]
    [ProducesResponseType(typeof(FoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FoodDto>> Create([FromBody] CreateFoodDto createDto)
    {
        try
        {
            var food = new Food
            {
                FdcId = createDto.FdcId,
                FoodClass = createDto.FoodClass,
                DataType = createDto.DataType,
                Description = createDto.Description,
                BrandOwner = createDto.BrandOwner,
                BrandedFoodCategory = createDto.BrandedFoodCategory,
                GtinUpc = createDto.GtinUpc,
                MarketCountry = createDto.MarketCountry,
                Ingredients = createDto.Ingredients,
                ServingSize = createDto.ServingSize,
                ServingSizeUnit = createDto.ServingSizeUnit,
                HouseholdServingFullText = createDto.HouseholdServingFullText,
                PublicationDate = createDto.PublicationDate,
                TradeChannels = createDto.TradeChannels
            };

            var created = await _repository.CreateAsync(food);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating food");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateFoodDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Food with ID {id} not found");

            if (updateDto.Description != null) existing.Description = updateDto.Description;
            if (updateDto.BrandOwner != null) existing.BrandOwner = updateDto.BrandOwner;
            if (updateDto.Ingredients != null) existing.Ingredients = updateDto.Ingredients;
            if (updateDto.ServingSize.HasValue) existing.ServingSize = updateDto.ServingSize;
            if (updateDto.ServingSizeUnit != null) existing.ServingSizeUnit = updateDto.ServingSizeUnit;

            var success = await _repository.UpdateAsync(id, existing);
            if (!success)
                return StatusCode(500, "Failed to update food");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating food {Id}", id);
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
                return NotFound($"Food with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting food {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static FoodDto MapToDto(Food food) => new()
    {
        Id = food.Id,
        FdcId = food.FdcId,
        FoodClass = food.FoodClass,
        DataType = food.DataType,
        Description = food.Description,
        BrandOwner = food.BrandOwner,
        BrandedFoodCategory = food.BrandedFoodCategory,
        GtinUpc = food.GtinUpc,
        MarketCountry = food.MarketCountry,
        Ingredients = food.Ingredients,
        ServingSize = food.ServingSize,
        ServingSizeUnit = food.ServingSizeUnit,
        HouseholdServingFullText = food.HouseholdServingFullText,
        PublicationDate = food.PublicationDate,
        AvailableDate = food.AvailableDate,
        ModifiedDate = food.ModifiedDate,
        TradeChannels = food.TradeChannels
    };
}
