using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GreaseMonkeyJournal.Api.Components.Services;

public class LogAttachmentService : ILogAttachmentService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "application/pdf"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly VehicleLogDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LogAttachmentService> _logger;

    public LogAttachmentService(VehicleLogDbContext context, IWebHostEnvironment env, ILogger<LogAttachmentService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    public async Task<List<LogAttachment>> GetByLogEntryIdAsync(int logEntryId)
    {
        return await _context.LogAttachments
            .Where(a => a.LogEntryId == logEntryId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<LogAttachment> SaveAsync(int logEntryId, IBrowserFile file)
    {
        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed.");

        if (file.Size > MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the 10 MB limit.");

        var ext = Path.GetExtension(file.Name);
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var filePath = Path.Combine(uploadsDir, storedName);

        await using (var dest = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        await using (var src = file.OpenReadStream(MaxFileSizeBytes))
        {
            await src.CopyToAsync(dest);
        }

        var attachment = new LogAttachment
        {
            LogEntryId = logEntryId,
            FileName = file.Name,
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Size,
            UploadedAt = DateTime.UtcNow
        };

        _context.LogAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved attachment {FileName} for log entry {LogEntryId}", file.Name, logEntryId);
        return attachment;
    }

    public async Task DeleteAsync(int attachmentId)
    {
        var attachment = await _context.LogAttachments.FindAsync(attachmentId);
        if (attachment == null) return;

        var filePath = Path.Combine(_env.ContentRootPath, "uploads", attachment.StoredFileName);
        if (File.Exists(filePath))
            File.Delete(filePath);

        _context.LogAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted attachment {AttachmentId}", attachmentId);
    }

    public async Task<AttachmentDownload?> GetFileAsync(int attachmentId)
    {
        var attachment = await _context.LogAttachments.FindAsync(attachmentId);
        if (attachment == null) return null;

        var filePath = Path.Combine(_env.ContentRootPath, "uploads", attachment.StoredFileName);
        if (!File.Exists(filePath)) return null;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new AttachmentDownload(stream, attachment.ContentType, attachment.FileName);
    }
}
