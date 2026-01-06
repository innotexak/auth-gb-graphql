using System.Net.Http.Json;
using System.Text.Json;
using Auth.API.Entities;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.API.Pages.Posts;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string Id { get; private set; } = string.Empty;
    public Post? Post { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(string id)
    {
        Id = id;

        if (string.IsNullOrWhiteSpace(Id))
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
                    query GetPostById($id: String!) {
                      postById(id: $id) {
                        id
                        title
                        description
                        content
                      }
                    }",
                variables = new { id = Id }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, jsonOptions);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"GraphQL Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Failed to load post. Status: {(int)response.StatusCode} - {responseContent}";
                return;
            }

            var graphResponse = await JsonSerializer.DeserializeAsync<GraphQLResponse<PostPayload>>(
                await response.Content.ReadAsStreamAsync(),
                jsonOptions);

            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return;
            }

            Post = graphResponse?.Data?.PostById;

            if (Post is null)
            {
                ErrorMessage = "Post not found.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private sealed class PostPayload
    {
        public Post? PostById { get; set; }
    }
}