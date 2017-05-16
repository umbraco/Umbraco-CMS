using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a user that is being edited
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    public class UserDisplay : EntityBasic, INotificationModel
    {
        public UserDisplay()
        {
            Notifications = new List<Notification>();
        }

        [DataMember(Name = "culture", IsRequired = true)]
        public string Culture { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        public string Email { get; set; }

        [DataMember(Name = "userType")]
        public string UserType { get; set; }

        /// <summary>
        /// Gets the available user types (i.e. to populate a drop down)
        /// The key is the Alias the value is the Name - the Alias is what is used in the UserType property and for persistence
        /// </summary>
        [DataMember(Name = "availableUserTypes")]
        public IDictionary<string, string> AvailableUserTypes { get; set; }

        /// <summary>
        /// Gets the available cultures (i.e. to populate a drop down)
        /// The key is the culture stored in the database, the value is the Name
        /// </summary>
        [DataMember(Name = "availableCultures")]
        public IDictionary<string, string> AvailableCultures { get; set; }

        [DataMember(Name = "startContentId")]
        public int StartContentId { get; set; }

        [DataMember(Name = "startMediaId")]
        public int StartMediaId { get; set; }

        /// <summary>
        /// A list of sections the user is allowed to view.
        /// </summary>
        [DataMember(Name = "allowedSections")]
        public IEnumerable<string> AllowedSections { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}