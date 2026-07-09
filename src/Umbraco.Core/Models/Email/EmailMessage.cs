namespace Umbraco.Cms.Core.Models.Email;

/// <summary>
///     Represents an email message with sender, recipients, subject, body, and optional attachments.
/// </summary>
public class EmailMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailMessage" /> class with a single recipient.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="isBodyHtml">A value indicating whether the body is HTML formatted.</param>
    public EmailMessage(string? from, string? to, string? subject, string? body, bool isBodyHtml)
        : this(from, new[] { to }, null, null, null, subject, body, isBodyHtml, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailMessage" /> class with multiple recipients and optional CC, BCC, and reply-to addresses.
    /// </summary>
    /// <param name="from">The sender's email address.</param>
    /// <param name="to">The array of recipient email addresses.</param>
    /// <param name="cc">The array of CC email addresses.</param>
    /// <param name="bcc">The array of BCC email addresses.</param>
    /// <param name="replyTo">The array of reply-to email addresses.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body content.</param>
    /// <param name="isBodyHtml">A value indicating whether the body is HTML formatted.</param>
    /// <param name="attachments">The collection of email attachments.</param>
    public EmailMessage(
        string? from,
        string?[] to,
        string[]? cc,
        string[]? bcc,
        string[]? replyTo,
        string? subject,
        string? body,
        bool isBodyHtml,
        IEnumerable<EmailMessageAttachment>? attachments)
    {
        ArgumentIsNotNullOrEmpty(to, nameof(to));
        ArgumentIsNotNullOrEmpty(subject, nameof(subject));
        ArgumentIsNotNullOrEmpty(body, nameof(body));

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
    public string? From { get; }

    /// <summary>
    ///     Gets the array of recipient email addresses.
    /// </summary>
    public string?[] To { get; }

    /// <summary>
    ///     Gets the array of CC email addresses.
    /// </summary>
    public string[]? Cc { get; }

    /// <summary>
    ///     Gets the array of BCC email addresses.
    /// </summary>
    public string[]? Bcc { get; }

    /// <summary>
    ///     Gets the array of reply-to email addresses.
    /// </summary>
    public string[]? ReplyTo { get; }

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

    /// <summary>
    ///     Validates that the specified string argument is not null or empty.
    /// </summary>
    /// <param name="arg">The string argument to validate.</param>
    /// <param name="argName">The name of the argument for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the argument is empty.</exception>
    private static void ArgumentIsNotNullOrEmpty(string? arg, string argName)
    {
        if (arg == null)
        {
            throw new ArgumentNullException(argName);
        }

        if (arg.Length == 0)
        {
            throw new ArgumentException("Value cannot be empty.", argName);
        }
    }

    /// <summary>
    ///     Validates that the specified string array argument is not null, empty, or contains only null or empty elements.
    /// </summary>
    /// <param name="arg">The string array argument to validate.</param>
    /// <param name="argName">The name of the argument for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the argument is an empty array or contains only null or empty elements.</exception>
    private static void ArgumentIsNotNullOrEmpty(string?[]? arg, string argName)
    {
        if (arg == null)
        {
            throw new ArgumentNullException(argName);
        }

        if (arg.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty array.", argName);
        }

        if (arg.Any(x => x is not null && x.Length > 0) == false)
        {
            throw new ArgumentException("Value cannot be an array containing only null or empty elements.", argName);
        }
    }
}
