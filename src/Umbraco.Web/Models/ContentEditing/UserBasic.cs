using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "user", Namespace = "")]
    [ReadOnly(true)]
    public class UserBasic : EntityBasic, INotificationModel
    {
        public UserBasic()
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
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}