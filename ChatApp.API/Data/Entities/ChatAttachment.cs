namespace ChatApp.Data.Entities;

public sealed class ChatAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MessageId { get; set; }

    public ChatMessage? Message { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string PublicUrl { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public AttachmentKind Kind { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum AttachmentKind
{
    Image = 1,
    Video = 2,
    File = 3
}
