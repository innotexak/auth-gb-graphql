using Auth.API.Entities;
using Auth.API.Helpers;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using static Auth.API.Services.DmService.DmDTOs;

namespace Auth.API.Pages.Dm
{
    public class ChatModel : PageModel
    {
        private readonly AuthHelpers _authHelpers;
        public List<UserDto> Users { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public SendDmInputModel Input { get; set; } = new();

        public ChatModel(AuthHelpers authHelpers)
        {
            _authHelpers = authHelpers;
        }

        public async Task OnGetAsync()
        {
            if (!_authHelpers.IsUserLoggedIn())
            {
                ErrorMessage = "Please log in to view messages.";
                return;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _authHelpers.GetAuthenticatedClient();

   
         var graphQLRequest = new
         {
           query = @"
            query GetUsers{
              allUsers{
                email
                username
                id
              }
            }"
                 };
        
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to load users.";
                return;
            }

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<AllUsersData>>();

            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return;
            }

            Users = graphResponse?.Data?.AllUsers ?? new();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (!_authHelpers.IsUserLoggedIn())
            {
                ErrorMessage = "You must be logged in to send a message.";
                return Page();
            }

            var client = _authHelpers.GetAuthenticatedClient();

            var graphQLRequest = new
            {
                query = @"
                    mutation SendDirectMessage($receiverId: String!, $message: String!) {
                      sendDirectMessage(receiverId: $receiverId, message: $message) {
                        id
                        conversationId
                        senderId
                        content
                        createdAt
                      }
                    }",
                variables = new
                {
                    receiverId = Input.ReceiverId,
                    message = Input.Content
                }
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Failed to send message. Status {(int)response.StatusCode}";
                return Page();
            }

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<SendDmPayload>>();
            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return Page();
            }

            return RedirectToPage();
        }

        public class SendDmInputModel
        {
            [Required]
            public string ReceiverId { get; set; } = string.Empty;

            [Required]
            public string Content { get; set; } = string.Empty;
        }

        private sealed class SendDmPayload
        {
            public DirectMessage? SendDirectMessage { get; set; }
        }

      
    }
} 