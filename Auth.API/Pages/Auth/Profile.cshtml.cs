using Auth.API.Helpers;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auth.API.Pages.Auth;

public class ProfileModel : PageModel
{
    // Ensure this matches exactly what is in your AppSettings/Program.cs
    private readonly AuthHelpers _authHelpers;

    public ProfileModel(IHttpClientFactory httpClientFactory, AuthHelpers authHelpers)
    {
        _authHelpers = authHelpers;

    }

    public ProfileDto? Profile { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (!_authHelpers.IsUserLoggedIn())
        {
            ErrorMessage = "You are not logged in. No session cookie found.";
            return;
        }


        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());


        var client = _authHelpers.GetAuthenticatedClient();

        var graphQLRequest = new
        {
            query = @"
             query GetProfile {
              profile {
                username
                email
                firstName
                lastName
                preferences {
                  emailNotification
                  profileVisibility
                }
                userStats {
                  postCount
                  groupCount
                }
                id
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

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<ProfilePayload>>(jsonOptions);


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

// Ensure this DTO matches your existing Auth.API.Entitie