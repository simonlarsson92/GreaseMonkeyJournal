using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace GreaseMonkeyJournal.Api.Components.Services;

public record AttachmentDownload(Stream Content, string ContentType, string FileName);

public interface ILogAttachmentService
{
    Task<List<LogAttachment>> GetByLogEntryIdAsync(int logEntryId);
    Task<LogAttachment> SaveAsync(int logEntryId, IBrowserFile file);
    Task DeleteAsync(int attachmentId);
    Task<AttachmentDownload?> GetFileAsync(int attachmentId);
}
