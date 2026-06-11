using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Services;

public sealed class TokenService(IConfiguration configuration)
{
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(AppUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(configuration.GetValue("Jwt:AccessTokenMinutes", 120));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var roleName = user.Role?.Name ?? "User";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
