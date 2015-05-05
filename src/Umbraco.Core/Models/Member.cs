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
        private IMemberType _contentType;
        private readonly string _contentTypeAlias;
        private string _username;
        private string _email;
        private string _rawPasswordValue;
        private object _providerUserKey;

        /// <summary>
        /// Constructor for creating an empty Member object
        /// </summary>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Member(IMemberType contentType)
            : base("", -1, contentType, new PropertyCollection())
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentTypeAlias = contentType.Alias;
            _contentType = contentType;
            IsApproved = true;

            //this cannot be null but can be empty
            _rawPasswordValue = "";
            _email = "";
            _username = "";
        }

        /// <summary>
        /// Constructor for creating a Member object
        /// </summary>
        /// <param name="name">Name of the content</param>
        /// <param name="contentType">ContentType for the current Content object</param>
        public Member(string name, IMemberType contentType)
            : this(contentType)
        {
            Mandate.ParameterNotNull(contentType, "contentType");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            _contentTypeAlias = contentType.Alias;
            _contentType = contentType;
            IsApproved = true;

            //this cannot be null but can be empty
            _rawPasswordValue = "";
            _email = "";
            _username = "";
        }

        /// <summary>
        /// Constructor for creating a Member object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="contentType"></param>
        public Member(string name, string email, string username, IMemberType contentType)
            : base(name, -1, contentType, new PropertyCollection())
        {
            Mandate.ParameterNotNull(contentType, "contentType");
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Mandate.ParameterNotNullOrEmpty(email, "email");
            Mandate.ParameterNotNullOrEmpty(username, "username");

            _contentTypeAlias = contentType.Alias;
            _contentType = contentType;
            _email = email;
            _username = username;
            IsApproved = true;

            //this cannot be null but can be empty
            _rawPasswordValue = "";
        }

        /// <summary>
        /// Constructor for creating a Member object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="rawPasswordValue">
        /// The password value passed in to this parameter should be the encoded/encrypted/hashed format of the member's password
        /// </param>
        /// <param name="contentType"></param>
        public Member(string name, string email, string username, string rawPasswordValue, IMemberType contentType)
            : base(name, -1, contentType, new PropertyCollection())
        {
            Mandate.ParameterNotNull(contentType, "contentType");

            _contentTypeAlias = contentType.Alias;
            _contentType = contentType;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            IsApproved = true;
        }

        private static readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Username);
        private static readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Email);
        private static readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.RawPasswordValue);
        private static readonly PropertyInfo ProviderUserKeySelector = ExpressionHelper.GetPropertyInfo<Member, object>(x => x.ProviderUserKey);

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
        /// Gets or sets the raw password value
        /// </summary>
        [IgnoreDataMember]
        public string RawPasswordValue
        {
            get { return _rawPasswordValue; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _rawPasswordValue = value;
                    return _rawPasswordValue;
                }, _rawPasswordValue, PasswordSelector);
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
        /// Alias: umbracoMemberPasswordRetrievalQuestion
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Gets or sets the raw password answer value
        /// </summary>
        /// <remarks>
        /// For security reasons this value should be encrypted, the encryption process is handled by the memberhip provider
        /// Alias: umbracoMemberPasswordRetrievalAnswer
        /// 
        /// Part of the standard properties collection.
        /// </remarks>
        [IgnoreDataMember]
        public string RawPasswordAnswerValue
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
        /// Alias: umbracoMemberComments
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Alias: umbracoMemberApproved
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
        public bool IsApproved
        {
            get
            {
                var a = WarnIfPropertyTypeNotFoundOnGet(Constants.Conventions.Member.IsApproved, "IsApproved", 
                    //This is the default value if the prop is not found
                    true);
                if (a.Success == false) return a.Result;

                var tryConvert = Properties[Constants.Conventions.Member.IsApproved].Value.TryConvertTo<bool>();
                if (tryConvert.Success)
                {
                    return tryConvert.Result;
                }
                //if the property exists but it cannot be converted, we will assume true
                return true;
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
        /// Alias: umbracoMemberLockedOut
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
                return false;
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
        /// Alias: umbracoMemberLastLogin
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Alias: umbracoMemberLastPasswordChangeDate
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Alias: umbracoMemberLastLockoutDate
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Alias: umbracoMemberFailedPasswordAttempts
        /// Part of the standard properties collection.
        /// </remarks>
        [DataMember]
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
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
            {
                Key = Guid.NewGuid();
                ProviderUserKey = Key;
            }
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
        [IgnoreDataMember]
        internal string LongStringPropertyValue { get; set; }
        [IgnoreDataMember]
        internal string ShortStringPropertyValue { get; set; }
        [IgnoreDataMember]
        internal int IntegerPropertyValue { get; set; }
        [IgnoreDataMember]
        internal bool BoolPropertyValue { get; set; }
        [IgnoreDataMember]
        internal DateTime DateTimePropertyValue { get; set; }
        [IgnoreDataMember]
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

        public override object DeepClone()
        {
            var clone = (Member)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually clone this since it's not settable
            clone._contentType = (IMemberType)ContentType.DeepClone();
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;

        }
    }
}