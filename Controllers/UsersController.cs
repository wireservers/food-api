using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repository, ILogger<UsersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize);
            var response = new PaginatedResponse<UserDto>
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
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        try
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
                return NotFound($"User with email {email} not found");

            return Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createDto)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _repository.GetByEmailAsync(createDto.Email);
            if (existingUser != null)
                return Conflict("Email already exists");

            var user = new User
            {
                OidcSub = createDto.OidcSub,
                Email = createDto.Email,
                DisplayName = createDto.DisplayName,
                Roles = createDto.Roles
            };

            var created = await _repository.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"User with ID {id} not found");

            if (updateDto.Email != null) existing.Email = updateDto.Email;
            if (updateDto.DisplayName != null) existing.DisplayName = updateDto.DisplayName;
            if (updateDto.Roles != null) existing.Roles = updateDto.Roles;
            if (updateDto.Verified.HasValue) existing.Verified = updateDto.Verified.Value;

            var success = await _repository.UpdateAsync(id, existing);
            if (!success)
                return StatusCode(500, "Failed to update user");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Id}", id);
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
                return NotFound($"User with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        OidcSub = user.OidcSub,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Roles = user.Roles,
        Verified = user.Verified,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
