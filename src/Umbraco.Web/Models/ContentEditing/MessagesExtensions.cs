using Umbraco.Web.UI;

namespace Umbraco.Web.Models.ContentEditing
{
    public static class MessagesExtensions
    {
        public static void AddNotification(this INotificationModel model, string header, string msg, SpeechBubbleIcon type)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = type
                });
        }

        public static void AddSuccessNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = SpeechBubbleIcon.Success
                });
        }

        public static void AddErrorNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = SpeechBubbleIcon.Error
                });
        }

        public static void AddWarningNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = SpeechBubbleIcon.Warning
                });
        }

        public static void AddInfoNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = SpeechBubbleIcon.Info
                });
        }
    }
}