using System.Globalization;
using System.Net;
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

public class EmailUserInviteSender : IUserInviteSender
{
    private readonly IEmailSender _emailSender;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly GlobalSettings _globalSettings;

    public EmailUserInviteSender(
        IEmailSender emailSender,
        ILocalizedTextService localizedTextService,
        IOptions<GlobalSettings> globalSettings)
    {
        _emailSender = emailSender;
        _localizedTextService = localizedTextService;
        _globalSettings = globalSettings.Value;
    }

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

        await _emailSender.SendAsync(message, Constants.Web.EmailTypes.UserInvite, true);
    }

    public bool CanSendInvites() => _emailSender.CanSendRequiredEmail();
}
