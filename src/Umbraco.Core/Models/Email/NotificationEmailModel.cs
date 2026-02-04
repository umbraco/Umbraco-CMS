namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email when sent with notifications.
/// </summary>
public class NotificationEmailModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotificationEmailModel" /> class.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The collection of recipient email addresses.</param>
    /// <param name="cc">The collection of CC email addresses.</param>
    /// <param name="bcc">The collection of BCC email addresses.</param>
    /// <param name="replyTo">The collection of reply-to email addresses.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="attachments">The collection of email attachments.</param>
    /// <param name="isBodyHtml">A value indicating whether the body is HTML formatted.</param>
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

    /// <summary>
    ///     Gets the sender's email address.
    /// </summary>
    public NotificationEmailAddress? From { get; }

    /// <summary>
    ///     Gets the collection of recipient email addresses.
    /// </summary>
    public IEnumerable<NotificationEmailAddress?>? To { get; }

    /// <summary>
    ///     Gets the collection of CC email addresses.
    /// </summary>
    public IEnumerable<NotificationEmailAddress>? Cc { get; }

    /// <summary>
    ///     Gets the collection of BCC email addresses.
    /// </summary>
    public IEnumerable<NotificationEmailAddress>? Bcc { get; }

    /// <summary>
    ///     Gets the collection of reply-to email addresses.
    /// </summary>
    public IEnumerable<NotificationEmailAddress>? ReplyTo { get; }

    /// <summary>
    ///     Gets the email subject.
    /// </summary>
    public string? Subject { get; }

    /// <summary>
    ///     Gets the email body content.
    /// </summary>
    public string? Body { get; }

    /// <summary>
    ///     Gets a value indicating whether the body is HTML formatted.
    /// </summary>
    public bool IsBodyHtml { get; }

    /// <summary>
    ///     Gets the collection of email attachments.
    /// </summary>
    public IList<EmailMessageAttachment>? Attachments { get; }

    /// <summary>
    ///     Gets a value indicating whether the email has any attachments.
    /// </summary>
    public bool HasAttachments => Attachments != null && Attachments.Count > 0;
}
