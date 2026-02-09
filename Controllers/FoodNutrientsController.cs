using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

/// <summary>
/// Manages food-nutrient relationships (which foods contain which nutrients with what values)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FoodNutrientsController : ControllerBase
{
    private readonly IFoodNutrientRepository _repository;
    private readonly INutrientRepository _nutrientRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<FoodNutrientsController> _logger;

    public FoodNutrientsController(
        IFoodNutrientRepository repository,
        INutrientRepository nutrientRepository,
        IFoodRepository foodRepository,
        ILogger<FoodNutrientsController> logger)
    {
        _repository = repository;
        _nutrientRepository = nutrientRepository;
        _foodRepository = foodRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all food-nutrient relationships with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all food-nutrient relationships")]
    [ProducesResponseType(typeof(PaginatedResponse<FoodNutrientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<FoodNutrientDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<FoodNutrientDto>
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
            _logger.LogError(ex, "Error retrieving food nutrients");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all nutrients for a specific food (with nutrient details)
    /// </summary>
    [HttpGet("by-food/{foodId}")]
    [SwaggerOperation(Summary = "Get nutrients for a food", Description = "Returns all nutrient values for a specific food item")]
    [ProducesResponseType(typeof(List<FoodNutrientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FoodNutrientDto>>> GetByFoodId(string foodId)
    {
        try
        {
            var food = await _foodRepository.GetByIdAsync(foodId);
            if (food == null)
                return NotFound($"Food with ID {foodId} not found");

            var foodNutrients = await _repository.GetByFoodIdAsync(foodId);
            var nutrients = await _nutrientRepository.GetAllNoCacheAsync();
            var nutrientMap = nutrients.ToDictionary(n => n.Id!, n => n);

            var result = foodNutrients.Select(fn =>
            {
                var dto = MapToDto(fn);
                if (nutrientMap.TryGetValue(fn.NutrientId, out var nutrient))
                {
                    dto.NutrientName = nutrient.Name;
                    dto.NutrientUnit = nutrient.Unit;
                    dto.NutrientNumber = nutrient.NutrientNumber;
                }
                return dto;
            }).OrderBy(fn => fn.NutrientNumber).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nutrients for food {FoodId}", foodId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a food with all its nutrients in a single response
    /// </summary>
    [HttpGet("food-with-nutrients/{foodId}")]
    [SwaggerOperation(Summary = "Get food with nutrients", Description = "Returns food details along with all its nutrient values")]
    [ProducesResponseType(typeof(FoodWithNutrientsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodWithNutrientsDto>> GetFoodWithNutrients(string foodId)
    {
        try
        {
            var food = await _foodRepository.GetByIdAsync(foodId);
            if (food == null)
                return NotFound($"Food with ID {foodId} not found");

            var foodNutrients = await _repository.GetByFoodIdAsync(foodId);
            var nutrients = await _nutrientRepository.GetAllNoCacheAsync();
            var nutrientMap = nutrients.ToDictionary(n => n.Id!, n => n);

            var result = new FoodWithNutrientsDto
            {
                Food = new FoodDto
                {
                    Id = food.Id,
                    FdcId = food.FdcId,
                    Description = food.Description,
                    FoodClass = food.FoodClass,
                    DataType = food.DataType,
                    BrandOwner = food.BrandOwner,
                    Ingredients = food.Ingredients,
                    ServingSize = food.ServingSize,
                    ServingSizeUnit = food.ServingSizeUnit
                },
                Nutrients = foodNutrients.Select(fn =>
                {
                    var dto = MapToDto(fn);
                    if (nutrientMap.TryGetValue(fn.NutrientId, out var nutrient))
                    {
                        dto.NutrientName = nutrient.Name;
                        dto.NutrientUnit = nutrient.Unit;
                        dto.NutrientNumber = nutrient.NutrientNumber;
                    }
                    return dto;
                }).OrderBy(fn => fn.NutrientNumber).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving food with nutrients {FoodId}", foodId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new food-nutrient relationship
    /// </summary>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(Summary = "Create food-nutrient relationship")]
    [ProducesResponseType(typeof(FoodNutrientDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<FoodNutrientDto>> Create([FromBody] CreateFoodNutrientDto createDto)
    {
        try
        {
            var foodNutrient = new FoodNutrient
            {
                FoodId = createDto.FoodId,
                NutrientId = createDto.NutrientId,
                Amount = createDto.Amount,
                DerivationDescription = createDto.DerivationDescription,
                DataPoints = createDto.DataPoints,
                Min = createDto.Min,
                Max = createDto.Max
            };

            var created = await _repository.CreateAsync(foodNutrient);
            return CreatedAtAction(nameof(GetByFoodId), new { foodId = created.FoodId }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating food nutrient");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Bulk creates food-nutrient relationships for a single food
    /// </summary>
    [Authorize]
    [HttpPost("bulk")]
    [SwaggerOperation(Summary = "Bulk create food nutrients", Description = "Creates multiple nutrient values for a single food")]
    [ProducesResponseType(typeof(List<FoodNutrientDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<List<FoodNutrientDto>>> BulkCreate([FromBody] BulkCreateFoodNutrientsDto bulkDto)
    {
        try
        {
            var food = await _foodRepository.GetByIdAsync(bulkDto.FoodId);
            if (food == null)
                return NotFound($"Food with ID {bulkDto.FoodId} not found");

            var foodNutrients = bulkDto.Nutrients.Select(n => new FoodNutrient
            {
                FoodId = bulkDto.FoodId,
                NutrientId = n.NutrientId,
                Amount = n.Amount,
                DerivationDescription = n.DerivationDescription
            }).ToList();

            var created = await _repository.CreateManyAsync(foodNutrients);
            return CreatedAtAction(nameof(GetByFoodId), new { foodId = bulkDto.FoodId }, created.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating food nutrients");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes all nutrient relationships for a food
    /// </summary>
    [Authorize]
    [HttpDelete("by-food/{foodId}")]
    [SwaggerOperation(Summary = "Delete all nutrients for a food")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteByFoodId(string foodId)
    {
        try
        {
            await _repository.DeleteByFoodIdAsync(foodId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting food nutrients for {FoodId}", foodId);
            return StatusCode(500, "Internal server error");
        }
    }

    private static FoodNutrientDto MapToDto(FoodNutrient fn) => new()
    {
        Id = fn.Id,
        FoodId = fn.FoodId,
        NutrientId = fn.NutrientId,
        Amount = fn.Amount,
        DerivationDescription = fn.DerivationDescription,
        DataPoints = fn.DataPoints,
        Min = fn.Min,
        Max = fn.Max
    };
}
