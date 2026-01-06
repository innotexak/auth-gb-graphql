using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.API.Entities;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Auth.API.Helpers
{
    public class AuthHelpers
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly string _authCookieName;

        // Constructor for both JWT and HttpClient logic
        public AuthHelpers(
            IConfiguration configuration,
            IHttpClientFactory? httpClientFactory = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _authCookieName = "xx-auth-cookie";
        }

        #region JWT Logic (Mobile)
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var secret = _configuration.GetValue<string>("AppSettings:JWT_SECRET")
                         ?? throw new InvalidOperationException("JWT Secret not found.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        #endregion

        #region Razor Pages / GraphQL Loopback Logic
        public HttpClient GetAuthenticatedClient()
        {
            if (_httpClientFactory == null || _httpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("HttpClientFactory or HttpContext is not available.");
            }

            var context = _httpContextAccessor.HttpContext;
            var client = _httpClientFactory.CreateClient();

            // Set Base Address to the current running server
            client.BaseAddress = new Uri($"{context.Request.Scheme}://{context.Request.Host}");

            // 1. Forward the cookies from the browser to the internal GraphQL request
            if (context.Request.Headers.TryGetValue("Cookie", out var cookies))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookies.ToString());
            }

            // 2. Identify the platform as 'web' so the AuthDatasource knows to look for cookies
            client.DefaultRequestHeaders.Add("X-Platform", "web");

            return client;
        }

        public bool IsUserLoggedIn()
        {
            return _httpContextAccessor?.HttpContext?.Request.Cookies.ContainsKey(_authCookieName) ?? false;
        }
        #endregion


        #region Logout Logic
        public void Logout()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return;

            var context = _httpContextAccessor.HttpContext;

            // Delete auth cookie
            context.Response.Cookies.Delete(_authCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            // Optional: clear ClaimsPrincipal
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
        #endregion

    }
}