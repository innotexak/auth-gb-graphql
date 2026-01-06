using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Auth.API.Entities;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.API.Pages.Posts;

public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // This property will hold the ID for the UI to use
    [BindProperty]
    public string Id { get; set; } = string.Empty;

    public string? PostTitle { get; set; }
    public string? ErrorMessage { get; set; }

    // The 'id' comes from the URL route @page "{id}"
    public async Task OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            ErrorMessage = "No post ID provided.";
            return;
        }

        Id = id; // Store it in the property for the form
        await FetchPostDetails(id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            ErrorMessage = "Post ID is missing. Cannot delete.";
            return Page();
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

        var graphQLRequest = new
        {
            query = @"
                mutation DeletePost($id: String!) {
                  deletePost(id: $id)
                }",
            variables = new { id = Id }
        };

        try
        {
            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);
            var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<DeletePostPayload>>();

            if (result?.Errors != null && result.Errors.Any())
            {
                ErrorMessage = result.Errors.First().Message;
                await FetchPostDetails(Id); // Reload title for the UI
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

    private async Task FetchPostDetails(string id)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

        var graphQLRequest = new
        {
            query = @"
            query Post($id: String!) {
              getPostById(id: $id) {
                id
                title
              }
            }",
            variables = new { id = id }
        };

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GraphQLResponse<PostPayload>>();
            if (result?.Data?.GetPostById != null)
            {
                PostTitle = result.Data.GetPostById.Title;
            }
        }
    }

    // --- Helper Classes ---
    private sealed class PostPayload
    {
        [JsonPropertyName("getPostById")]
        public Post? GetPostById { get; set; }
    }

    private sealed class DeletePostPayload
    {
        [JsonPropertyName("deletePost")]
        public bool DeletePost { get; set; }
    }
}