using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "notificationModel", Namespace = "")]
    public class SimpleNotificationModel : INotificationModel
    {
        public SimpleNotificationModel()
        {
            Notifications = new List<Notification>();
        }

        public SimpleNotificationModel(params Notification[] notifications)
        {
            Notifications = new List<Notification>(notifications);
        }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}