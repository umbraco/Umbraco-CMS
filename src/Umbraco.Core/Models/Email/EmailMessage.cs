namespace Umbraco.Cms.Core.Models.Email;

public class EmailMessage
{
    public EmailMessage(string? from, string? to, string? subject, string? body, bool isBodyHtml)
        : this(from, new[] { to }, null, null, null, subject, body, isBodyHtml, null)
    {
    }

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

    public string? From { get; }

    public string?[] To { get; }

    public string[]? Cc { get; }

    public string[]? Bcc { get; }

    public string[]? ReplyTo { get; }

    public string? Subject { get; }

    public string? Body { get; }

    public bool IsBodyHtml { get; }

    public IList<EmailMessageAttachment>? Attachments { get; }

    public bool HasAttachments => Attachments != null && Attachments.Count > 0;

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
