using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace Auth.API.Pages.Auth;

public class ProfileModel : PageModel
{
    // Ensure this matches exactly what is in your AppSettings/Program.cs
    private readonly string _authCookieName="xx-auth-cookie";
    private readonly IHttpClientFactory _httpClientFactory;

    public ProfileModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

    }

    public ProfileDto? Profile { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (!Request.Cookies.TryGetValue(_authCookieName, out var authCookie))
        {
            ErrorMessage = "You are not logged in. No session cookie found.";
            return;
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Cookie", $"{_authCookieName}={authCookie}");
        client.DefaultRequestHeaders.Add("X-Platform", "web");

        var graphQLRequest = new
        {
            query = @"
            query GetProfile {
                profile {
                    username
                    email
                }
            }"
        };

        try
        {
            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Backend returned status {(int)response.StatusCode}. You might be unauthorized.";
                return;
            }

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<ProfilePayload>>();

            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return;
            }

            Profile = graphResponse?.Data?.profile;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection error: {ex.Message}";
        }
    }

    // Helper classes for deserialization
    public class GraphQLResponse<T>
    {
        public T? Data { get; set; }
        public List<GraphQLError>? Errors { get; set; }
    }

    public class GraphQLError
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ProfilePayload
    {
        public ProfileDto? profile { get; set; }
    }
}

// Ensure this DTO matches your existing Auth.API.Entities
public class ProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}