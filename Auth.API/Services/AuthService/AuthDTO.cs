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
    public Guid? Id { get; set; }
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Username { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginResponseDto
{
    public  TokenResponseDto? Tokens { get; set; }
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}
public class NormalResponseDto
{
    public required string Message { get; set; }
    public int? StatusCode { get; set; } = 200;
}

public class NormalResponseWithDataDto<T>
{
    public required string Message { get; set; }
    public int? StatusCode { get; set; } = 200;
    public T? Data { get; set; }
}
public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
public class ChatUser
{
    public  string Email { get; set; }
    public  string Username { get; set; }
}
public class LogoutResponseDto
{
    public string Message { get; set; } = "Logged out successfully";
}
