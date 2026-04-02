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

/// <summary>
/// Provides functionality to send password reset emails to users who have requested a password reset.
/// </summary>
public class EmailUserForgotPasswordSender : IUserForgotPasswordSender
{
    private readonly IEmailSender _emailSender;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly GlobalSettings _globalSettings;
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailUserForgotPasswordSender"/> class.
    /// </summary>
    /// <param name="emailSender">An implementation of <see cref="IEmailSender"/> used to send emails.</param>
    /// <param name="localizedTextService">An implementation of <see cref="ILocalizedTextService"/> for retrieving localized text resources.</param>
    /// <param name="globalSettings">A monitor for <see cref="GlobalSettings"/> providing application-wide configuration values.</param>
    /// <param name="securitySettings">A monitor for <see cref="SecuritySettings"/> providing security-related configuration values.</param>
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
    }

    /// <summary>
    /// Asynchronously sends a password reset email to the user specified in the provided message model.
    /// </summary>
    /// <param name="messageModel">The message model containing information about the recipient and the password reset link.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous send operation.</returns>
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

        await _emailSender.SendAsync(message, Constants.Web.EmailTypes.PasswordReset, true, _securitySettings.PasswordResetEmailExpiry);
    }

    /// <summary>
    /// Determines whether a forgot password email can be sent, based on the current security settings and the email sender's capability to send required emails.
    /// </summary>
    /// <returns><c>true</c> if the forgot password email can be sent; otherwise, <c>false</c>.</returns>
    public bool CanSend() => _securitySettings.AllowPasswordReset && _emailSender.CanSendRequiredEmail();
}
