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
        public User(IUserType userType)
        {
            if (userType == null) throw new ArgumentNullException("userType");

            _userType = userType;
            _defaultPermissions = _userType.Permissions == null ? Enumerable.Empty<string>() : new List<string>(_userType.Permissions);
            //Groups = new List<object> { userType };
            SessionTimeout = 60;
            _sectionCollection = new ObservableCollection<string>();
            _addedSections = new List<string>();
            _removedSections = new List<string>();
            _language = GlobalSettings.DefaultUILanguage;
            _sectionCollection.CollectionChanged += SectionCollectionChanged;
            _isApproved = true;
            _isLockedOut = false;
            _startContentId = -1;
            _startMediaId = -1;
            //cannot be null
            _rawPasswordValue = "";
        }

        public User(string name, string email, string username, string rawPasswordValue, IUserType userType)
            : this(userType)
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

        private IUserType _userType;
        private string _name;
        private string _securityStamp;
        private List<string> _addedSections;
        private List<string> _removedSections;
        private ObservableCollection<string> _sectionCollection;
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

        private IEnumerable<string> _defaultPermissions; 
        
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
            public readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<User, IEnumerable<string>>(x => x.AllowedSections);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Name);

            public readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Username);
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Email);
            public readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.RawPasswordValue);
            public readonly PropertyInfo IsLockedOutSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsLockedOut);
            public readonly PropertyInfo IsApprovedSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsApproved);
            public readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Language);

            public readonly PropertyInfo DefaultToLiveEditingSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.DefaultToLiveEditing);
            public readonly PropertyInfo UserTypeSelector = ExpressionHelper.GetPropertyInfo<User, IUserType>(x => x.UserType);
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
            get { return _sectionCollection; }
        }

        public void RemoveAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias))
            {
                _sectionCollection.Remove(sectionAlias);
            }
        }

        public void AddAllowedSection(string sectionAlias)
        {
            if (_sectionCollection.Contains(sectionAlias) == false)
            {
                _sectionCollection.Add(sectionAlias);
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
        /// Used internally to check if we need to add a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> AddedSections
        {
            get { return _addedSections; }
        }

        /// <summary>
        /// Used internally to check if we need to remove  a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> RemovedSections
        {
            get { return _removedSections; }
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

        //TODO: This should be a private set
        [DataMember]
        public IEnumerable<string> DefaultPermissions
        {
            get {  return _defaultPermissions;}
            set { _defaultPermissions = value; }
        }

        [IgnoreDataMember]
        internal bool DefaultToLiveEditing
        {
            get { return _defaultToLiveEditing; }
            set { SetPropertyValueAndDetectChanges(value, ref _defaultToLiveEditing, Ps.Value.DefaultToLiveEditingSelector); }
        }

        [IgnoreDataMember]
        public IUserType UserType
        {
            get { return _userType; }
            set
            {
                if (value.HasIdentity == false)
                {
                    throw new InvalidOperationException("Cannot assign a User Type that has not been persisted");
                }

                SetPropertyValueAndDetectChanges(value, ref _userType, Ps.Value.UserTypeSelector);                
            }
        }

        #endregion

        /// <summary>
        /// Whenever resetting occurs, clear the remembered add/removed collections, even if 
        /// rememberPreviouslyChangedProperties is true, the AllowedSections property will still
        /// be flagged as dirty.
        /// </summary>
        /// <param name="rememberPreviouslyChangedProperties"></param>
        public override void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _addedSections.Clear();
            _removedSections.Clear();
            base.ResetDirtyProperties(rememberPreviouslyChangedProperties);
        }

        /// <summary>
        /// Handles the collection changed event in order for us to flag the AllowedSections property as changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.AllowedSectionsSelector);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var item = e.NewItems.Cast<string>().First();

                if (_addedSections.Contains(item) == false)
                {
                    _addedSections.Add(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = e.OldItems.Cast<string>().First();

                if (_removedSections.Contains(item) == false)
                {
                    _removedSections.Add(item);    
                }

            }
        }

        public override object DeepClone()
        {
            var clone = (User)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to create new collections otherwise they'll get copied by ref
            clone._addedSections = new List<string>();
            clone._removedSections = new List<string>();
            clone._sectionCollection = new ObservableCollection<string>(_sectionCollection.ToList());
            clone._defaultPermissions = new List<string>(_defaultPermissions.ToList());
            //re-create the event handler
            clone._sectionCollection.CollectionChanged += clone.SectionCollectionChanged;
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