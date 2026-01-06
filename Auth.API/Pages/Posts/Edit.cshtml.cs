using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Auth.API.Entities;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Invalid post id.";
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

            var graphQLRequest = new
            {
                query = @"
                    query Post($id: String!) {
                      postById(id: $id) {
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
                ErrorMessage = $"Failed to load post. Status: {(int)response.StatusCode}";
                return;
            }

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<PostPayload>>();

            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return;
            }

            var post = graphResponse?.Data?.PostById;

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
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
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
                    input = new
                    {
                        id = Input.Id,
                        title = Input.Title,
                        description = Input.Description,
                        content = Input.Content
                    }
                }
            };

            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Failed to update post. Status: {(int)response.StatusCode}";
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
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            return Page();
        }
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
        [JsonPropertyName("postById")]
        public Post? PostById { get; set; }
    }

    private sealed class UpdatePostPayload
    {
        [JsonPropertyName("updatePost")]
        public Post? UpdatePost { get; set; }
    }
}