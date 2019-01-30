
namespace Umbraco.Web.Models.ContentEditing
{
    public static class MessagesExtensions
    {
        public static void AddNotification(this INotificationModel model, string header, string msg, NotificationStyle type)
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
                    NotificationType = NotificationStyle.Success
                });
        }

        public static void AddErrorNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Error
                });
        }

        public static void AddWarningNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Warning
                });
        }

        public static void AddInfoNotification(this INotificationModel model, string header, string msg)
        {
            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Info
                });
        }
    }
}
