using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A generic model supporting notifications, this is useful for returning any model type to include notifications from api controllers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract(Name = "model", Namespace = "")]
    public class ModelWithNotifications<T> : INotificationModel
    {
        public ModelWithNotifications(T value)
        {
            Value = value;
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// The generic value
        /// </summary>
        [DataMember(Name = "value")]
        public T Value { get; private set; }

        /// <summary>
        /// The notifications
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}