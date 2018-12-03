using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    using System.Collections.Generic;

    [DataContract(Name = "memberGroup", Namespace = "")]
    public class MemberGroupDisplay : EntityBasic, INotificationModel
    {
        public MemberGroupDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}
