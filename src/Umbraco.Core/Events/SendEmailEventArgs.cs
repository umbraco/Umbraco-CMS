using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for send email operations.
/// </summary>
public class SendEmailEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SendEmailEventArgs" /> class.
    /// </summary>
    /// <param name="message">The email message being sent.</param>
    public SendEmailEventArgs(EmailMessage message) => Message = message;

    /// <summary>
    ///     Gets the email message being sent.
    /// </summary>
    public EmailMessage Message { get; }
}
