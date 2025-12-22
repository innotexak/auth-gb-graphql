using System.Net.Http.Json;
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

    [FromRoute]
    public string Id { get; set; } = string.Empty;

    public string? PostTitle { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Load minimal info for confirmation
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
            variables = new { id = Id }
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

        PostTitle = graphResponse?.Data?.GetPostById?.Title;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Failed to delete post. Status {(int)response.StatusCode}";
            return Page();
        }

        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<DeletePostPayload>>();
        if (graphResponse?.Errors != null && graphResponse.Errors.Any())
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return Page();
        }

        // If GraphQL returns false, treat as failure but still navigate back
        return RedirectToPage("Index");
    }

    private sealed class PostPayload
    {
        public Post? GetPostById { get; set; }
    }

    private sealed class DeletePostPayload
    {
        public bool DeletePost { get; set; }
    }
}


