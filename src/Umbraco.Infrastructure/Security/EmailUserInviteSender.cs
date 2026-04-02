using System.Globalization;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Provides functionality to send email invitations to users, typically for onboarding or granting access to the system.
/// </summary>
public class EmailUserInviteSender : IUserInviteSender
{
    private readonly IEmailSender _emailSender;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly GlobalSettings _globalSettings;
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailUserInviteSender"/> class.
    /// </summary>
    /// <param name="emailSender">Service used to send emails.</param>
    /// <param name="localizedTextService">Service for retrieving localized strings.</param>
    /// <param name="globalSettings">The global settings options.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public EmailUserInviteSender(
        IEmailSender emailSender,
        ILocalizedTextService localizedTextService,
        IOptions<GlobalSettings> globalSettings)
        : this(
              emailSender,
              localizedTextService,
              globalSettings,
              StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Security.EmailUserInviteSender"/> class with the specified dependencies.
    /// </summary>
    /// <param name="emailSender">The service used to send emails.</param>
    /// <param name="localizedTextService">The service used for retrieving localized text.</param>
    /// <param name="globalSettings">The global settings options.</param>
    /// <param name="securitySettings">The security settings options.</param>
    public EmailUserInviteSender(
        IEmailSender emailSender,
        ILocalizedTextService localizedTextService,
        IOptions<GlobalSettings> globalSettings,
        IOptions<SecuritySettings> securitySettings)
    {
        _emailSender = emailSender;
        _localizedTextService = localizedTextService;
        _globalSettings = globalSettings.Value;
        _securitySettings = securitySettings.Value;
    }

    /// <summary>
    /// Asynchronously sends an invitation email to a user using the details provided in the <paramref name="invite"/> parameter.
    /// </summary>
    /// <param name="invite">A <see cref="UserInvitationMessage"/> containing the recipient, sender, invitation message, and invitation URI.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation of sending the invitation email.</returns>
    public async Task InviteUser(UserInvitationMessage invite)
    {
        CultureInfo recipientCulture = UmbracoUserExtensions.GetUserCulture(
            invite.Recipient.Language,
            _localizedTextService,
            _globalSettings);

        string senderEmail = string.IsNullOrEmpty(_globalSettings.Smtp?.From)
            ? invite.Sender.Email
            : _globalSettings.Smtp.From;

        string emailSubject = _localizedTextService.Localize(
            "user",
            "inviteEmailCopySubject",
            recipientCulture);

        string?[] bodyTokes =
        {
            invite.Recipient.Name,
            invite.Sender.Name ?? invite.Sender.Email,
            WebUtility.HtmlEncode(invite.Message)!.ReplaceLineEndings("<br/>"),
            invite.InviteUri.ToString(),
            senderEmail,
        };

        string emailBody = _localizedTextService.Localize(
            "user",
            "inviteEmailCopyFormat",
            recipientCulture,
            bodyTokes);

        // This needs to be in the correct mailto format including the name, else
        // the name cannot be captured in the email sending notification.
        // i.e. "Some Person" <hello@example.com>
        var address = new MailboxAddress(invite.Recipient.Name, invite.Recipient.Email);

        var message = new EmailMessage(senderEmail, address.ToString(), emailSubject, emailBody, true);

        await _emailSender.SendAsync(message, Constants.Web.EmailTypes.UserInvite, true, _securitySettings.UserInviteEmailExpiry);
    }

    /// <summary>
    /// Determines whether the system can send user invite emails.
    /// </summary>
    /// <returns><c>true</c> if invites can be sent; otherwise, <c>false</c>.</returns>
    public bool CanSendInvites() => _emailSender.CanSendRequiredEmail();
}
