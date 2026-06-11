namespace ChatApp.Contracts;

public sealed record RegisterRequest(string Email, string DisplayName, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, UserProfile User, DateTimeOffset ExpiresAt);

public sealed record UserProfile(Guid Id, string Email, string DisplayName, string Role);

public sealed record ApiError(string Error);
