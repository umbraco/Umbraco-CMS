using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Events;

public class SendEmailEventArgs : EventArgs
{
    public SendEmailEventArgs(EmailMessage message) => Message = message;

    public EmailMessage Message { get; }
}
