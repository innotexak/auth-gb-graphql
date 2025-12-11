using System.ComponentModel.DataAnnotations;

namespace Auth.API.Services;

// Registration DTO
public class RegisterDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    public required string Email { get; set; }
}

// Login DTO
public class LoginDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

// Profile DTO
public class ProfileDto
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Username { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}
public class NormalResponseDto
{
    public required string Message { get; set; }
    public int? StatusCode { get; set; } = 200;
    public Object? Data { get; set; }
}
 