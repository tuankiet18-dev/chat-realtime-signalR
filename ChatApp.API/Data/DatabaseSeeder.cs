using ChatApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public sealed class DatabaseSeeder(
    ChatDbContext dbContext,
    IConfiguration configuration,
    IPasswordHasher<AppUser> passwordHasher)
{
    public async Task SeedAsync()
    {
        await dbContext.Database.MigrateAsync();

        var adminRole = await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("User");

        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@chat.local";
        var adminPassword = configuration["SeedAdmin:Password"] ?? "Admin@123456";
        var adminDisplayName = configuration["SeedAdmin:DisplayName"] ?? "Admin";

        var hasAdmin = await dbContext.Users.AnyAsync(user => user.Email == adminEmail);
        if (!hasAdmin)
        {
            var admin = new AppUser
            {
                Email = adminEmail,
                DisplayName = adminDisplayName,
                RoleId = adminRole.Id
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<AppRole> EnsureRoleAsync(string name)
    {
        var role = await dbContext.Roles.SingleOrDefaultAsync(existing => existing.Name == name);
        if (role is not null)
        {
            return role;
        }

        role = new AppRole { Name = name };
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
        return role;
    }
}
