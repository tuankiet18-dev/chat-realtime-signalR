using ChatApp.Contracts;
using ChatApp.Data;
using ChatApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public sealed class AuthService(
    ChatDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher,
    TokenService tokenService)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Email and password are required.");
        }

        var exists = await dbContext.Users.AnyAsync(user => user.Email == email);
        if (exists)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var userRole = await dbContext.Roles.SingleAsync(role => role.Name == "User");
        var user = new AppUser
        {
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? email : request.DisplayName.Trim(),
            RoleId = userRole.Id,
            Role = userRole
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .Include(existing => existing.Role)
            .SingleOrDefaultAsync(existing => existing.Email == email);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        return CreateResponse(user);
    }

    public async Task<UserProfile?> GetProfileAsync(Guid userId)
    {
        var user = await dbContext.Users
            .Include(existing => existing.Role)
            .SingleOrDefaultAsync(existing => existing.Id == userId);

        return user is null ? null : ToProfile(user);
    }

    private AuthResponse CreateResponse(AppUser user)
    {
        var (token, expiresAt) = tokenService.CreateToken(user);
        return new AuthResponse(token, ToProfile(user), expiresAt);
    }

    private static UserProfile ToProfile(AppUser user)
    {
        return new UserProfile(user.Id, user.Email, user.DisplayName, user.Role?.Name ?? "User");
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
