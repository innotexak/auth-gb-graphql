using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Auth.API.Entities;

namespace Auth.API.Pages.Posts;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public UpdatePostInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string id)
    {
        // Load existing post to pre-populate form
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

        var graphQLRequest = new
        {
            query = @"
query Post($id: String!) {
  getPostById(id: $id) {
    id
    title
    description
    content
  }
}",
            variables = new { id }
        };

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Failed to load post. Status {(int)response.StatusCode}";
            return;
        }

        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<PostPayload>>();
        if (graphResponse?.Errors != null && graphResponse.Errors.Any())
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return;
        }

        var post = graphResponse?.Data?.GetPostById;
        if (post is null)
        {
            ErrorMessage = "Post not found.";
            return;
        }

        Input = new UpdatePostInputModel
        {
            Id = post.Id.ToString(),
            Title = post.Title,
            Description = post.Description,
            Content = post.Content
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

        var graphQLRequest = new
        {
            query = @"
mutation UpdatePost($input: UpdatePostDto!) {
  updatePost(input: $input) {
    id
  }
}",
            variables = new
            {
                input = new UpdatePostDto
                {
                    Id = Input.Id,
                    Title = Input.Title,
                    Description = Input.Description,
                    Content = Input.Content
                }
            }
        };

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Failed to update post. Status {(int)response.StatusCode}";
            return Page();
        }

        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<UpdatePostPayload>>();
        if (graphResponse?.Errors != null && graphResponse.Errors.Any())
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return Page();
        }

        return RedirectToPage("Index");
    }

    public class UpdatePostInputModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class PostPayload
    {
        public Post? GetPostById { get; set; }
    }

    private sealed class UpdatePostPayload
    {
        public Post? UpdatePost { get; set; }
    }
}


