using Auth.API.Entities;
using Auth.API.Helpers;
using Auth.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Pages.Dm
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AuthHelpers _authHelpers;
        public List<GroupDto> Groups { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public CreateGroupInputModel Input { get; set; } = new();

        public IndexModel(AuthHelpers authHelpers)
        {
            _authHelpers = authHelpers;
        }

        public async Task OnGetAsync()
        {
            if (!_authHelpers.IsUserLoggedIn())
            {
                ErrorMessage = "Please log in to view groups.";
                return;
            }

            await LoadUserGroups();
        }

        private async Task LoadUserGroups()
        {
            try
            {
                var client = _authHelpers.GetAuthenticatedClient();

                var graphQLRequest = new
                {
                    query = @"
                        query {
                          userGroups {
                            message
                            statusCode
                            data {
                              description
                              id
                              status
                              title
                              userId
                            }
                          }
                        }"
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Failed to load groups.";
                    return;
                }

                var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<UserGroupsData>>(options);

                if (graphResponse?.Errors != null && graphResponse.Errors.Any())
                {
                    ErrorMessage = graphResponse.Errors.First().Message;
                    return;
                }

                Groups = graphResponse?.Data?.UserGroups?.Data ?? new();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading groups: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostCreateGroupAsync()
        {
            System.Diagnostics.Debug.WriteLine($"CreateGroup called - Title: {Input?.Title}, Desc: {Input?.Description}, Status: {Input?.Status}");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                System.Diagnostics.Debug.WriteLine($"ModelState invalid: {errors}");
                return new JsonResult(new { message = $"Validation failed: {errors}", statusCode = 400 });
            }

            if (!_authHelpers.IsUserLoggedIn())
                return new JsonResult(new { message = "You must be logged in", statusCode = 401 });

            try
            {
                var client = _authHelpers.GetAuthenticatedClient();

                var graphQLRequest = new
                {
                    query = @"
                        mutation CreateUserGroup($input: GroupInputDtoInput!) {
                          createUserGroup(input: $input) {
                            message
                            statusCode
                          }
                        }",
                    variables = new
                    {
                        input = new
                        {
                            title = Input.Title,
                            description = Input.Description,
                            status = Input.Status ?? "ACTIVE"
                        }
                    }
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                System.Diagnostics.Debug.WriteLine($"GraphQL Request: {JsonSerializer.Serialize(graphQLRequest)}");

                var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"GraphQL Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"GraphQL Response Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                    return new JsonResult(new { message = $"Failed to create group. Status {(int)response.StatusCode}. Response: {responseContent}", statusCode = 500 });

                var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<CreateGroupPayload>>(options);

                if (graphResponse?.Errors != null && graphResponse.Errors.Any())
                {
                    var errorMsg = graphResponse.Errors.First().Message;
                    System.Diagnostics.Debug.WriteLine($"GraphQL Error: {errorMsg}");
                    return new JsonResult(new { message = errorMsg, statusCode = 400 });
                }

                var groupData = graphResponse?.Data?.CreateUserGroup;
                return new JsonResult(new
                {
                    message = groupData?.Message ?? "Group created successfully",
                    statusCode = groupData?.StatusCode ?? 200,
 
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}\n{ex.StackTrace}");
                return new JsonResult(new { message = $"Error: {ex.Message}", statusCode = 500 });
            }
        }

        public class CreateGroupInputModel
        {
            [Required(ErrorMessage = "Group title is required")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
            public string Title { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
            public string Description { get; set; } = string.Empty;

            public string? Status { get; set; } = "ACTIVE";
        }

        public class GroupDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("userId")]
            public string UserId { get; set; } = string.Empty;
        }

        private sealed class UserGroupsResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;

            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }

            [JsonPropertyName("data")]
            public List<GroupDto> Data { get; set; } = new();
        }

        private sealed class UserGroupsData
        {
            [JsonPropertyName("userGroups")]
            public UserGroupsResponse UserGroups { get; set; } = new();
        }

        private sealed class CreateGroupData
        {
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;

            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }

            [JsonPropertyName("data")]
            public GroupDto? Data { get; set; }
        }

        private sealed class CreateGroupPayload
        {
            [JsonPropertyName("createUserGroup")]
            public CreateGroupData CreateUserGroup { get; set; } = new();
        }
    }
}