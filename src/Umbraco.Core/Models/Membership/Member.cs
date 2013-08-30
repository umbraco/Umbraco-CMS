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
    internal class Member : MemberProfile, IMember
    {
        private bool _hasIdentity;
        private int _id;
        private Guid _key;
        private DateTime _createDate;
        private DateTime _updateDate;
        private int _contentTypeId;
        private string _contentTypeAlias;

        private static readonly PropertyInfo IdSelector = ExpressionHelper.GetPropertyInfo<Member, int>(x => x.Id);
        private static readonly PropertyInfo KeySelector = ExpressionHelper.GetPropertyInfo<Member, Guid>(x => x.Key);
        private static readonly PropertyInfo CreateDateSelector = ExpressionHelper.GetPropertyInfo<Member, DateTime>(x => x.CreateDate);
        private static readonly PropertyInfo UpdateDateSelector = ExpressionHelper.GetPropertyInfo<Member, DateTime>(x => x.UpdateDate);
        private static readonly PropertyInfo HasIdentitySelector = ExpressionHelper.GetPropertyInfo<Member, bool>(x => x.HasIdentity);
        private static readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<Member, int>(x => x.ContentTypeId);
        private static readonly PropertyInfo DefaultContentTypeAliasSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.ContentTypeAlias);
        
        public Member()
        {}

        /// <summary>
        /// Integer Id
        /// </summary>
        [DataMember]
        public override int Id
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
        public override Guid Key
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
        public override DateTime CreateDate
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
        public override DateTime UpdateDate
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
        [IgnoreDataMember]
        public override bool HasIdentity
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

        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Password Question
        /// </summary>
        /// <remarks>
        /// Alias: umbracoPasswordRetrievalQuestionPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public string PasswordQuestion
        {
            get
            {
                return Properties[Constants.Conventions.Member.PasswordQuestion].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordQuestion].Value.ToString();
            }
            set
            {
                Properties[Constants.Conventions.Member.PasswordQuestion].Value = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the Password Answer
        /// </summary>
        /// <remarks>
        /// Alias: umbracoPasswordRetrievalAnswerPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public string PasswordAnswer
        {
            get
            {
                return Properties[Constants.Conventions.Member.PasswordAnswer].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordAnswer].Value.ToString();
            }
            set
            {
                Properties[Constants.Conventions.Member.PasswordAnswer].Value = value;
            }
        }

        /// <summary>
        /// Gets or set the comments for the member
        /// </summary>
        /// <remarks>
        /// Alias: umbracoCommentPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public string Comments
        {
            get
            {
                return Properties[Constants.Conventions.Member.Comments].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.Comments].Value.ToString();
            }
            set
            {
                Properties[Constants.Conventions.Member.Comments].Value = value;
            }
        }
        
        /// <summary>
        /// Gets or sets a boolean indicating whether the Member is approved
        /// </summary>
        /// <remarks>
        /// Alias: umbracoApprovePropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public bool IsApproved
        {
            get
            {
                if (Properties[Constants.Conventions.Member.IsApproved].Value == null)
                    return default(bool);

                if (Properties[Constants.Conventions.Member.IsApproved].Value is bool)
                    return (bool)Properties[Constants.Conventions.Member.IsApproved].Value;

                return (bool)Convert.ChangeType(Properties[Constants.Conventions.Member.IsApproved].Value, typeof(bool));
            }
            set
            {
                Properties[Constants.Conventions.Member.IsApproved].Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Member is locked out
        /// </summary>
        /// <remarks>
        /// Alias: umbracoLockPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public bool IsLockedOut
        {
            get
            {
                if (Properties[Constants.Conventions.Member.IsLockedOut].Value == null)
                    return default(bool);

                if (Properties[Constants.Conventions.Member.IsLockedOut].Value is bool)
                    return (bool)Properties[Constants.Conventions.Member.IsLockedOut].Value;

                return (bool)Convert.ChangeType(Properties[Constants.Conventions.Member.IsLockedOut].Value, typeof(bool));
            }
            set
            {
                Properties[Constants.Conventions.Member.IsLockedOut].Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the date for last login
        /// </summary>
        /// <remarks>
        /// Alias: umbracoLastLoginPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public DateTime LastLoginDate
        {
            get
            {
                if (Properties[Constants.Conventions.Member.LastLoginDate].Value == null)
                    return default(DateTime);

                if (Properties[Constants.Conventions.Member.LastLoginDate].Value is DateTime)
                    return (DateTime)Properties[Constants.Conventions.Member.LastLoginDate].Value;

                return (DateTime)Convert.ChangeType(Properties[Constants.Conventions.Member.LastLoginDate].Value, typeof(DateTime));
            }
            set
            {
                Properties[Constants.Conventions.Member.LastLoginDate].Value = value;
            }
        }

        /// <summary>
        /// Gest or sets the date for last password change
        /// </summary>
        /// <remarks>
        /// Alias: umbracoMemberLastPasswordChange
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public DateTime LastPasswordChangeDate
        {
            get
            {
                if (Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value == null)
                    return default(DateTime);

                if (Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value is DateTime)
                    return (DateTime)Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value;

                return (DateTime)Convert.ChangeType(Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value, typeof(DateTime));
            }
            set
            {
                Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the date for when Member was locked out
        /// </summary>
        /// <remarks>
        /// Alias: umbracoMemberLastLockout
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public DateTime LastLockoutDate
        {
            get
            {
                if (Properties[Constants.Conventions.Member.LastLockoutDate].Value == null)
                    return default(DateTime);

                if (Properties[Constants.Conventions.Member.LastLockoutDate].Value is DateTime)
                    return (DateTime)Properties[Constants.Conventions.Member.LastLockoutDate].Value;

                return (DateTime)Convert.ChangeType(Properties[Constants.Conventions.Member.LastLockoutDate].Value, typeof(DateTime));
            }
            set
            {
                Properties[Constants.Conventions.Member.LastLockoutDate].Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of failed password attempts.
        /// This is the number of times the password was entered incorrectly upon login.
        /// </summary>
        /// <remarks>
        /// Alias: umbracoFailedPasswordAttemptsPropertyTypeAlias
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public int FailedPasswordAttempts
        {
            get
            {
                if (Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value == null)
                    return default(int);

                if (Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value is int)
                    return (int)Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value;

                return (int)Convert.ChangeType(Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value, typeof(int));
            }
            set
            {
                Properties[Constants.Conventions.Member.LastLockoutDate].Value = value;
            }
        }

        /// <summary>
        /// Boolean indicating whether the user is online
        /// </summary>
        /// <remarks>Context dependent property</remarks>
        [IgnoreDataMember]
        public bool IsOnline { get; set; }

        /// <summary>
        /// Guid Id of the curent Version
        /// </summary>
        [DataMember]
        public Guid Version { get; internal set; }

        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        [DataMember]
        public virtual int ContentTypeId
        {
            get { return _contentTypeId; }
            internal set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentTypeId = value;
                    return _contentTypeId;
                }, _contentTypeId, DefaultContentTypeIdSelector);
            }
        }

        /// <summary>
        /// String alias of the default ContentType
        /// </summary>
        [DataMember]
        public virtual string ContentTypeAlias
        {
            get { return _contentTypeAlias; }
            internal set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentTypeAlias = value;
                    return _contentTypeAlias;
                }, _contentTypeAlias, DefaultContentTypeAliasSelector);
            }
        }

        public object ProfileId { get; set; }
        public IEnumerable<object> Groups { get; set; }

        /* Internal experiment - only used for mapping queries. 
         * Adding these to have first level properties instead of the Properties collection.
         */
        internal string LongStringPropertyValue { get; set; }
        internal string ShortStringPropertyValue { get; set; }
        internal int IntegerropertyValue { get; set; }
        internal bool BoolPropertyValue { get; set; }
        internal DateTime DateTimePropertyValue { get; set; }
        internal string PropertyTypeAlias { get; set; }
        
        #region Internal methods

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal void AddingEntity()
        {
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }

        /// <summary>
        /// Method to call on entity saved/updated
        /// </summary>
        internal virtual void UpdatingEntity()
        {
            UpdateDate = DateTime.Now;
        }

        internal virtual void ResetIdentity()
        {
            _hasIdentity = false;
            _id = default(int);
        }

        #endregion
    }
}