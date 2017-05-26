using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a user that is being edited
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    [ReadOnly(true)]
    public class UserDisplay : EntityBasic, INotificationModel
    {
        public UserDisplay()
        {
            Notifications = new List<Notification>();
        }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        /// <summary>
        /// The MD5 lowercase hash of the email which can be used by gravatar
        /// </summary>
        [DataMember(Name = "emailHash")]
        public string EmailHash { get; set; }

        [DataMember(Name = "lastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Returns a list of different size avatars
        /// </summary>
        [DataMember(Name = "avatars")]
        public string[] Avatars { get; set; }

        [DataMember(Name = "userState")]
        public UserState UserState { get; set; }

        [DataMember(Name = "culture", IsRequired = true)]
        public string Culture { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        public string Email { get; set; }

        /// <summary>
        /// The list of group aliases assigned to the user
        /// </summary>
        [DataMember(Name = "userGroups")]
        public IEnumerable<string> UserGroups { get; set; }

        /// <summary>
        /// Gets the available user groups (i.e. to populate a drop down)
        /// </summary>
        [DataMember(Name = "availableUserGroups")]
        public IEnumerable<UserGroupDisplay> AvailableUserGroups { get; set; }

        /// <summary>
        /// Gets the available cultures (i.e. to populate a drop down)
        /// The key is the culture stored in the database, the value is the Name
        /// </summary>
        [DataMember(Name = "availableCultures")]
        public IDictionary<string, string> AvailableCultures { get; set; }

        [DataMember(Name = "startContentIds")]
        public int[] StartContentIds { get; set; }

        [DataMember(Name = "startMediaIds")]
        public int[] StartMediaIds { get; set; }

        ///// <summary>
        ///// A list of sections the user is allowed to view based on their current groups assigned
        ///// </summary>
        //[DataMember(Name = "allowedSections")]
        //public IEnumerable<string> AllowedSections { get; set; }        

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}