using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BringTheDiet.Api.DTOs;
using BringTheDiet.Api.Models;
using BringTheDiet.Api.Repositories;

namespace BringTheDiet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MeController> _logger;

    public MeController(IUserRepository userRepository, ILogger<MeController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns the current authenticated user's profile.
    /// Creates a new user record if this is their first sign-in.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            // Extract claims from the JWT token
            var oidcSub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("oid")
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(oidcSub))
                return Unauthorized("No subject claim found in token");

            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("preferred_username")
                ?? User.FindFirstValue("email")
                ?? "";

            var displayName = User.FindFirstValue("name")
                ?? User.FindFirstValue(ClaimTypes.GivenName)
                ?? email;

            // Look up existing user by OidcSub
            var user = await _userRepository.GetByOidcSubAsync(oidcSub);

            if (user == null)
            {
                // First sign-in: create user record
                _logger.LogInformation("Creating new user for OidcSub {Sub}", oidcSub);
                user = new User
                {
                    OidcSub = oidcSub,
                    Email = email,
                    DisplayName = displayName,
                    Roles = new List<string> { "user" },
                    Verified = true
                };
                user = await _userRepository.CreateAsync(user);
            }
            else
            {
                // Update profile info from token on each login
                var changed = false;
                if (!string.IsNullOrEmpty(email) && user.Email != email)
                {
                    user.Email = email;
                    changed = true;
                }
                if (!string.IsNullOrEmpty(displayName) && user.DisplayName != displayName)
                {
                    user.DisplayName = displayName;
                    changed = true;
                }
                if (changed)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user.Id!, user);
                }
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                OidcSub = user.OidcSub,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Roles = user.Roles,
                Verified = user.Verified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return StatusCode(500, "Internal server error");
        }
    }
}
