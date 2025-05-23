using System.Globalization;
using Microsoft.Extensions.Options;
using MimeKit;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Security;

public class EmailUserForgotPasswordSender : IUserForgotPasswordSender
{
    private readonly IEmailSender _emailSender;
    private readonly ILocalizedTextService _localizedTextService;
    private GlobalSettings _globalSettings;
    private SecuritySettings _securitySettings;

    public EmailUserForgotPasswordSender(
        IEmailSender emailSender,
        ILocalizedTextService localizedTextService,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IOptionsMonitor<SecuritySettings> securitySettings)
    {
        _emailSender = emailSender;
        _localizedTextService = localizedTextService;
        _globalSettings = globalSettings.CurrentValue;
        _securitySettings = securitySettings.CurrentValue;

        globalSettings.OnChange(settings => _globalSettings = settings);
        securitySettings.OnChange(settings => _securitySettings = settings);
    }

    public async Task SendForgotPassword(UserForgotPasswordMessage messageModel)
    {
        CultureInfo recipientCulture = UmbracoUserExtensions.GetUserCulture(
            messageModel.Recipient.Language,
            _localizedTextService,
            _globalSettings);

        string? senderEmail = _globalSettings.Smtp?.From;

        var emailSubject = _localizedTextService.Localize(
            "login",
            "resetPasswordEmailCopySubject",
            recipientCulture);

        string?[] bodyTokes =
        {
            messageModel.Recipient.Username,
            messageModel.ForgotPasswordUri.ToString(),
            senderEmail,
        };

        string emailBody = _localizedTextService.Localize(
            "login",
            "resetPasswordEmailCopyFormat",
            recipientCulture,
            bodyTokes);

        // This needs to be in the correct mailto format including the name, else
        // the name cannot be captured in the email sending notification.
        // i.e. "Some Person" <hello@example.com>
        var address = new MailboxAddress(messageModel.Recipient.Name, messageModel.Recipient.Email);

        var message = new EmailMessage(senderEmail, address.ToString(), emailSubject, emailBody, true);

        await _emailSender.SendAsync(message, Constants.Web.EmailTypes.PasswordReset, true);
    }

    public bool CanSend() => _securitySettings.AllowPasswordReset && _emailSender.CanSendRequiredEmail();
}
