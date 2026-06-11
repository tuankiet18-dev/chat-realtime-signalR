namespace ChatApp.Data.Entities;

public sealed class ReadReceipt
{
    public Guid MessageId { get; set; }

    public ChatMessage? Message { get; set; }

    public Guid UserId { get; set; }

    public AppUser? User { get; set; }

    public DateTimeOffset SeenAt { get; set; } = DateTimeOffset.UtcNow;
}
