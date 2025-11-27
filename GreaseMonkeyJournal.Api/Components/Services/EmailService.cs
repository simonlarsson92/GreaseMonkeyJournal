using GreaseMonkeyJournal.Api.Components.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Email service implementation using MailKit for SMTP delivery.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendReminderEmailAsync(string toEmail, Dictionary<string, List<Reminder>> reminders)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpHost))
        {
            _logger.LogWarning("Email service not configured. Skipping email to {Email}", toEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Vehicle Maintenance Reminders - Grease Monkey Journal";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildHtmlEmailBody(reminders)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
            
            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername))
            {
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Reminder email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder email to {Email}", toEmail);
            throw;
        }
    }

    private string BuildHtmlEmailBody(Dictionary<string, List<Reminder>> reminders)
    {
        var totalReminders = reminders.Values.Sum(list => list.Count);
        
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #212529; color: white; padding: 20px; border-radius: 5px 5px 0 0; }
        .content { background-color: #f8f9fa; padding: 20px; }
        .reminder-section { margin-bottom: 25px; }
        .reminder-section h3 { color: #212529; border-bottom: 2px solid #dee2e6; padding-bottom: 5px; }
        .reminder-item { background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0d6efd; border-radius: 3px; }
        .reminder-today { border-left-color: #dc3545; }
        .reminder-1day { border-left-color: #ffc107; }
        .reminder-7day { border-left-color: #0dcaf0; }
        .vehicle-name { font-weight: bold; color: #0d6efd; }
        .due-date { color: #6c757d; font-size: 0.9em; }
        .footer { background-color: #212529; color: #adb5bd; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; font-size: 0.85em; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0;'>🔧 Vehicle Maintenance Reminders</h1>
            <p style='margin: 5px 0 0 0;'>Grease Monkey Journal</p>
        </div>
        <div class='content'>
            <p>You have <strong>" + totalReminders + " reminder(s)</strong> for upcoming vehicle maintenance:</p>";

        if (reminders.ContainsKey("Today") && reminders["Today"].Any())
        {
            html += @"
            <div class='reminder-section'>
                <h3>⚠️ Due Today</h3>";
            
            foreach (var reminder in reminders["Today"])
            {
                html += $@"
                <div class='reminder-item reminder-today'>
                    <div class='vehicle-name'>{reminder.Vehicle?.Make} {reminder.Vehicle?.Model} ({reminder.Vehicle?.Year})</div>
                    <div><strong>{reminder.Type}:</strong> {reminder.Description}</div>
                    <div class='due-date'>Due: {reminder.DueDate:MMMM dd, yyyy}</div>
                </div>";
            }
            
            html += "</div>";
        }

        if (reminders.ContainsKey("1 Day") && reminders["1 Day"].Any())
        {
            html += @"
            <div class='reminder-section'>
                <h3>📅 Due Tomorrow</h3>";
            
            foreach (var reminder in reminders["1 Day"])
            {
                html += $@"
                <div class='reminder-item reminder-1day'>
                    <div class='vehicle-name'>{reminder.Vehicle?.Make} {reminder.Vehicle?.Model} ({reminder.Vehicle?.Year})</div>
                    <div><strong>{reminder.Type}:</strong> {reminder.Description}</div>
                    <div class='due-date'>Due: {reminder.DueDate:MMMM dd, yyyy}</div>
                </div>";
            }
            
            html += "</div>";
        }

        if (reminders.ContainsKey("7 Days") && reminders["7 Days"].Any())
        {
            html += @"
            <div class='reminder-section'>
                <h3>🔔 Due in 7 Days</h3>";
            
            foreach (var reminder in reminders["7 Days"])
            {
                html += $@"
                <div class='reminder-item reminder-7day'>
                    <div class='vehicle-name'>{reminder.Vehicle?.Make} {reminder.Vehicle?.Model} ({reminder.Vehicle?.Year})</div>
                    <div><strong>{reminder.Type}:</strong> {reminder.Description}</div>
                    <div class='due-date'>Due: {reminder.DueDate:MMMM dd, yyyy}</div>
                </div>";
            }
            
            html += "</div>";
        }

        html += @"
            <p style='margin-top: 20px;'>Log in to your Grease Monkey Journal to view details and mark reminders as complete.</p>
        </div>
        <div class='footer'>
            <p style='margin: 0;'>This is an automated reminder from Grease Monkey Journal</p>
            <p style='margin: 5px 0 0 0;'>To manage notification preferences, log in and visit the Settings page</p>
        </div>
    </div>
</body>
</html>";

        return html;
    }
}
