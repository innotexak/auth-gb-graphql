using System.Net.Http.Json;
using Auth.API.Entities;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.API.Pages.Posts;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<Post> Posts { get; set; } = new();
    public string? ErrorMessage { get; set; }

    [FromQuery]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

            var graphQLRequest = new
            {
                query = @"
                        query Posts($pageNumber: Int!, $pageSize: Int!) {
                          posts(input: { pageNumber: $pageNumber, pageSize: $pageSize }) {
                            items {
                              id
                              title
                              description
                              content
                              user {
                                id
                                username
                                email
                              }
                            }
                            pageNumber
                            pageSize
                            totalCount
                            totalPages
                          }
                        }",
                variables = new { pageNumber = PageNumber, pageSize = PageSize }
            };

            var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Failed to load posts. Status {(int)response.StatusCode}";
                return;
            }

            var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<PostsPayload>>();
            if (graphResponse?.Errors != null && graphResponse.Errors.Any())
            {
                ErrorMessage = graphResponse.Errors.First().Message;
                return;
            }

            var data = graphResponse?.Data?.Posts;
            if (data == null)
            {
                ErrorMessage = "Unexpected response from server.";
                return;
            }

            Posts = data.Items;
            PageNumber = data.PageNumber;
            PageSize = data.PageSize;
            TotalPages = data.TotalPages;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    // Payload classes matching GraphQL response
    private sealed class PostsPayload
    {
        public PaginatedResult<Post> Posts { get; set; } = new();
    }

 
}
