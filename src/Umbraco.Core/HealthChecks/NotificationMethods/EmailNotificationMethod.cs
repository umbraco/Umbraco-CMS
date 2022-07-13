using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.NotificationMethods;

[HealthCheckNotificationMethod("email")]
public class EmailNotificationMethod : NotificationMethodBase
{
    private readonly IEmailSender? _emailSender;
    private readonly IHostingEnvironment? _hostingEnvironment;
    private readonly IMarkdownToHtmlConverter? _markdownToHtmlConverter;
    private readonly ILocalizedTextService? _textService;
    private ContentSettings? _contentSettings;

    public EmailNotificationMethod(
        ILocalizedTextService textService,
        IHostingEnvironment hostingEnvironment,
        IEmailSender emailSender,
        IOptionsMonitor<HealthChecksSettings> healthChecksSettings,
        IOptionsMonitor<ContentSettings> contentSettings,
        IMarkdownToHtmlConverter markdownToHtmlConverter)
        : base(healthChecksSettings)
    {
        var recipientEmail = Settings?["RecipientEmail"];
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            Enabled = false;
            return;
        }

        RecipientEmail = recipientEmail;

        _textService = textService ?? throw new ArgumentNullException(nameof(textService));
        _hostingEnvironment = hostingEnvironment;
        _emailSender = emailSender;
        _markdownToHtmlConverter = markdownToHtmlConverter;
        _contentSettings = contentSettings.CurrentValue ?? throw new ArgumentNullException(nameof(contentSettings));

        contentSettings.OnChange(x => _contentSettings = x);
    }

    public string? RecipientEmail { get; }

    public override async Task SendAsync(HealthCheckResults results)
    {
        if (ShouldSend(results) == false)
        {
            return;
        }

        if (string.IsNullOrEmpty(RecipientEmail))
        {
            return;
        }

        var message = _textService?.Localize(
            "healthcheck",
            "scheduledHealthCheckEmailBody",
            new[]
            {
                DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(),
                _markdownToHtmlConverter?.ToHtml(results, Verbosity),
            });

        // Include the umbraco Application URL host in the message subject so that
        // you can identify the site that these results are for.
        var host = _hostingEnvironment?.ApplicationMainUrl?.ToString();

        var subject = _textService?.Localize("healthcheck", "scheduledHealthCheckEmailSubject", new[] { host });

        EmailMessage mailMessage = CreateMailMessage(subject, message);
        Task? task = _emailSender?.SendAsync(mailMessage, Constants.Web.EmailTypes.HealthCheck);
        if (task is not null)
        {
            await task;
        }
    }

    private EmailMessage CreateMailMessage(string? subject, string? message)
    {
        var to = _contentSettings?.Notifications.Email;

        if (string.IsNullOrWhiteSpace(subject))
        {
            subject = "Umbraco Health Check Status";
        }

        var isBodyHtml = message.IsNullOrWhiteSpace() == false && message!.Contains("<") && message.Contains("</");
        return new EmailMessage(to, RecipientEmail, subject, message, isBodyHtml);
    }
}
