namespace ChatApp.Data.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public AppRole? Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ChatMessage> Messages { get; set; } = [];

    public List<ReadReceipt> ReadReceipts { get; set; } = [];
}
