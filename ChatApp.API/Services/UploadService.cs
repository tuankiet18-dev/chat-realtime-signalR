using System.Text.RegularExpressions;
using ChatApp.Contracts;
using Microsoft.AspNetCore.Components.Forms;

namespace ChatApp.Services;

public sealed partial class UploadService(IWebHostEnvironment environment, IConfiguration configuration)
{
    public async Task<UploadResponse> SaveAsync(IBrowserFile file)
    {
        var maxBytes = configuration.GetValue<long>("Uploads:MaxFileSizeBytes", 50 * 1024 * 1024);
        if (file.Size <= 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        if (file.Size > maxBytes)
        {
            throw new InvalidOperationException("File must be 50 MB or smaller.");
        }

        var (root, storedName, originalName) = PrepareStorage(file.Name);
        var storedPath = Path.Combine(root, storedName);

        await using var input = file.OpenReadStream(maxBytes);
        await using var output = File.Create(storedPath);
        await input.CopyToAsync(output);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var publicPath = configuration["Uploads:PublicPath"] ?? "/uploads";
        return new UploadResponse(
            $"{publicPath}/{storedName}",
            originalName,
            contentType,
            file.Size,
            ResolveKind(contentType, file.Name));
    }

    public async Task<UploadResponse> SaveAsync(IFormFile file)
    {
        var maxBytes = configuration.GetValue<long>("Uploads:MaxFileSizeBytes", 50 * 1024 * 1024);
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        if (file.Length > maxBytes)
        {
            throw new InvalidOperationException("File must be 50 MB or smaller.");
        }

        var (root, storedName, originalName) = PrepareStorage(file.FileName);
        var storedPath = Path.Combine(root, storedName);

        await using var stream = File.Create(storedPath);
        await file.CopyToAsync(stream);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var publicPath = configuration["Uploads:PublicPath"] ?? "/uploads";
        return new UploadResponse(
            $"{publicPath}/{storedName}",
            originalName,
            contentType,
            file.Length,
            ResolveKind(contentType, file.FileName));
    }

    private (string Root, string StoredName, string OriginalName) PrepareStorage(string fileName)
    {
        var configuredRoot = configuration["Uploads:RootPath"] ?? "wwwroot/uploads";
        var root = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
        Directory.CreateDirectory(root);

        var originalName = Path.GetFileName(fileName);
        var safeName = FileNameSanitizer().Replace(originalName, "-").Trim('-');
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "upload";
        }

        var extension = Path.GetExtension(safeName);
        var storedName = $"{Path.GetFileNameWithoutExtension(safeName)}-{Guid.NewGuid():N}{extension}";
        return (root, storedName, originalName);
    }

    private static string ResolveKind(string contentType, string fileName = "")
    {
        var lowerType = contentType.ToLowerInvariant();
        var lowerExt = Path.GetExtension(fileName).ToLowerInvariant();

        if (lowerType.StartsWith("image/") || lowerExt == ".jpg" || lowerExt == ".jpeg" || lowerExt == ".png" || lowerExt == ".gif" || lowerExt == ".webp")
        {
            return "image";
        }

        if (lowerType.StartsWith("video/") || lowerExt == ".mp4" || lowerExt == ".webm" || lowerExt == ".ogg")
        {
            return "video";
        }

        return "file";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9._-]+")]
    private static partial Regex FileNameSanitizer();
}
