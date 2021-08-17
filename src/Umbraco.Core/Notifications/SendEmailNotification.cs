using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Notifications
{
    public class SendEmailNotification : INotification
    {
        public SendEmailNotification(NotificationEmailModel message) => Message = message;

        public NotificationEmailModel Message { get; }

        /// <summary>
        /// Call to tell Umbraco that the email sending is handled.
        /// </summary>
        public void HandleEmail() => IsHandled = true;

        /// <summary>
        /// Returns true if the email sending is handled.
        /// </summary>
        public bool IsHandled { get; private set; }
    }
}
