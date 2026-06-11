using ChatApp.Contracts;
using ChatApp.Data;
using ChatApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public sealed class ChatService(ChatDbContext dbContext)
{
    public async Task<IReadOnlyList<ChatMessageDto>> GetRecentMessagesAsync(Guid viewerId, bool viewerIsAdmin, int take = 50)
    {
        var messages = await dbContext.Messages
            .Include(message => message.Sender)
            .Include(message => message.Attachment)
            .Include(message => message.ReadReceipts)
            .OrderByDescending(message => message.CreatedAt)
            .Take(take)
            .ToListAsync();

        return messages
            .OrderBy(message => message.CreatedAt)
            .Select(message => ToDto(message, viewerId, viewerIsAdmin))
            .ToList();
    }

    public async Task<ChatMessageDto?> SendMessageAsync(Guid senderId, bool senderIsAdmin, SendMessageRequest request)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text) && request.Attachment is null)
        {
            return null;
        }

        var message = new ChatMessage
        {
            SenderId = senderId,
            Text = text
        };

        if (request.Attachment is not null)
        {
            message.Attachment = new ChatAttachment
            {
                OriginalFileName = request.Attachment.FileName,
                StoredFileName = Path.GetFileName(request.Attachment.Url),
                PublicUrl = request.Attachment.Url,
                ContentType = request.Attachment.ContentType,
                Size = request.Attachment.Size,
                Kind = ParseKind(request.Attachment.Kind)
            };
        }

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();

        var saved = await LoadMessageAsync(message.Id);
        return saved is null ? null : ToDto(saved, senderId, senderIsAdmin);
    }

    public async Task<ChatMessageDto?> RecallMessageAsync(Guid messageId, Guid userId, bool isAdmin)
    {
        var message = await dbContext.Messages
            .Include(existing => existing.Sender)
            .Include(existing => existing.Attachment)
            .Include(existing => existing.ReadReceipts)
            .SingleOrDefaultAsync(existing => existing.Id == messageId);

        if (message is null || message.SenderId != userId || message.State != MessageState.Active)
        {
            return null;
        }

        message.State = MessageState.Recalled;
        message.RecalledAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();

        return ToDto(message, userId, isAdmin);
    }

    public async Task<ChatMessageDto?> AdminDeleteMessageAsync(Guid messageId, Guid adminId)
    {
        var message = await dbContext.Messages
            .Include(existing => existing.Sender)
            .Include(existing => existing.Attachment)
            .Include(existing => existing.ReadReceipts)
            .SingleOrDefaultAsync(existing => existing.Id == messageId);

        if (message is null)
        {
            return null;
        }

        message.State = MessageState.DeletedByAdmin;
        message.DeletedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();

        return ToDto(message, adminId, true);
    }

    public async Task<ChatMessageDto?> MarkSeenAsync(Guid messageId, Guid userId, bool isAdmin)
    {
        var exists = await dbContext.ReadReceipts.AnyAsync(receipt =>
            receipt.MessageId == messageId && receipt.UserId == userId);
        if (!exists)
        {
            dbContext.ReadReceipts.Add(new ReadReceipt { MessageId = messageId, UserId = userId });
            await dbContext.SaveChangesAsync();
        }

        var message = await LoadMessageAsync(messageId);
        return message is null ? null : ToDto(message, userId, isAdmin);
    }

    private async Task<ChatMessage?> LoadMessageAsync(Guid messageId)
    {
        return await dbContext.Messages
            .Include(message => message.Sender)
            .Include(message => message.Attachment)
            .Include(message => message.ReadReceipts)
            .SingleOrDefaultAsync(message => message.Id == messageId);
    }

    private static ChatMessageDto ToDto(ChatMessage message, Guid viewerId, bool viewerIsAdmin)
    {
        var text = message.State == MessageState.Active ? message.Text : "Tin nhắn đã được thu hồi";
        var attachment = message.State == MessageState.Active && message.Attachment is not null
            ? ToAttachmentDto(message.Attachment)
            : null;

        return new ChatMessageDto(
            message.Id,
            message.SenderId,
            message.Sender?.DisplayName ?? "Unknown",
            text,
            message.State.ToString(),
            message.CreatedAt,
            attachment,
            message.ReadReceipts.Count,
            message.State == MessageState.Active && message.SenderId == viewerId,
            viewerIsAdmin && message.State == MessageState.Active);
    }

    private static ChatAttachmentDto ToAttachmentDto(ChatAttachment attachment)
    {
        return new ChatAttachmentDto(
            attachment.Id,
            attachment.PublicUrl,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.Size,
            attachment.Kind.ToString());
    }

    private static AttachmentKind ParseKind(string kind)
    {
        return Enum.TryParse<AttachmentKind>(kind, true, out var parsed) ? parsed : AttachmentKind.File;
    }
}
