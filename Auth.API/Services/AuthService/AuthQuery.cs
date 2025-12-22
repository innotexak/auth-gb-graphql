using Auth.API.Entities;
using Auth.API.Services.AuthService;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;


namespace Auth.API.Services.AuthService
{
    [ExtendObjectType(Name = "Query")]
    public class AuthQuery
    {
        [Authorize]
        public async Task<ProfileDto> GetProfile(
          ClaimsPrincipal claimsPrincipal,
          [Service] AuthDatasource authDatasource,
          [Service] IHttpContextAccessor httpContextAccessor,
          CancellationToken cancellationToken)
        {

            if (claimsPrincipal?.Identity?.IsAuthenticated != true)
            {
                throw new GraphQLException("User is not authenticated.");
            }

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("User information not found in token.");
            }
            return await authDatasource.GetProfileAsync(userId, cancellationToken);
        }

        public async Task<List<User>> GetAllUsers(
      ClaimsPrincipal claimsPrincipal, 
      [Service] AuthDatasource authDatasource)
        {
           
            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

           
            return await authDatasource.GetAllUsersAsync(userId);
        }
    }
}
