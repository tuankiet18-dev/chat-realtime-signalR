namespace ChatApp.Data.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RoomId { get; set; } = "general";

    public Guid SenderId { get; set; }

    public AppUser? Sender { get; set; }

    public string Text { get; set; } = string.Empty;

    public MessageState State { get; set; } = MessageState.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RecalledAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ChatAttachment? Attachment { get; set; }

    public List<ReadReceipt> ReadReceipts { get; set; } = [];
}

public enum MessageState
{
    Active = 1,
    Recalled = 2,
    DeletedByAdmin = 3
}
