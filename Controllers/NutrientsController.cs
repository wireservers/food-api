using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

/// <summary>
/// Manages nutrient definitions (master list of nutrient types)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NutrientsController : ControllerBase
{
    private readonly INutrientRepository _repository;
    private readonly ILogger<NutrientsController> _logger;

    public NutrientsController(INutrientRepository repository, ILogger<NutrientsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all nutrients with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all nutrients", Description = "Retrieves a paginated list of all nutrient definitions")]
    [ProducesResponseType(typeof(PaginatedResponse<NutrientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<NutrientDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 100;
            if (pageSize > 500) pageSize = 500;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<NutrientDto>
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
            _logger.LogError(ex, "Error retrieving nutrients");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific nutrient by ID
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get nutrient by ID")]
    [ProducesResponseType(typeof(NutrientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NutrientDto>> GetById(string id)
    {
        try
        {
            var nutrient = await _repository.GetByIdAsync(id);
            if (nutrient == null)
                return NotFound($"Nutrient with ID {id} not found");

            return Ok(MapToDto(nutrient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nutrient {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Retrieves a nutrient by its FDA nutrient number
    /// </summary>
    [HttpGet("by-number/{nutrientNumber}")]
    [SwaggerOperation(Summary = "Get nutrient by FDA number")]
    [ProducesResponseType(typeof(NutrientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NutrientDto>> GetByNutrientNumber(int nutrientNumber)
    {
        try
        {
            var nutrient = await _repository.GetByNutrientNumberAsync(nutrientNumber);
            if (nutrient == null)
                return NotFound($"Nutrient with number {nutrientNumber} not found");

            return Ok(MapToDto(nutrient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nutrient by number {Number}", nutrientNumber);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Searches for nutrients by name
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search nutrients")]
    [ProducesResponseType(typeof(List<NutrientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NutrientDto>>> Search([FromQuery] string term)
    {
        try
        {
            var nutrients = await _repository.SearchByNameAsync(term);
            return Ok(nutrients.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching nutrients");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Creates a new nutrient definition
    /// </summary>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(Summary = "Create nutrient")]
    [ProducesResponseType(typeof(NutrientDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<NutrientDto>> Create([FromBody] CreateNutrientDto createDto)
    {
        try
        {
            var nutrient = new Nutrient
            {
                NutrientNumber = createDto.NutrientNumber,
                Name = createDto.Name,
                Unit = createDto.Unit,
                Category = createDto.Category,
                SortOrder = createDto.SortOrder
            };

            var created = await _repository.CreateAsync(nutrient);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating nutrient");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates an existing nutrient definition
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update nutrient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateNutrientDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Nutrient with ID {id} not found");

            if (updateDto.Name != null) existing.Name = updateDto.Name;
            if (updateDto.Unit != null) existing.Unit = updateDto.Unit;
            if (updateDto.Category != null) existing.Category = updateDto.Category;
            if (updateDto.SortOrder.HasValue) existing.SortOrder = updateDto.SortOrder.Value;

            await _repository.UpdateAsync(id, existing);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating nutrient {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a nutrient definition
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete nutrient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                return NotFound($"Nutrient with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting nutrient {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static NutrientDto MapToDto(Nutrient nutrient) => new()
    {
        Id = nutrient.Id,
        NutrientNumber = nutrient.NutrientNumber,
        Name = nutrient.Name,
        Unit = nutrient.Unit,
        Category = nutrient.Category,
        SortOrder = nutrient.SortOrder
    };
}
