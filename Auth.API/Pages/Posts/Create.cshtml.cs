using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.API.Pages.Posts;

public class CreateModel : PageModel
{
    private const string AuthCookieName = "gq_auth";
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

        if (!Request.Cookies.TryGetValue(AuthCookieName, out var authCookie))
        {
            ErrorMessage = "You must be logged in to create a post.";
            return Page();
        }

        LoginResponseDto? auth;
        try
        {
            auth = JsonSerializer.Deserialize<LoginResponseDto>(authCookie);
        }
        catch
        {
            ErrorMessage = "Invalid auth data; please login again.";
            return Page();
        }

        if (auth is null || string.IsNullOrEmpty(auth.UserId))
        {
            ErrorMessage = "Invalid auth data; please login again.";
            return Page();
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

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
                    UserId = auth.UserId
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


