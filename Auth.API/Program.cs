using System.Text;
using Auth.API.Data;
using Auth.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


builder.Services.AddGraphQLServer()
    .AddAuthorization()     
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var key = Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JWT_SECRET"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})    
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
}).AddCookie(builder.Configuration["AppSettings:COOKIENAME"]!, options => {
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Forbidden";
    options.Cookie.Name = builder.Configuration["AppSettings:COOKIENAME"];

});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
