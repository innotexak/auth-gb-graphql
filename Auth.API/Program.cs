using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.ErrorHandling;
using Auth.API.Helpers;
using Auth.API.Modules;
using Auth.API.Services.AuthService;
using Auth.API.Services.DmService;
using Auth.API.Services.PostService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Database
// --------------------
builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --------------------
// Razor Pages + HTTP
// --------------------
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthHelpers>();
builder.Services.AddScoped<AuthDatasource>();
builder.Services.AddScoped<PostDatasource>();
builder.Services.AddScoped<DmDatasource>();
builder.Services.AddHttpContextAccessor();

// --------------------
// GraphQL
// --------------------
var modules = new GraphQLModules();
builder.Services.AddGraphQLServer()
    .AddErrorFilter<GraphQLErrorFilter>()
    .AddAuthorization()
    .AddQueryType(d => d.Name("Query"))
    .AddTypes(modules.Queries)
    .AddMutationType(d => d.Name("Mutation"))
    .AddTypes(modules.Mutations)
    .AddSubscriptionType(d => d.Name("Subscription"))
    .AddTypes(modules.Subscriptions)
    .AddType<DirectMessage>()
    .AddInMemorySubscriptions();

// --------------------
// Authentication
// --------------------
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JWT_SECRET"]!);
var cookieName = "xx-auth-cookie";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SMART_AUTH";
    options.DefaultChallengeScheme = "SMART_AUTH";
})
.AddPolicyScheme("SMART_AUTH", "JWT or Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // Mobile JWT
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return JwtBearerDefaults.AuthenticationScheme;
        }

        // Web cookie
        if (!string.IsNullOrEmpty(cookieName) && context.Request.Cookies.ContainsKey(cookieName))
        {
            return CookieAuthenticationDefaults.AuthenticationScheme; // MUST match AddCookie scheme
        }

        // Fallback
        return JwtBearerDefaults.AuthenticationScheme;
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Forbidden";
    options.Cookie.Name = cookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =  CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

// --------------------
// Build & Middleware
// --------------------
var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapRazorPages();
app.UseWebSockets();
app.MapGraphQL();
app.RunWithGraphQLCommands(args);