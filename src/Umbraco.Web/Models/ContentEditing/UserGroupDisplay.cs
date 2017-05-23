using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "userGroup", Namespace = "")]
    public class UserGroupDisplay : EntityBasic, INotificationModel
    {
        public UserGroupDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }

        [DataMember(Name = "sections")]
        public IEnumerable<string> Sections { get; set; }

        [DataMember(Name = "startNodeContent")]
        public int StartContentId { get; set; }

        [DataMember(Name = "startNodeMedia")]
        public int StartMediaId { get; set; }
    }
}