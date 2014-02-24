using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Logging;

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

        //TODO: When get/setting all of these properties we MUST:
        // * Check if we are using the umbraco membership provider, if so then we need to use the configured fields - not the explicit fields below
        // * If any of the fields don't exist, what should we do? Currently it will throw an exception!

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.PasswordQuestion, "PasswordQuestion", default(string));
                if (a.Success == false) return a.Result;

                return Properties[Constants.Conventions.Member.PasswordQuestion].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordQuestion].Value.ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.PasswordQuestion,
                    "PasswordQuestion") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.PasswordAnswer, "PasswordAnswer", default(string));
                if (a.Success == false) return a.Result;

                return Properties[Constants.Conventions.Member.PasswordAnswer].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordAnswer].Value.ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.PasswordAnswer,
                    "PasswordAnswer") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.Comments, "Comments", default(string));
                if (a.Success == false) return a.Result;

                return Properties[Constants.Conventions.Member.Comments].Value == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.Comments].Value.ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.Comments,
                    "Comments") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.IsApproved, "IsApproved", true);
                if (a.Success == false) return a.Result;

                var tryConvert = Properties[Constants.Conventions.Member.IsApproved].Value.TryConvertTo<bool>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(bool);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.IsApproved,
                    "IsApproved") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.IsLockedOut, "IsLockedOut", false);
                if (a.Success == false) return a.Result;
                
                var tryConvert = Properties[Constants.Conventions.Member.IsLockedOut].Value.TryConvertTo<bool>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(bool);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.IsLockedOut,
                    "IsLockedOut") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.LastLoginDate, "LastLoginDate", default(DateTime));
                if (a.Success == false) return a.Result;
                
                var tryConvert = Properties[Constants.Conventions.Member.LastLoginDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.LastLoginDate,
                    "LastLoginDate") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.LastPasswordChangeDate, "LastPasswordChangeDate", default(DateTime));
                if (a.Success == false) return a.Result;

                var tryConvert = Properties[Constants.Conventions.Member.LastPasswordChangeDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.LastPasswordChangeDate,
                    "LastPasswordChangeDate") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.LastLockoutDate, "LastLockoutDate", default(DateTime));
                if (a.Success == false) return a.Result;
                
                var tryConvert = Properties[Constants.Conventions.Member.LastLockoutDate].Value.TryConvertTo<DateTime>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(DateTime);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.LastLockoutDate,
                    "LastLockoutDate") == false) return;

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
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.FailedPasswordAttempts, "FailedPasswordAttempts", 0);
                if (a.Success == false) return a.Result;
                
                var tryConvert = Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value.TryConvertTo<int>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                return default(int);
                //TODO: Use TryConvertTo<T> instead
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.FailedPasswordAttempts,
                    "FailedPasswordAttempts") == false) return;

                Properties[Constants.Conventions.Member.FailedPasswordAttempts].Value = value;
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

        private Attempt<T> WarnIfPropertyTypeNotFoundOnGet<T>(string propertyAlias, string propertyName, T defaultVal)
        {
            Action doLog = () => LogHelper.Warn<Member>(
                        "Trying to access the '"
                        + propertyName
                        + "' property on "
                        + typeof(Member)
                        + " but the "
                        + propertyAlias
                        + " property does not exist on the member type so a default value is returned. Ensure that you have a property type with alias: "
                        + propertyAlias
                        + " configured on your member type in order to use the '"
                        + propertyName
                        + "' property on the model correctly.");     

            //if the property doesn't exist, then do the logging and return a failure
            if (Properties.Contains(propertyAlias) == false)
            {
                //we'll put a warn in the log if this entity has been persisted
                if (HasIdentity)
                {
                    doLog();
                }
                return Attempt<T>.Fail(defaultVal);
            }

            //if the property doesn't have an identity but we do, then do logging and return failure
            var prop = Properties.Single(x => x.Alias == propertyAlias);
            if (prop.HasIdentity == false && HasIdentity)
            {
                doLog();
                return Attempt<T>.Fail(defaultVal);
            }

            return Attempt<T>.Succeed();
        }

        private bool WarnIfPropertyTypeNotFoundOnSet(string propertyAlias, string propertyname)
        {
            Action doLog = () => LogHelper.Warn<Member>("An attempt was made to set a value on the property '"
                        + propertyname
                        + "' on type "
                        + typeof(Member)
                        + " but the property type "
                        + propertyAlias
                        + " does not exist on the member type, ensure that this property type exists so that setting this property works correctly.");

            //if the property doesn't exist, then do the logging and return a failure
            if (Properties.Contains(propertyAlias) == false)
            {
                if (HasIdentity)
                {
                    doLog();
                }
                return false;
            }

            //if the property doesn't have an identity but we do, then do logging and return failure
            var prop = Properties.Single(x => x.Alias == propertyAlias);
            if (prop.HasIdentity == false && HasIdentity)
            {
                doLog();
                return false;
            }

            return true;
        }
    }
}