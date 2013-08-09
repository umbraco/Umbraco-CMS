using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a user -> entity permission
    /// </summary>
    internal class EntityPermission
    {
        public EntityPermission(object userId, int entityId, string[] assignedPermissions)
        {
            UserId = userId;
            EntityId = entityId;
            AssignedPermissions = assignedPermissions;
        }

        public object UserId { get; set; }
        public int EntityId { get; set; }

        /// <summary>
        /// The assigned permissions for the user/entity combo
        /// </summary>
        public string[] AssignedPermissions { get; set; }
    }

    /// <summary>
    /// Represents a backoffice user
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class User : UserProfile, IUser
    {
        private bool _hasIdentity;

        public User(IUserType userType)
        {
            Groups = new List<object> { userType };
        }

        #region Implementation of IEntity

        [IgnoreDataMember]
        public bool HasIdentity { get { return Id != null || _hasIdentity; } }

        [IgnoreDataMember]
        int IEntity.Id
        {
            get
            {
                return int.Parse(base.Id.ToString());
            }
            set
            {
                base.Id = value;
                _hasIdentity = true;
            }
        }

        public Guid Key { get; set; }

        #endregion

        #region Implementation of IMembershipUser

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string PasswordQuestion { get; set; }
        [DataMember]
        public string PasswordAnswer { get; set; }
        [DataMember]
        public string Comments { get; set; }
        [DataMember]
        public bool IsApproved { get; set; }
        [DataMember]
        public bool IsOnline { get; set; }
        [DataMember]
        public bool IsLockedOut { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime UpdateDate { get; set; }
        [DataMember]
        public DateTime LastLoginDate { get; set; }
        [DataMember]
        public DateTime LastPasswordChangeDate { get; set; }
        [DataMember]
        public DateTime LastLockoutDate { get; set; }

        [DataMember]
        public object ProfileId { get; set; }
        [DataMember]
        public IEnumerable<object> Groups { get; set; }

        #endregion

        #region Implementation of IUser

        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string DefaultPermissions { get; set; }

        [DataMember]
        public bool DefaultToLiveEditing { get; set; }
        [DataMember]
        public bool NoConsole { get; set; }

        [IgnoreDataMember]
        public IUserType UserType
        {
            get
            {
                var type = Groups.FirstOrDefault();
                return type as IUserType;
            }
        }

        #endregion
    }
}