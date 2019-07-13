
using System.Linq;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    public static class MessagesExtensions
    {
        public static void AddNotification(this INotificationModel model, string header, string msg, NotificationStyle type)
        {
            if (model.Exists(header, msg, type)) return;

            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = type
                });
        }

        public static void AddSuccessNotification(this INotificationModel model, string header, string msg)
        {
            if (model.Exists(header, msg, NotificationStyle.Success)) return;

            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Success
                });
        }

        public static void AddErrorNotification(this INotificationModel model, string header, string msg)
        {
            if (model.Exists(header, msg, NotificationStyle.Error)) return;

            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Error
                });
        }

        public static void AddWarningNotification(this INotificationModel model, string header, string msg)
        {
            if (model.Exists(header, msg, NotificationStyle.Warning)) return;

            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Warning
                });
        }

        public static void AddInfoNotification(this INotificationModel model, string header, string msg)
        {
            if (model.Exists(header, msg, NotificationStyle.Info)) return;

            model.Notifications.Add(new Notification()
                {
                    Header = header,
                    Message = msg,
                    NotificationType = NotificationStyle.Info
                });
        }

        private static bool Exists(this INotificationModel model, string header, string message, NotificationStyle notificationType) => model.Notifications.Any(x => x.Header.InvariantEquals(header) && x.Message.InvariantEquals(message) && x.NotificationType == notificationType);
    }
}
