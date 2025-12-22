using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace Auth.API.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
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
                mutation Register($input: RegisterDtoInput!) {
                  registerUser(input: $input) {
                    message
                    statusCode
                  }
                }",
            variables = new
            {
                input = new RegisterDto
                {
                    Username = Input.Username,
                    Email = Input.Email,
                    Password = Input.Password
                }
            }
        };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
       

        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest, options); 

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Registration failed with status {(int)response.StatusCode}";
            return Page();
        }

        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<RegisterPayload>>();

        if (graphResponse?.Errors != null && graphResponse.Errors.Any())
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return Page();
        }

        var payload = graphResponse?.Data?.RegisterUser;
        if (payload == null)
        {
            ErrorMessage = "Unexpected response from server.";
            return Page();
        }

        SuccessMessage = payload.Message;
        ModelState.Clear();
        Input = new RegisterInputModel();

        return Page();
    }

    public class RegisterInputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    private sealed class RegisterPayload
    {
        public NormalResponseDto? RegisterUser { get; set; }
    }
}


