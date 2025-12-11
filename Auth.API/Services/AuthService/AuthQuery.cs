using Auth.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Services;

[GraphQLName("Query")]
public class Query
{
    public async Task<ProfileDto> GetProfile(string userId, [Service] AuthDBContext dbContext, CancellationToken cancellationToken)
    {

    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        return new ProfileDto
        {
            Username = user.Username,
            Email = user.Email
        };
    }
}