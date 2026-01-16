using HotChocolate.Authorization;
using System.Security.Claims;

namespace Auth.API.Services.AuthService
{
    [ExtendObjectType("Mutation")]
    public class AuthMutation
    {
        public async Task<NormalResponseDto> RegisterUser(
            RegisterDto input,
            [Service] AuthDatasource authDatasource,
            CancellationToken cancellationToken)
        {
            return await authDatasource.RegisterUserAsync(input, cancellationToken);
        }

       
        public async Task<NormalResponseWithDataDto<LoginResponseDto>> LoginUser(
            LoginDto input,
            [Service] AuthDatasource authDatasource,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            var user = await authDatasource.LoginUserAsync(input, cancellationToken);
            if (user == null)
            {


                return new NormalResponseWithDataDto<LoginResponseDto>
                {
                    Message = "Invalid credentials",
                    StatusCode = 401,
                    Data = null
                };
            }

                if (context == null)
            {

                return new NormalResponseWithDataDto<LoginResponseDto>
                {
                    Message = "Internal Server Error: No HTTP Context",
                    StatusCode = 500
                };
            }

            var platformHeader = context.Request!.Headers["X-Platform"].FirstOrDefault();
            

            var isWeb = string.Equals(platformHeader, "web", StringComparison.OrdinalIgnoreCase);
            //Cookie base Web login
            if (isWeb)
            {
                var resultPayload = await authDatasource.SetCookieAndGetResponsePayloadAsync(user, context);
                return new NormalResponseWithDataDto<LoginResponseDto>
                {
                    Message = "Login successfful. Cookie set",
                    StatusCode = 200,
                    Data =  resultPayload
                };
          
            }
            else 
            {   
                //Authorization base mobile app login (return user tokens and details)
                var tokens = await authDatasource.CreateRefreshTokens(user);
                return new NormalResponseWithDataDto<LoginResponseDto>
                {
                    Message="Login successful",
                    StatusCode=200,
                    Data = new LoginResponseDto 
                    {
                         Tokens = tokens,
                         UserId = user.Id.ToString(),
                         Username = user.Username,
                         Email = user.Email
                    }
                };
            }
               
        }

        [Authorize]
        public async Task<NormalResponseWithDataDto<LogoutResponseDto>> LogoutUser(
            [Service] AuthDatasource authDatasource,
            HttpContext context
        )
        {
            await authDatasource.LogoutAsync(context);

            return new NormalResponseWithDataDto<LogoutResponseDto>
            {
                Message ="Logout successfully",
                StatusCode = 200,
            };
        }

        [Authorize]
        public async Task<NormalResponseDto> UpdateUserDetails(
            ProfileUpdateDto input,
            ClaimsPrincipal user,
            [Service] AuthDatasource authDatasource,
            HttpContext context)
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            Guid userId = Guid.Parse(currentUserId);
            System.Diagnostics.Debug.WriteLine(input);
            return await authDatasource.UpdateUserDetails(input, userId);
        }

        [Authorize]
        public async Task<NormalResponseDto> DeleteUser(
            ClaimsPrincipal user,
            [Service] AuthDatasource authDatasource)
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            Guid userId = Guid.Parse(currentUserId);

            return await authDatasource.DeleteUserAsync(userId);
        }
    }
}
