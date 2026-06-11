using ChatApp.Data;
using ChatApp.Data.Entities;
using ChatApp.Contracts;
using ChatApp.Hubs;
using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string LocalCorsPolicy = "LocalChatClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddHttpClient();
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<UploadService>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseCors(LocalCorsPolicy);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "ChatApp.API",
    hub = "/hubs/chat",
    uploads = "/api/uploads"
}));

app.MapPost("/auth/register", async (RegisterRequest request, AuthService authService) =>
{
    try
    {
        return Results.Ok(await authService.RegisterAsync(request));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ApiError(ex.Message));
    }
});

app.MapPost("/auth/login", async (LoginRequest request, AuthService authService) =>
{
    try
    {
        return Results.Ok(await authService.LoginAsync(request));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ApiError(ex.Message));
    }
});

app.MapGet("/auth/me", async (ClaimsPrincipal user, AuthService authService) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userId, out var id))
    {
        return Results.Unauthorized();
    }

    var profile = await authService.GetProfileAsync(id);
    return profile is null ? Results.Unauthorized() : Results.Ok(profile);
}).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

app.MapPost("/api/uploads", async (IFormFile file, UploadService uploadService) =>
{
    try
    {
        return Results.Ok(await uploadService.SaveAsync(file));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new ApiError(ex.Message));
    }
})
.DisableAntiforgery()
.RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

app.MapHub<ChatHub>("/hubs/chat");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
