using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Auth.API.Services.AuthService
{
    [ExtendObjectType(Name = "Mutation")]
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

    }
}
