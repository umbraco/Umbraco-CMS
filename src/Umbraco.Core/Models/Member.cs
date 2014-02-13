using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Member object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Member : ContentBase, IMember
    {
        private readonly IMemberType _contentType;
        private string _contentTypeAlias;
        private string _username;
        private string _email;
        private string _password;
        private object _providerUserKey;
        private Type _userTypeKey;

        /// <summary>
        /// Constructor for creating a Member object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Member(string name, IMemberType contentType)
            : base(name, -1, contentType, new PropertyCollection())
        {
            _contentType = contentType;
        }

        //TODO: Should we just get rid of this one? no reason to have a level set.
        internal Member(string name, string email, string username, string password, int parentId, IMemberType contentType)
            : base(name, parentId, contentType, new PropertyCollection())
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentType = contentType;
            _email = email;
            _username = username;
            _password = password;
        }

        public Member(string name, string email, string username, string password, IMemberType contentType)
            : this(name, email, username, password, -1, contentType)
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentType = contentType;
            _email = email;
            _username = username;
            _password = password;
        }

        //public Member(string name, string email, string username, string password, IContentBase parent, IMemberType contentType)
        //    : base(name, parent, contentType, new PropertyCollection())
        //{
        //    Mandate.ParameterNotNull(contentType, "contentType");

        //    _contentType = contentType;
        //    _email = email;
        //    _username = username;
        //    _password = password;
        //}

        private static readonly PropertyInfo DefaultContentTypeAliasSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.ContentTypeAlias);
        private static readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Username);
        private static readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Email);
        private static readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Password);
        private static readonly PropertyInfo ProviderUserKeySelector = ExpressionHelper.GetPropertyInfo<Member, object>(x => x.ProviderUserKey);
        private static readonly PropertyInfo UserTypeKeySelector = ExpressionHelper.GetPropertyInfo<Member, Type>(x => x.ProviderUserKeyType);

        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        [DataMember]
        public string Username
        {
            get { return _username; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _username = value;
                    return _username;
                }, _username, UsernameSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        [DataMember]
        public string Email
        {
            get { return _email; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _email = value;
                    return _email;
                }, _email, EmailSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [DataMember]
        public string Password
        {
            get { return _password; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _password = value;
                    return _password;
                }, _password, PasswordSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Groups that Member is part of
        /// </summary>
        [DataMember]
        public IEnumerable<string> Groups { get; set; }

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
                var tryConvert = Properties[Constants.Conventions.Member.IsApproved].Value.TryConvertTo<bool>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(bool);
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
                var tryConvert = Properties[Constants.Conventions.Member.IsLockedOut].Value.TryConvertTo<bool>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(bool);
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
                var tryConvert = Properties[Constants.Conventions.Member.LastLoginDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
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
                var tryConvert = Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
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
                var tryConvert = Properties[Constants.Conventions.Member.LastLockoutDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
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
                var tryConvert = Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value.TryConvertTo<int>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(int);
            }
            set
            {
                Properties[Constants.Conventions.Member.LastLockoutDate].Value = value;
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

        /// <summary>
        /// User key from the Provider.
        /// </summary>
        /// <remarks>
        /// When using standard umbraco provider this key will 
        /// correspond to the guid UniqueId/Key.
        /// Otherwise it will the one available from the asp.net
        /// membership provider.
        /// </remarks>
        [DataMember]
        public virtual object ProviderUserKey
        {
            get
            {
                return _providerUserKey;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _providerUserKey = value;
                    return _providerUserKey;
                }, _providerUserKey, ProviderUserKeySelector);
            }
        }

        /// <summary>
        /// Gets or sets the type of the provider user key.
        /// </summary>
        /// <value>
        /// The type of the provider user key.
        /// </value>
        [IgnoreDataMember]
        internal Type ProviderUserKeyType
        {
            get
            {
                return _userTypeKey;
            }
            private set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _userTypeKey = value;
                    return _userTypeKey;
                }, _userTypeKey, UserTypeKeySelector);
            }
        }

        /// <summary>
        /// Sets the type of the provider user key.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SetProviderUserKeyType(Type type)
        {
            ProviderUserKeyType = type;
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public IMemberType ContentType
        {
            get { return _contentType; }
        }

        public override void ChangeTrashedState(bool isTrashed, int parentId = -20)
        {
            throw new NotSupportedException("Members can't be trashed as no Recycle Bin exists, so use of this method is invalid");
        }

        /* Internal experiment - only used for mapping queries. 
         * Adding these to have first level properties instead of the Properties collection.
         */
        internal string LongStringPropertyValue { get; set; }
        internal string ShortStringPropertyValue { get; set; }
        internal int IntegerropertyValue { get; set; }
        internal bool BoolPropertyValue { get; set; }
        internal DateTime DateTimePropertyValue { get; set; }
        internal string PropertyTypeAlias { get; set; }
    }
}