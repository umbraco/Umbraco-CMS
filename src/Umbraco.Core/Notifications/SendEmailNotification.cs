using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Notifications
{
    public class SendEmailNotification : INotification
    {
        public SendEmailNotification(NotificationEmailModel message) => Message = message;

        public NotificationEmailModel Message { get; }
    }
}
