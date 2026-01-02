using Auth.API.Enums;
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
    public string? Avatar { get; set; }
    public PrefereceDto? Preferences { get; set; }
    public string? Bio { get; set; }

    public UserStatistics UserStats { get; set; }
    


}

    public class UserStatistics
    {
    public int PostCount { get; set; } = 0;
    public int GroupCount { get; set; } = 0;
    }

public class PrefereceDto
{
    public ProfileVisibilityEnum ProfileVisibility { get; set; } = ProfileVisibilityEnum.Public;
    public bool EmailNotification { get; set; } = true;
}




public class ProfileUpdateDto
{

    public  string? Avatar { get; set; }
    public PrefereceDto? Preferences { get; set; }
    public string? Bio { get; set; }
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
    public Guid Id { get; set; }
    public  string Email { get; set; }
    public  string Username { get; set; }
}
public class LogoutResponseDto
{
    public string Message { get; set; } = "Logged out successfully";
}
