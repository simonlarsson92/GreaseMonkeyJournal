using Microsoft.AspNetCore.Components.Forms;

namespace GreaseMonkeyJournal.Api.Components.Models;

public record LogEntrySubmission(LogEntry Entry, IReadOnlyList<IBrowserFile> NewFiles, IReadOnlyList<int> AttachmentIdsToDelete);
