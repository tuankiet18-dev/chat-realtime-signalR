using System.Text.Json.Serialization;

namespace ChatApp.Contracts;

public sealed record AttachmentInput(
    [property: JsonPropertyName("url")] string Url, 
    [property: JsonPropertyName("fileName")] string FileName, 
    [property: JsonPropertyName("contentType")] string ContentType, 
    [property: JsonPropertyName("size")] long Size, 
    [property: JsonPropertyName("kind")] string Kind);

public sealed record SendMessageRequest(
    [property: JsonPropertyName("text")] string? Text, 
    [property: JsonPropertyName("attachment")] AttachmentInput? Attachment);

public sealed record ChatAttachmentDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("contentType")] string ContentType,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("kind")] string Kind);

public sealed record ChatMessageDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("senderId")] Guid SenderId,
    [property: JsonPropertyName("senderName")] string SenderName,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("attachment")] ChatAttachmentDto? Attachment,
    [property: JsonPropertyName("seenCount")] int SeenCount,
    [property: JsonPropertyName("canRecall")] bool CanRecall,
    [property: JsonPropertyName("canAdminDelete")] bool CanAdminDelete);

public sealed record PresenceUser(
    [property: JsonPropertyName("userId")] Guid UserId, 
    [property: JsonPropertyName("displayName")] string DisplayName, 
    [property: JsonPropertyName("role")] string Role);

public sealed record TypingState(
    [property: JsonPropertyName("userId")] Guid UserId, 
    [property: JsonPropertyName("displayName")] string DisplayName, 
    [property: JsonPropertyName("isTyping")] bool IsTyping);

public sealed record UploadResponse(
    [property: JsonPropertyName("url")] string Url, 
    [property: JsonPropertyName("fileName")] string FileName, 
    [property: JsonPropertyName("contentType")] string ContentType, 
    [property: JsonPropertyName("size")] long Size, 
    [property: JsonPropertyName("kind")] string Kind);
