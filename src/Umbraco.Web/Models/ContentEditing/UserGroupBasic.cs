using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "userGroup", Namespace = "")]
    public class UserGroupBasic : EntityBasic, INotificationModel
    {
        public UserGroupBasic()
        {
            Notifications = new List<Notification>();
            Sections = Enumerable.Empty<Section>();
        }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }

        [DataMember(Name = "sections")]
        public IEnumerable<Section> Sections { get; set; }

        [DataMember(Name = "contentStartNode")]
        public EntityBasic ContentStartNode { get; set; }

        [DataMember(Name = "mediaStartNode")]
        public EntityBasic MediaStartNode { get; set; }

        /// <summary>
        /// The number of users assigned to this group
        /// </summary>
        [DataMember(Name = "userCount")]
        public int UserCount { get; set; }
    }
}
