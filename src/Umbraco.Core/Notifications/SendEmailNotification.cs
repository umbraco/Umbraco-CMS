using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications
{
    public class SendEmailNotification : INotification
    {
        public SendEmailNotification(EmailMessage message) => Message = message;

        public EmailMessage Message { get; set; }
    }
}
