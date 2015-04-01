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
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
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

        private string _username;
        private string _email;
        private string _rawPasswordValue;
        private bool _isApproved;
        private bool _isLockedOut;
        private string _language;

        private IEnumerable<string> _defaultPermissions; 
        
        private bool _defaultToLiveEditing;

        private static readonly PropertyInfo SecurityStampSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.SecurityStamp);
        private static readonly PropertyInfo SessionTimeoutSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.SessionTimeout);
        private static readonly PropertyInfo StartContentIdSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.StartContentId);
        private static readonly PropertyInfo StartMediaIdSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.StartMediaId);
        private static readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<User, IEnumerable<string>>(x => x.AllowedSections);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Name);
        
        private static readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Username);
        private static readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Email);
        private static readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.RawPasswordValue);
        private static readonly PropertyInfo IsLockedOutSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsLockedOut);
        private static readonly PropertyInfo IsApprovedSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsApproved);
        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Language);

        private static readonly PropertyInfo DefaultToLiveEditingSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.DefaultToLiveEditing);
        private static readonly PropertyInfo UserTypeSelector = ExpressionHelper.GetPropertyInfo<User, IUserType>(x => x.UserType);
        
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _username = value;
                    return _username;
                }, _username, UsernameSelector);
            }
        }
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
        [DataMember]
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

        [DataMember]
        public bool IsApproved
        {
            get { return _isApproved; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isApproved = value;
                    return _isApproved;
                }, _isApproved, IsApprovedSelector);
            }
        }    

        [DataMember]
        public bool IsLockedOut
        {
            get { return _isLockedOut; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _isLockedOut = value;
                    return _isLockedOut;
                }, _isLockedOut, IsLockedOutSelector);
            }
        }

        //TODO: Figure out how to support all of this! - we cannot have NotImplementedExceptions because these get used by the IMembershipMemberService<IUser> service so
        // we'll just have them as generic get/set which don't interact with the db.

        [IgnoreDataMember]
        public string PasswordQuestion { get; set; }
        [IgnoreDataMember]
        public string RawPasswordAnswerValue { get; set; }
        [IgnoreDataMember]
        public string Comments { get; set; }        
        [IgnoreDataMember]
        public DateTime LastLoginDate { get; set; }
        [IgnoreDataMember]
        public DateTime LastPasswordChangeDate { get; set; }
        [IgnoreDataMember]
        public DateTime LastLockoutDate { get; set; }
        [IgnoreDataMember]
        public int FailedPasswordAttempts { get; set; }
        
        #endregion
        
        #region Implementation of IUser

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
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
        public string SecurityStamp
        {
            get { return _securityStamp; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _securityStamp = value;
                    return _securityStamp;
                }, _securityStamp, SecurityStampSelector);
            }
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                    {
                        _sessionTimeout = value;
                        return _sessionTimeout;
                    }, _sessionTimeout, SessionTimeoutSelector);
            }
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                    {
                        _startContentId = value;
                        return _startContentId;
                    }, _startContentId, StartContentIdSelector);
            }
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                    {
                        _startMediaId = value;
                        return _startMediaId;
                    }, _startMediaId, StartMediaIdSelector);
            }
        }

        [DataMember]
        public string Language
        {
            get { return _language; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _language = value;
                    return _language;
                }, _language, LanguageSelector);
            }
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _defaultToLiveEditing = value;
                    return _defaultToLiveEditing;
                }, _defaultToLiveEditing, DefaultToLiveEditingSelector);
            }
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

                SetPropertyValueAndDetectChanges(o =>
                {
                    _userType = value;
                    return _userType;
                }, _userType, UserTypeSelector);
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
            OnPropertyChanged(AllowedSectionsSelector);

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