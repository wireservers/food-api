namespace BringTheDiet.Api.DTOs;

public class UserDto
{
    public string? Id { get; set; }
    public string? OidcSub { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public List<string>? Roles { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public string? OidcSub { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public List<string>? Roles { get; set; }
}

public class UpdateUserDto
{
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public List<string>? Roles { get; set; }
    public bool? Verified { get; set; }
}
