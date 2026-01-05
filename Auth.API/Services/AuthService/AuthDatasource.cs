using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Enums;
using Auth.API.ErrorHandling.Exceptions;
using Auth.API.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auth.API.Services.AuthService
{
    public class AuthDatasource
    {
        private readonly AuthDBContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;


        public AuthDatasource(AuthDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
        }

        public async Task<NormalResponseDto> RegisterUserAsync(RegisterDto input, CancellationToken cancellationToken)
        {
            // Check if username or email already exists
            if (await _dbContext.Users.AnyAsync(u => u.Username == input.Username || u.Email == input.Email, cancellationToken))
            {
                return new NormalResponseDto { Message = "Username or Email already exists", StatusCode = 400 };
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = input.Username,
                Email = input.Email
            };

            user.Password = _passwordHasher.HashPassword(user, input.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new NormalResponseDto
            {
                Message = "User registered successfully",
                StatusCode = 201
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var ring = RandomNumberGenerator.Create();
            ring.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<TokenResponseDto> CreateRefreshTokens(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = new AuthHelpers(_configuration).CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };


        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var tokens = await _dbContext.Authentication
                .Where(a => a.UserId == user.Id)
                .OrderBy(a => a.RefreshTokenExpiryTime)
                .ToListAsync();

            var newRefreshToken = GenerateRefreshToken();
            var expiry = DateTime.UtcNow.AddDays(7);

            if (tokens.Count < 2)
            {
                // Create new token
                var authentication = new Auth.API.Entities.Authentication
                {
                    UserId = user.Id,
                    Role = "User",
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiryTime = expiry
                };

                _dbContext.Authentication.Add(authentication);
            }
            else
            {
                // Update the oldest token
                var tokenToUpdate = tokens.First();

                tokenToUpdate.RefreshToken = newRefreshToken;
                tokenToUpdate.RefreshTokenExpiryTime = expiry;

                _dbContext.Authentication.Update(tokenToUpdate);
            }

            await _dbContext.SaveChangesAsync();
            return newRefreshToken;
        }
        private async Task<User?> ValidateRefreshTokenAsync(
                Guid userId,
                string refreshToken)
        {
            var auth = await _dbContext.Authentication
                .Include(a => a.User)
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.RefreshToken == refreshToken &&
                    a.RefreshTokenExpiryTime > DateTime.UtcNow
                );

            return auth?.User;
        }

        public async Task<User?> LoginUserAsync(LoginDto input, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == input.Username);

            if (user == null)
            {
                return null;
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new UserInputException("Invalid password");
            }
          return user;
        }

        public async Task<LoginResponseDto> SetCookieAndGetResponsePayloadAsync(User user, HttpContext httpContext)
        {

      
            var principal = GenerateClaimsPrincipal(user);
            var authProperties = new AuthenticationProperties
            {
  
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            };

            await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return new LoginResponseDto
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

        }

        private ClaimsPrincipal GenerateClaimsPrincipal(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return new ClaimsPrincipal(identity);
        }

        public async Task<ProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken)
        {
            // Parse userId to Guid if stored as Guid in database
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new NotFoundException("Invalid user ID format");
            }

            // Fetch user with related data
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userGuid, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            // Get user statistics (Posts count)
            var postCount = await _dbContext.Posts
                .AsNoTracking()
                .CountAsync(p => p.UserId == userGuid, cancellationToken);

            // Get user statistics (Groups joined count)
            var groupCount = await _dbContext.Groups
                .AsNoTracking()
                .CountAsync(ug => ug.UserId == userGuid, cancellationToken);

            // Get user preferences

            var preferences = new PrefereceDto
            {
                ProfileVisibility = user.Preferences != null
            ? user.Preferences.ProfileVisibility
            : ProfileVisibilityEnum.Public,

                EmailNotification = user.Preferences != null
            ? user.Preferences.EmailNotification
            : true
            };

            // Return complete profile DTO
            return new ProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                Avatar = user.Avatar,
                Preferences = preferences,
                UserStats = new UserStatistics
                {
                    PostCount = postCount,
                    GroupCount = groupCount
                }
            };
        }

        public async Task<List<User>> GetAllUsersAsync(string? excludeUserId )
        {
            var query = _dbContext.Users.AsQueryable();
           
            if (excludeUserId != null)
            {
                
                var currentUserId = new Guid(excludeUserId);
                query = query.Where(u => u.Id != currentUserId);
            }

            return await query
                .OrderBy(u => u.Username)
                .ToListAsync();
        }


        //Update user details
        public async Task<NormalResponseDto> UpdateUserDetails(ProfileUpdateDto input, Guid userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Preferences) // make sure EF loads it
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new NormalResponseDto { Message = "User not found", StatusCode = 404 };

            if (!string.IsNullOrEmpty(input.FirstName)) user.FirstName = input.FirstName;
            if (!string.IsNullOrEmpty(input.LastName)) user.LastName = input.LastName;
            if (!string.IsNullOrEmpty(input.Avatar)) user.Avatar = input.Avatar;
            if (!string.IsNullOrEmpty(input.Bio)) user.Bio = input.Bio;

            if (input.Preferences != null)
            {
                if (user.Preferences == null)
                {
                    user.Preferences = new PrefereceDto(); // create if missing
                    _dbContext.Entry(user).Reference(u => u.Preferences).IsModified = true;
                }

                // copy over individual preference fields
                user.Preferences.EmailNotification = input.Preferences.EmailNotification;
                user.Preferences.ProfileVisibility = input.Preferences.ProfileVisibility;

                // mark the navigation as modified
                _dbContext.Entry(user).Reference(u => u.Preferences).IsModified = true;
            }

            await _dbContext.SaveChangesAsync();

            return new NormalResponseDto { Message = "Profile updated successfully", StatusCode = 200 };
        }

        //Delete a user
        public async Task<NormalResponseDto> DeleteUserAsync(Guid UserId)
        {
            var user = await _dbContext.Users.FindAsync(UserId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                return new NormalResponseDto { Message = "User deleted successfully", StatusCode = 200 };
            }

            return new NormalResponseDto { Message = "User not found", StatusCode = 404 };
        }

    }
}
