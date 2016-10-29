using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a backoffice user
    /// </summary>    
    [Serializable]
    [DataContract(IsReference = true)]
    public class User : Entity, IUser
    {
        public User()
        {
            SessionTimeout = 60;
            _groupCollection = new List<IUserGroup>();
            _language = GlobalSettings.DefaultUILanguage;
            _isApproved = true;
            _isLockedOut = false;
            _startContentId = -1;
            _startMediaId = -1;
            //cannot be null
            _rawPasswordValue = "";
        }

        public User(string name, string email, string username, string rawPasswordValue)
            : this()
        {
            _name = name;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            _isApproved = true;
            _isLockedOut = false;
            _startContentId = -1;
            _startMediaId = -1;
        }

        private string _name;
        private string _securityStamp;
        private List<IUserGroup> _groupCollection;
        private bool _groupsLoaded;
        private int _sessionTimeout;
        private int _startContentId;
        private int _startMediaId;
        private int _failedLoginAttempts;

        private string _username;
        private string _email;
        private string _rawPasswordValue;
        private bool _isApproved;
        private bool _isLockedOut;
        private string _language;
        private DateTime _lastPasswordChangedDate;
        private DateTime _lastLoginDate;
        private DateTime _lastLockoutDate;

        private bool _defaultToLiveEditing;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo FailedPasswordAttemptsSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.FailedPasswordAttempts);
            public readonly PropertyInfo LastLockoutDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastLockoutDate);
            public readonly PropertyInfo LastLoginDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastLoginDate);
            public readonly PropertyInfo LastPasswordChangeDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastPasswordChangeDate);

            public readonly PropertyInfo SecurityStampSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.SecurityStamp);
            public readonly PropertyInfo SessionTimeoutSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.SessionTimeout);
            public readonly PropertyInfo StartContentIdSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.StartContentId);
            public readonly PropertyInfo StartMediaIdSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.StartMediaId);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Name);

            public readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Username);
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Email);
            public readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.RawPasswordValue);
            public readonly PropertyInfo IsLockedOutSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsLockedOut);
            public readonly PropertyInfo IsApprovedSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsApproved);
            public readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Language);

            public readonly PropertyInfo DefaultToLiveEditingSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.DefaultToLiveEditing);
        }
        
        #region Implementation of IMembershipUser

        [IgnoreDataMember]
        public object ProviderUserKey
        {
            get { return Id; }
            set { throw new NotSupportedException("Cannot set the provider user key for a user"); }
        }


        [DataMember]
        public string Username
        {
            get { return _username; }
            set { SetPropertyValueAndDetectChanges(value, ref _username, Ps.Value.UsernameSelector); }
        }
        [DataMember]
        public string Email
        {
            get { return _email; }
            set { SetPropertyValueAndDetectChanges(value, ref _email, Ps.Value.EmailSelector); }
        }
        [DataMember]
        public string RawPasswordValue
        {
            get { return _rawPasswordValue; }
            set { SetPropertyValueAndDetectChanges(value, ref _rawPasswordValue, Ps.Value.PasswordSelector); }
        }

        [DataMember]
        public bool IsApproved
        {
            get { return _isApproved; }
            set { SetPropertyValueAndDetectChanges(value, ref _isApproved, Ps.Value.IsApprovedSelector); }
        }

        [IgnoreDataMember]
        public bool IsLockedOut
        {
            get { return _isLockedOut; }
            set { SetPropertyValueAndDetectChanges(value, ref _isLockedOut, Ps.Value.IsLockedOutSelector); }
        }

        [IgnoreDataMember]
        public DateTime LastLoginDate
        {
            get { return _lastLoginDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _lastLoginDate, Ps.Value.LastLoginDateSelector); }
        }

        [IgnoreDataMember]
        public DateTime LastPasswordChangeDate
        {
            get { return _lastPasswordChangedDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangedDate, Ps.Value.LastPasswordChangeDateSelector); }
        }

        [IgnoreDataMember]
        public DateTime LastLockoutDate
        {
            get { return _lastLockoutDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _lastLockoutDate, Ps.Value.LastLockoutDateSelector); }
        }

        [IgnoreDataMember]
        public int FailedPasswordAttempts
        {
            get { return _failedLoginAttempts; }
            set { SetPropertyValueAndDetectChanges(value, ref _failedLoginAttempts, Ps.Value.FailedPasswordAttemptsSelector); }
        }

        //TODO: Figure out how to support all of this! - we cannot have NotImplementedExceptions because these get used by the IMembershipMemberService<IUser> service so
        // we'll just have them as generic get/set which don't interact with the db.

        [IgnoreDataMember]
        public string PasswordQuestion { get; set; }
        [IgnoreDataMember]
        public string RawPasswordAnswerValue { get; set; }
        [IgnoreDataMember]
        public string Comments { get; set; }               
        
        #endregion
        
        #region Implementation of IUser

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        public IEnumerable<string> AllowedSections
        {
            get
            {
                if (GroupsLoaded == false)
                {
                    return Enumerable.Empty<string>();
                }

                return Groups
                    .SelectMany(x => x.AllowedSections)
                    .Distinct();
            }
        }

        public IProfile ProfileData
        {
            get { return new UserProfile(this); }
        }

        /// <summary>
        /// The security stamp used by ASP.Net identity
        /// </summary>
        [IgnoreDataMember]
        public string SecurityStamp
        {
            get { return _securityStamp; }
            set { SetPropertyValueAndDetectChanges(value, ref _securityStamp, Ps.Value.SecurityStampSelector); }
        }

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        /// <value>
        /// The session timeout.
        /// </value>
        [DataMember]
        public int SessionTimeout
        {
            get { return _sessionTimeout; }
            set { SetPropertyValueAndDetectChanges(value, ref _sessionTimeout, Ps.Value.SessionTimeoutSelector); }
        }

        /// <summary>
        /// Gets or sets the start content id.
        /// </summary>
        /// <value>
        /// The start content id.
        /// </value>
        [DataMember]
        public int StartContentId
        {
            get { return _startContentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _startContentId, Ps.Value.StartContentIdSelector); }
        }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>
        /// The start media id.
        /// </value>
        [DataMember]
        public int StartMediaId
        {
            get { return _startMediaId; }
            set { SetPropertyValueAndDetectChanges(value, ref _startMediaId, Ps.Value.StartMediaIdSelector); }
        }

        [DataMember]
        public string Language
        {
            get { return _language; }
            set { SetPropertyValueAndDetectChanges(value, ref _language, Ps.Value.LanguageSelector); }
        }

        [IgnoreDataMember]
        internal bool DefaultToLiveEditing
        {
            get { return _defaultToLiveEditing; }
            set { SetPropertyValueAndDetectChanges(value, ref _defaultToLiveEditing, Ps.Value.DefaultToLiveEditingSelector); }
        }

        /// <summary>
        /// Gets or sets the groups that user is part of
        /// </summary>
        [DataMember]
        public IEnumerable<IUserGroup> Groups
        {
            get { return _groupCollection; }
        }

        /// <summary>
        /// Indicates if the groups for a user have been loaded
        /// </summary>
        public bool GroupsLoaded { get { return _groupsLoaded; } }

        public void RemoveGroup(IUserGroup group)
        {
            if (_groupCollection.Select(x => x.Id).Contains(group.Id))
            {
                _groupCollection.Remove(group);
            }
        }

        public void AddGroup(IUserGroup group)
        {
            if (_groupCollection.Select(x => x.Id).Contains(group.Id) == false)
            {
                _groupCollection.Add(group);
            }
        }

        public void SetGroupsLoaded()
        {
            _groupsLoaded = true;
        }

        #endregion

        public override object DeepClone()
        {
            var clone = (User)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to create new collections otherwise they'll get copied by ref
            clone._groupCollection = new List<IUserGroup>(_groupCollection.ToList());
            //re-create the event handler
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }

        /// <summary>
        /// Internal class used to wrap the user in a profile
        /// </summary>
        private class UserProfile : IProfile
        {
            private readonly IUser _user;

            public UserProfile(IUser user)
            {
                _user = user;
            }

            public object Id
            {
                get { return _user.Id; }
                set { _user.Id = (int)value; }
            }

            public string Name
            {
                get { return _user.Name; }
                set { _user.Name = value; }
            }

            protected bool Equals(UserProfile other)
            {
                return _user.Equals(other._user);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UserProfile) obj);
            }

            public override int GetHashCode()
            {
                return _user.GetHashCode();
            }
        }
    }
}