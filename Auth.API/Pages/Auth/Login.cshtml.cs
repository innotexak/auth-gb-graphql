using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;


namespace Auth.API.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var client = _httpClientFactory.CreateClient();

        client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");
        client.DefaultRequestHeaders.Add("X-Platform", "web");



        var graphQLRequest = new
        {
            query = @"
            mutation Login($input:LoginDtoInput!){
              loginUser(input: $input){
              statusCode
              message
              data{
                userId
                username
                email
              }
              }
            }",
            variables = new
            {
                input = new LoginDto
                {
                    Username = Input.Username,
                    Password = Input.Password
                }
            }
        };
        var response = await client.PostAsJsonAsync("/graphql", graphQLRequest);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Login failed.";
            return Page();
        }

        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                Response.Headers.Append("Set-Cookie", cookie);
            }
        }
        var graphResponse = await response.Content.ReadFromJsonAsync<GraphQLResponse<LoginPayload>>();

        if (graphResponse?.Errors?.Any() == true)
        {
            ErrorMessage = graphResponse.Errors.First().Message;
            return Page();
        }

        // Now map the data
        var loginData = graphResponse?.Data?.loginUser?.Data;

        if (loginData == null)
        {
            ErrorMessage = "Login failed. No user data returned.";
            return Page();
        }

        // Success
        SuccessMessage = graphResponse.Data.loginUser.Message;

        // Redirect
        return RedirectToPage("/Posts/Index");

    }


    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginPayload
    {
        public NormalResponseWithDataDto<LoginResponseDto>? loginUser { get; set; }
    }

}


