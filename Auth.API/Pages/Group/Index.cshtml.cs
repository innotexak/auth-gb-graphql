using Auth.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auth.API.Pages.Dm
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AuthHelpers _authHelpers;
        public List<GroupDto> Groups { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateGroupInputModel Input { get; set; } = new();

        public IndexModel(AuthHelpers authHelpers) => _authHelpers = authHelpers;

        public class AddMembersInput
        {
            [JsonPropertyName("conversationId")]
            public string ConversationId { get; set; } = string.Empty;

            [JsonPropertyName("userIds")]
            public List<string> UserIds { get; set; } = new();
        }

        // Ensure this handler is reached
        public async Task<IActionResult> OnPostAddMembersAsync([FromBody] AddMembersInput input)
        {
            if (!_authHelpers.IsUserLoggedIn()) return Unauthorized();

            // Safety check: if JS sends empty list
            if (input.UserIds == null || !input.UserIds.Any())
                return new JsonResult(new { message = "No users selected" }) { StatusCode = 400 };

            try
            {
                var client = _authHelpers.GetAuthenticatedClient();
                string lastMsg = "Successfully added members";

                foreach (var userId in input.UserIds)
                {
                    var request = new
                    {
                        query = @"mutation Add($conId: UUID!, $uId: UUID!) {
                    addUserToGroupChat(conversationId: $conId, newUserId: $uId) { message statusCode }
                }",
                        variables = new { conId = input.ConversationId, uId = userId }
                    };

                    var res = await client.PostAsJsonAsync("/graphql", request);
                    // Check for errors here if one specific user fails
                }

                return new JsonResult(new { message = lastMsg });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { message = ex.Message }) { StatusCode = 500 };
            }
        }
        public async Task OnGetAsync()
        {
            if (_authHelpers.IsUserLoggedIn()) await LoadUserGroups();
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
               
                var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);
                var responseContent = await response.Content.ReadAsStringAsync();

       

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
          
                return new JsonResult(new { message = $"Error: {ex.Message}", statusCode = 500 });
            }
        }


        private async Task LoadUserGroups()
        {
            try
            {
                var client = _authHelpers.GetAuthenticatedClient();
                var request = new { query = @"{ userGroups { data { id title description status userId conversationId } } }" };

                var res = await client.PostAsJsonAsync("/graphql", request);
                var graphRes = await res.Content.ReadFromJsonAsync<GraphQLResponse<UserGroupsData>>();

                Groups = graphRes?.Data?.UserGroups?.Data ?? new();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        // DTOs for JSON Mapping
        public class GroupDto
        {
            [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
            [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
            [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
            [JsonPropertyName("conversationId")] public string ConversationId { get; set; } = string.Empty;
        }

        public class CreateGroupInputModel
        {
            [Required] public string Title { get; set; } = "";
            public string Description { get; set; } = "";

            public string Status { get; set; }
        }

        private sealed class UserGroupsData
        {
            [JsonPropertyName("userGroups")] public UserGroupsResponse UserGroups { get; set; } = new();
        }

        private sealed class UserGroupsResponse
        {
            [JsonPropertyName("data")] public List<GroupDto> Data { get; set; } = new();
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

    public class GraphQLResponse<T>
    {
        public T? Data { get; set; }
        public List<GraphQLError>? Errors { get; set; }
    }
    public class GraphQLError { public string Message { get; set; } = ""; }
    }


}