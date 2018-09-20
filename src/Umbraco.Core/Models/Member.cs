﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Member object
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Member : ContentBase, IMember
    {
        private IDictionary<string, object> _additionalData;
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
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _contentTypeAlias = contentType.Alias;
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
            : base(name, -1, contentType, new PropertyCollection())
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));

            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _contentTypeAlias = contentType.Alias;
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
        public Member(string name, string email, string username, IMemberType contentType, bool isApproved = true)
            : base(name, -1, contentType, new PropertyCollection())
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullOrEmptyException(nameof(email));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullOrEmptyException(nameof(username));

            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _contentTypeAlias = contentType.Alias;
            _email = email;
            _username = username;
            IsApproved = isApproved;

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
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _contentTypeAlias = contentType.Alias;

            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            IsApproved = true;
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
        /// <param name="isApproved"></param>
        public Member(string name, string email, string username, string rawPasswordValue, IMemberType contentType, bool isApproved)
            : base(name, -1, contentType, new PropertyCollection())
        {
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _contentTypeAlias = contentType.Alias;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            IsApproved = isApproved;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Username);
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.Email);
            public readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<Member, string>(x => x.RawPasswordValue);
            public readonly PropertyInfo ProviderUserKeySelector = ExpressionHelper.GetPropertyInfo<Member, object>(x => x.ProviderUserKey);
        }

        /// <summary>
        /// Gets or sets the Username
        /// </summary>
        [DataMember]
        public string Username
        {
            get { return _username; }
            set { SetPropertyValueAndDetectChanges(value, ref _username, Ps.Value.UsernameSelector); }
        }

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        [DataMember]
        public string Email
        {
            get { return _email; }
            set { SetPropertyValueAndDetectChanges(value, ref _email, Ps.Value.EmailSelector); }
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
                if (value == null)
                {
                    //special case, this is used to ensure that the password is not updated when persisting, in this case
                    //we don't want to track changes either
                    _rawPasswordValue = null;
                }
                else
                {
                    SetPropertyValueAndDetectChanges(value, ref _rawPasswordValue, Ps.Value.PasswordSelector);
                }
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

                return Properties[Constants.Conventions.Member.PasswordQuestion].GetValue() == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordQuestion].GetValue().ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.PasswordQuestion,
                    "PasswordQuestion") == false) return;

                Properties[Constants.Conventions.Member.PasswordQuestion].SetValue(value);
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

                return Properties[Constants.Conventions.Member.PasswordAnswer].GetValue() == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.PasswordAnswer].GetValue().ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.PasswordAnswer,
                    "PasswordAnswer") == false) return;

                Properties[Constants.Conventions.Member.PasswordAnswer].SetValue(value);
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

                return Properties[Constants.Conventions.Member.Comments].GetValue() == null
                    ? string.Empty
                    : Properties[Constants.Conventions.Member.Comments].GetValue().ToString();
            }
            set
            {
                if (WarnIfPropertyTypeNotFoundOnSet(
                    Constants.Conventions.Member.Comments,
                    "Comments") == false) return;

                Properties[Constants.Conventions.Member.Comments].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.IsApproved].GetValue() == null) return true;
                var tryConvert = Properties[Constants.Conventions.Member.IsApproved].GetValue().TryConvertTo<bool>();
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

                Properties[Constants.Conventions.Member.IsApproved].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.IsLockedOut].GetValue() == null) return false;
                var tryConvert = Properties[Constants.Conventions.Member.IsLockedOut].GetValue().TryConvertTo<bool>();
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

                Properties[Constants.Conventions.Member.IsLockedOut].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.LastLoginDate].GetValue() == null) return default(DateTime);
                var tryConvert = Properties[Constants.Conventions.Member.LastLoginDate].GetValue().TryConvertTo<DateTime>();
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

                Properties[Constants.Conventions.Member.LastLoginDate].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.LastPasswordChangeDate].GetValue() == null) return default(DateTime);
                var tryConvert = Properties[Constants.Conventions.Member.LastPasswordChangeDate].GetValue().TryConvertTo<DateTime>();
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

                Properties[Constants.Conventions.Member.LastPasswordChangeDate].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.LastLockoutDate].GetValue() == null) return default(DateTime);
                var tryConvert = Properties[Constants.Conventions.Member.LastLockoutDate].GetValue().TryConvertTo<DateTime>();
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

                Properties[Constants.Conventions.Member.LastLockoutDate].SetValue(value);
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
                if (Properties[Constants.Conventions.Member.FailedPasswordAttempts].GetValue() == null) return default(int);
                var tryConvert = Properties[Constants.Conventions.Member.FailedPasswordAttempts].GetValue().TryConvertTo<int>();
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

                Properties[Constants.Conventions.Member.FailedPasswordAttempts].SetValue(value);
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
            set { SetPropertyValueAndDetectChanges(value, ref _providerUserKey, Ps.Value.ProviderUserKeySelector); }
        }


        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (ProviderUserKey == null)
                ProviderUserKey = Key;
        }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        [IgnoreDataMember]
        public IMemberType ContentType
        {
            get { return _contentType; }
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
            void DoLog(string logPropertyAlias, string logPropertyName)
            {
                Current.Logger.Warn<Member>("Trying to access the '{PropertyName}' property on '{MemberType}' " +
                                            "but the {PropertyAlias} property does not exist on the member type so a default value is returned. " +
                                            "Ensure that you have a property type with alias:  {PropertyAlias} configured on your member type in order to use the '{PropertyName}' property on the model correctly.",
                                                logPropertyName,
                                                typeof(Member),
                                                logPropertyAlias);
            }

            // if the property doesn't exist,
            if (Properties.Contains(propertyAlias) == false)
            {
                // put a warn in the log if this entity has been persisted
                // then return a failure
                if (HasIdentity)
                    DoLog(propertyAlias, propertyName);
                return Attempt<T>.Fail(defaultVal);
            }

            return Attempt<T>.Succeed();
        }

        private bool WarnIfPropertyTypeNotFoundOnSet(string propertyAlias, string propertyName)
        {
            void DoLog(string logPropertyAlias, string logPropertyName)
            {
                Current.Logger.Warn<Member>("An attempt was made to set a value on the property '{PropertyName}' on type '{MemberType}' but the " +
                                            "property type {PropertyAlias} does not exist on the member type, ensure that this property type exists so that setting this property works correctly.",
                                                logPropertyName,
                                                typeof(Member),
                                                logPropertyAlias);
            }

            // if the property doesn't exist,
            if (Properties.Contains(propertyAlias) == false)
            {
                // put a warn in the log if this entity has been persisted
                // then return a failure
                if (HasIdentity)
                    DoLog(propertyAlias, propertyName);
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

        /// <inheritdoc />
        [DataMember]
        [DoNotClone]
        public IDictionary<string, object> AdditionalData => _additionalData ?? (_additionalData = new Dictionary<string, object>());

        /// <inheritdoc />
        [IgnoreDataMember]
        public bool HasAdditionalData => _additionalData != null;
    }
}
