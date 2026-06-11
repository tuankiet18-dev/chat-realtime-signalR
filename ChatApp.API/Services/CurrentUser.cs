using System.Security.Claims;

namespace ChatApp.Services;

public static class CurrentUser
{
    public static Guid GetRequiredUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Authenticated user id is missing.");
    }

    public static bool IsAdmin(ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }
}
