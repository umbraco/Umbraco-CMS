using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents an Umbraco Member
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}")]
    internal class Member : MemberProfile, IMembershipUser
    {
        private bool _hasIdentity;
        private int _id;
        private Guid _key;
        private DateTime _createDate;
        private DateTime _updateDate;

        private static readonly PropertyInfo IdSelector = ExpressionHelper.GetPropertyInfo<Member, int>(x => x.Id);
        private static readonly PropertyInfo KeySelector = ExpressionHelper.GetPropertyInfo<Member, Guid>(x => x.Key);
        private static readonly PropertyInfo CreateDateSelector = ExpressionHelper.GetPropertyInfo<Member, DateTime>(x => x.CreateDate);
        private static readonly PropertyInfo UpdateDateSelector = ExpressionHelper.GetPropertyInfo<Member, DateTime>(x => x.UpdateDate);
        private static readonly PropertyInfo HasIdentitySelector = ExpressionHelper.GetPropertyInfo<Member, bool>(x => x.HasIdentity);

        /// <summary>
        /// Integer Id
        /// </summary>
        [DataMember]
        public new int Id
        {
            get
            {
                return _id;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _id = value;
                    HasIdentity = true; //set the has Identity
                    return _id;
                }, _id, IdSelector);
            }
        }

        /// <summary>
        /// Guid based Id
        /// </summary>
        /// <remarks>The key is currectly used to store the Unique Id from the 
        /// umbracoNode table, which many of the entities are based on.</remarks>
        [DataMember]
        public Guid Key
        {
            get
            {
                if (_key == Guid.Empty)
                    return _id.ToGuid();

                return _key;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _key = value;
                    return _key;
                }, _key, KeySelector);
            }
        }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        [DataMember]
        public DateTime CreateDate
        {
            get { return _createDate; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _createDate = value;
                    return _createDate;
                }, _createDate, CreateDateSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Modified Date
        /// </summary>
        [DataMember]
        public DateTime UpdateDate
        {
            get { return _updateDate; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _updateDate = value;
                    return _updateDate;
                }, _updateDate, UpdateDateSelector);
            }
        }

        /// <summary>
        /// Indicates whether the current entity has an identity, eg. Id.
        /// </summary>
        public virtual bool HasIdentity
        {
            get
            {
                return _hasIdentity;
            }
            protected set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _hasIdentity = value;
                    return _hasIdentity;
                }, _hasIdentity, HasIdentitySelector);
            }
        }

        public string Username { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string Comments { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        
        public object ProfileId { get; set; }
        public IEnumerable<object> Groups { get; set; }

        #region Internal methods

        internal virtual void ResetIdentity()
        {
            _hasIdentity = false;
            _id = default(int);
        }

        #endregion
    }
}