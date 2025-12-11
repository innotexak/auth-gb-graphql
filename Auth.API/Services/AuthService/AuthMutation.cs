using Auth.API.Data;
using Auth.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services;
[GraphQLName("Mutation")]
public class Mutation
{
    public async Task<NormalResponseDto> RegisterUser(RegisterDto input,    [Service] AuthDBContext dbContext, CancellationToken cancellationToken)
    {
          var entity = new User
        {
            Id = new Guid(),
            Username = input.Username,
            Email = input.Email,
            Password =  new PasswordHasher<RegisterDto>().HashPassword(input, input.Password)
        };
        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new NormalResponseDto { Message = "User registered successfully", StatusCode = 201 };
    }

    public async Task<LoginResponseDto> LoginUser(LoginDto input, [Service] AuthDBContext dbContext, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == input.Username, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var passwordVerificationResult = new PasswordHasher<LoginDto>().VerifyHashedPassword(input, user.Password, input.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new Exception("Invalid password");
        }

        // Generate JWT token (implementation not shown here)
        var token = "GeneratedJWTToken"; // Replace with actual token generation logic

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };
    }
}