using Auth.API.Helpers;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Auth.API.Pages.Posts;

public class CreateModel : PageModel
{

 
    private readonly AuthHelpers _authHelpers;
    public CreateModel(AuthHelpers authHelpers)
    {
        _authHelpers = authHelpers;
    }

    [BindProperty]
    public CreatePostInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

 
    
        if (!_authHelpers.IsUserLoggedIn())
        {
            ErrorMessage = "Please log in to view messages.";
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var client = _authHelpers.GetAuthenticatedClient();

        var graphQLRequest = new
        {
            query = @"
                mutation CreatePost($input: CreatePostDtoInput!) {
                  createPost(input: $input) {
                    message
                    statusCode
                  }
                }",
            variables = new
            {
                input = new CreatePostDto
                {
                    Title = Input.Title,
                    Description = Input.Description,
                    Content = Input.Content,
                    UserId = userId
                }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options);
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Failed to create post. Status {(int)response.StatusCode}";
            return Page();
        }

        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<NormalResponsePayload>>();
        if (graphResponse?.Errors != null && graphResponse.Errors.Any())
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return Page();
        }

        return RedirectToPage("Index");
    }

    public class CreatePostInputModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class NormalResponsePayload
    {
        public NormalResponseDto? CreatePost { get; set; }
    }
}


