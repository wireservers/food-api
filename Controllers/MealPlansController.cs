using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealPlansController : ControllerBase
{
    private readonly IMealPlanRepository _repository;
    private readonly ILogger<MealPlansController> _logger;

    public MealPlansController(IMealPlanRepository repository, ILogger<MealPlansController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MealPlanDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<MealPlanDto>
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
            _logger.LogError(ex, "Error retrieving meal plans");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MealPlanDto>> GetById(string id)
    {
        try
        {
            var mealPlan = await _repository.GetByIdAsync(id);
            if (mealPlan == null)
                return NotFound($"Meal plan with ID {id} not found");

            return Ok(MapToDto(mealPlan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving meal plan {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<MealPlanDto>>> GetByUserId(string userId)
    {
        try
        {
            var mealPlans = await _repository.GetByUserIdAsync(userId);
            var mealPlanDtos = mealPlans.Select(MapToDto).ToList();
            return Ok(mealPlanDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving meal plans for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<MealPlanDto>> Create([FromBody] CreateMealPlanDto createDto)
    {
        try
        {
            var mealPlan = new MealPlan
            {
                UserId = createDto.UserId,
                WeekStart = createDto.WeekStart,
                Entries = createDto.Entries?.Select(e => new MealPlanEntry
                {
                    Day = e.Day,
                    MealType = e.MealType,
                    RecipeId = e.RecipeId,
                    Servings = e.Servings,
                    Notes = e.Notes
                }).ToList()
            };

            var created = await _repository.CreateAsync(mealPlan);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meal plan");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateMealPlanDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Meal plan with ID {id} not found");

            if (updateDto.WeekStart != null) existing.WeekStart = updateDto.WeekStart;
            if (updateDto.Verified.HasValue) existing.Verified = updateDto.Verified.Value;
            if (updateDto.Entries != null)
            {
                existing.Entries = updateDto.Entries.Select(e => new MealPlanEntry
                {
                    Day = e.Day,
                    MealType = e.MealType,
                    RecipeId = e.RecipeId,
                    Servings = e.Servings,
                    Notes = e.Notes
                }).ToList();
            }

            var success = await _repository.UpdateAsync(id, existing);
            if (!success)
                return StatusCode(500, "Failed to update meal plan");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meal plan {Id}", id);
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
                return NotFound($"Meal plan with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meal plan {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static MealPlanDto MapToDto(MealPlan mealPlan) => new()
    {
        Id = mealPlan.Id,
        UserId = mealPlan.UserId,
        WeekStart = mealPlan.WeekStart,
        Entries = mealPlan.Entries?.Select(e => new MealPlanEntryDto
        {
            Day = e.Day,
            MealType = e.MealType,
            RecipeId = e.RecipeId,
            Servings = e.Servings,
            Notes = e.Notes
        }).ToList(),
        Verified = mealPlan.Verified,
        CreatedAt = mealPlan.CreatedAt,
        UpdatedAt = mealPlan.UpdatedAt
    };
}
