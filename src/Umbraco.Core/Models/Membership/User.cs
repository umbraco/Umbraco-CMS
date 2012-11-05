using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a backoffice user
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    internal class User : UserProfile, IMembershipUser
    {
        #region Implementation of IMembershipUser

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string Comments { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public DateTime LastLockoutDate { get; set; }

        public object ProfileId { get; set; }
        public IEnumerable<object> Groups { get; set; }

        #endregion

        #region Implementation of IMembershipUserId

        public new object ProviderUserKey { get; set; }

        #endregion
    }
}