using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a backoffice user
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    internal class User : UserProfile, IUser
    {
        private bool _hasIdentity;
        private int _id;

        #region Implementation of IEntity

        public bool HasIdentity { get { return Id != null || _hasIdentity; } }

        int IEntity.Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                _hasIdentity = true;
            }
        }

        public Guid Key { get; set; }

        #endregion

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
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public DateTime LastLockoutDate { get; set; }

        public object ProfileId { get; set; }
        public IEnumerable<object> Groups { get; set; }

        #endregion
    }
}