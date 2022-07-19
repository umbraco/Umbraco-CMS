namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email when sent with notifications.
/// </summary>
public class NotificationEmailModel
{
    public NotificationEmailModel(
        NotificationEmailAddress? from,
        IEnumerable<NotificationEmailAddress?>? to,
        IEnumerable<NotificationEmailAddress>? cc,
        IEnumerable<NotificationEmailAddress>? bcc,
        IEnumerable<NotificationEmailAddress>? replyTo,
        string? subject,
        string? body,
        IEnumerable<EmailMessageAttachment>? attachments,
        bool isBodyHtml)
    {
        From = from;
        To = to;
        Cc = cc;
        Bcc = bcc;
        ReplyTo = replyTo;
        Subject = subject;
        Body = body;
        IsBodyHtml = isBodyHtml;
        Attachments = attachments?.ToList();
    }

    public NotificationEmailAddress? From { get; }

    public IEnumerable<NotificationEmailAddress?>? To { get; }

    public IEnumerable<NotificationEmailAddress>? Cc { get; }

    public IEnumerable<NotificationEmailAddress>? Bcc { get; }

    public IEnumerable<NotificationEmailAddress>? ReplyTo { get; }

    public string? Subject { get; }

    public string? Body { get; }

    public bool IsBodyHtml { get; }

    public IList<EmailMessageAttachment>? Attachments { get; }

    public bool HasAttachments => Attachments != null && Attachments.Count > 0;
}
