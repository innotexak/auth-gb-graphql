using Auth.API.Helpers;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auth.API.Pages.Auth;

public class ProfileModel : PageModel
{
    // Ensure this matches exactly what is in your AppSettings/Program.cs
    private readonly AuthHelpers _authHelpers;

    public ProfileDto? Profile { get; set; }
    public string? ErrorMessage { get; set; }

    public ProfileModel(IHttpClientFactory httpClientFactory, AuthHelpers authHelpers)
    {
        _authHelpers = authHelpers;

    }


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
                bio
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
            var body = await response.Content.ReadAsStringAsync();
        

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Backend returned {(int)response.StatusCode}: {body}";
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
  
    public async Task<IActionResult> OnPostUpdateProfileAsync([FromBody] ProfileUpdateDto input)
    {
        if (input == null) return new JsonResult(new { message = "Invalid data" }) { StatusCode = 400 };

        //var options = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        //};

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        // Add this to handle the ProfileVisibilityEnum conversion
        options.Converters.Add(new JsonStringEnumConverter());

        var client = _authHelpers.GetAuthenticatedClient();
      
        var graphQLRequest = new
        {
            query = @"
        mutation UpdateUserProfile($input: ProfileUpdateDtoInput!) {
            updateUserDetails(input: $input) {
                message
                statusCode
            }
        }",
            variables = new  { input = input }
        };

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);
        var content = await response.Content.ReadAsStringAsync();

        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }


    //Logout user by clearing the auth cookie
    public IActionResult OnPostLogoutAsync()
    {
        _authHelpers.Logout();
        return RedirectToPage("/Auth/Login");
    }

    //Delete user 
    public async Task<IActionResult> OnPostDeleteUserAsync()
    {
        // 1. Ensure user is authenticated
        if (!_authHelpers.IsUserLoggedIn())
        {
            return new JsonResult(new
            {
                message = "Unauthorized"
            })
            { StatusCode = StatusCodes.Status401Unauthorized };
        }

        // 2. Get authenticated HTTP client
        var client = _authHelpers.GetAuthenticatedClient();

        // 3. GraphQL mutation payload
        var graphQLRequest = new
        {
            query = @"
        mutation DeleteUser {
          deleteUser {
            message
            statusCode
          }
        }"
        };

        try
        {
            // 4. Send mutation to GraphQL
            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);

            if (!response.IsSuccessStatusCode)
            {
                return new JsonResult(new
                {
                    message = "Failed to delete account"
                })
                { StatusCode = (int)response.StatusCode };
            }

            // 5. Deserialize GraphQL response
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var graphResponse =
                await response.Content.ReadFromJsonAsync<
                    GraphQLResponse<NormalResponseDto>>(jsonOptions);

            // 6. Handle GraphQL errors
            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                return new JsonResult(new
                {
                    message = graphResponse.Errors.First().Message
                })
                { StatusCode = StatusCodes.Status400BadRequest };
            }

            // 7. Success → clear auth session
            _authHelpers.Logout();

            return new JsonResult(new
            {
                success = true,
                message = graphResponse?.Data?.Message ?? "Account deleted"
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new
            {
                message = "Unexpected error occurred",
                error = ex.Message
            })
            { StatusCode = StatusCodes.Status500InternalServerError };
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