namespace GreaseMonkeyJournal.Api.Components.Models;

public class LogAttachment
{
    public int Id { get; set; }
    public int LogEntryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public LogEntry? LogEntry { get; set; }
}
