using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a backoffice user
    /// </summary>    
    [Serializable]
    [DataContract(IsReference = true)]
    public class User : Entity, IUser, IProfile
    {
        /// <summary>
        /// Constructor for creating a new/empty user
        /// </summary>
        public User()
        {
            SessionTimeout = 60;
            _userGroups = new HashSet<IReadOnlyUserGroup>();
            _language = GlobalSettings.DefaultUILanguage;
            _isApproved = true;
            _isLockedOut = false;
            _startContentIds = new int[] { };
            _startMediaIds = new int[] { };
            //cannot be null
            _rawPasswordValue = "";
        }

        /// <summary>
        /// Constructor for creating a new/empty user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="rawPasswordValue"></param>
        public User(string name, string email, string username, string rawPasswordValue)
            : this()
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Value cannot be null or whitespace.", "email");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", "username");
            if (string.IsNullOrEmpty(rawPasswordValue)) throw new ArgumentException("Value cannot be null or empty.", "rawPasswordValue");

            _name = name;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            _userGroups = new HashSet<IReadOnlyUserGroup>();
            _isApproved = true;
            _isLockedOut = false;
            _startContentIds = new int[] { };
            _startMediaIds = new int[] { };
            
        }

        /// <summary>
        /// Constructor for creating a new User instance for an existing user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="rawPasswordValue"></param>
        /// <param name="userGroups"></param>
        /// <param name="startContentIds"></param>
        /// <param name="startMediaIds"></param>
        public User(int id, string name, string email, string username, string rawPasswordValue, IEnumerable<IReadOnlyUserGroup> userGroups, int[] startContentIds, int[] startMediaIds)
            : this()
        {
            //we allow whitespace for this value so just check null
            if (rawPasswordValue == null) throw new ArgumentNullException("rawPasswordValue");
            if (userGroups == null) throw new ArgumentNullException("userGroups");
            if (startContentIds == null) throw new ArgumentNullException("startContentIds");
            if (startMediaIds == null) throw new ArgumentNullException("startMediaIds");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", "username");            

            Id = id;
            _name = name;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            _userGroups = new HashSet<IReadOnlyUserGroup>(userGroups);
            _isApproved = true;
            _isLockedOut = false;
            _startContentIds = startContentIds;
            _startMediaIds = startMediaIds;
        }

        private string _name;
        private string _securityStamp;
        private string _avatar;
        private string _tourData;
        private int _sessionTimeout;
        private int[] _startContentIds;
        private int[] _startMediaIds;
        private int _failedLoginAttempts;

        private string _username;
        private DateTime? _emailConfirmedDate;
        private DateTime? _invitedDate;
        private string _email;
        private string _rawPasswordValue;
        private IEnumerable<string> _allowedSections;
        private HashSet<IReadOnlyUserGroup> _userGroups;
        private bool _isApproved;
        private bool _isLockedOut;
        private string _language;
        private DateTime _lastPasswordChangedDate;
        private DateTime _lastLoginDate;
        private DateTime _lastLockoutDate;
        private bool _defaultToLiveEditing;
        private IDictionary<string, object> _additionalData;
        private object _additionalDataLock = new object();

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo FailedPasswordAttemptsSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.FailedPasswordAttempts);
            public readonly PropertyInfo LastLockoutDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastLockoutDate);
            public readonly PropertyInfo LastLoginDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastLoginDate);
            public readonly PropertyInfo LastPasswordChangeDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime>(x => x.LastPasswordChangeDate);

            public readonly PropertyInfo SecurityStampSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.SecurityStamp);
            public readonly PropertyInfo AvatarSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Avatar);
            public readonly PropertyInfo TourDataSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.TourData);
            public readonly PropertyInfo SessionTimeoutSelector = ExpressionHelper.GetPropertyInfo<User, int>(x => x.SessionTimeout);
            public readonly PropertyInfo StartContentIdSelector = ExpressionHelper.GetPropertyInfo<User, int[]>(x => x.StartContentIds);
            public readonly PropertyInfo StartMediaIdSelector = ExpressionHelper.GetPropertyInfo<User, int[]>(x => x.StartMediaIds);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Name);

            public readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Username);
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Email);
            public readonly PropertyInfo PasswordSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.RawPasswordValue);
            public readonly PropertyInfo IsLockedOutSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsLockedOut);
            public readonly PropertyInfo IsApprovedSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.IsApproved);
            public readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<User, string>(x => x.Language);
            public readonly PropertyInfo EmailConfirmedDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime?>(x => x.EmailConfirmedDate);
            public readonly PropertyInfo InvitedDateSelector = ExpressionHelper.GetPropertyInfo<User, DateTime?>(x => x.InvitedDate);

            public readonly PropertyInfo DefaultToLiveEditingSelector = ExpressionHelper.GetPropertyInfo<User, bool>(x => x.DefaultToLiveEditing);

            public readonly PropertyInfo UserGroupsSelector = ExpressionHelper.GetPropertyInfo<User, IEnumerable<IReadOnlyUserGroup>>(x => x.Groups);

            //Custom comparer for enumerable
            public readonly DelegateEqualityComparer<IEnumerable<int>> IntegerEnumerableComparer =
                new DelegateEqualityComparer<IEnumerable<int>>(
                    (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
                    enum1 => enum1.GetHashCode());
        }
        
        #region Implementation of IMembershipUser

        [IgnoreDataMember]
        public object ProviderUserKey
        {
            get { return Id; }
            set { throw new NotSupportedException("Cannot set the provider user key for a user"); }
        }
        
        [DataMember]
        public DateTime? EmailConfirmedDate
        {
            get { return _emailConfirmedDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _emailConfirmedDate, Ps.Value.EmailConfirmedDateSelector); }
        }
        [DataMember]
        public DateTime? InvitedDate
        {
            get { return _invitedDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _invitedDate, Ps.Value.InvitedDateSelector); }
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

        public UserState UserState
        {
            get
            {
                if (LastLoginDate == default && IsApproved == false && InvitedDate != null)
                    return UserState.Invited;

                if (IsLockedOut)
                    return UserState.LockedOut;
                if (IsApproved == false)
                    return UserState.Disabled;

                // User is not disabled or locked and has never logged in before
                if (LastLoginDate == default && IsApproved && IsLockedOut == false)
                    return UserState.Inactive;

                    return UserState.Active;
            }
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        public IEnumerable<string> AllowedSections
        {
            get { return _allowedSections ?? (_allowedSections = new List<string>(_userGroups.SelectMany(x => x.AllowedSections).Distinct())); }
        }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IUserType IUser.UserType
        {
            get
            {
                //the best we can do here is to return the user's first user group as a IUserType object
                //but we should attempt to return any group that is the built in ones first
                var groups = Groups.ToArray();
                if (groups.Length == 0)
                {
                    //In backwards compatibility land, a user type cannot be null! so we need to return a fake one. 
                    return new UserType
                    {
                        Alias = "temp",
                        Id = int.MinValue,
                        Key = Guid.Empty,
                        CreateDate = default(DateTime),
                        DeletedDate = null,
                        Name = "Temp",
                        Permissions = new List<string>(),
                        UpdateDate = default(DateTime)
                    };
                }
                var builtIns = new[] { Constants.Security.AdminGroupAlias, "writer", "editor", "translator" };
                var foundBuiltIn = groups.FirstOrDefault(x => builtIns.Contains(x.Alias));
                IUserGroup realGroup;
                if (foundBuiltIn != null)
                {
                    //if the group isn't IUserGroup we'll need to look it up
                    realGroup = foundBuiltIn as IUserGroup ?? ApplicationContext.Current.Services.UserService.GetUserGroupById(foundBuiltIn.Id);

                    //return a mapped version of the group
                    return new UserType
                    {
                        Alias = realGroup.Alias,
                        Id = realGroup.Id,
                        Key = realGroup.Key,
                        CreateDate = realGroup.CreateDate,
                        DeletedDate = realGroup.DeletedDate,
                        Name = realGroup.Name,
                        Permissions = realGroup.Permissions,
                        UpdateDate = realGroup.UpdateDate
                    };
                }

                //otherwise return the first
                //if the group isn't IUserGroup we'll need to look it up
                realGroup = groups[0] as IUserGroup ?? ApplicationContext.Current.Services.UserService.GetUserGroupById(groups[0].Id);
                //return a mapped version of the group
                return new UserType
                {
                    Alias = realGroup.Alias,
                    Id = realGroup.Id,
                    Key = realGroup.Key,
                    CreateDate = realGroup.CreateDate,
                    DeletedDate = realGroup.DeletedDate,
                    Name = realGroup.Name,
                    Permissions = realGroup.Permissions,
                    UpdateDate = realGroup.UpdateDate
                };
            }
            set
            {
                //if old APIs are still using this lets first check if  the user is part of the user group with the alias specified
                if (Groups.Any(x => x.Alias == value.Alias))
                    return;

                //the only other option we have here is to lookup the group (and we'll need to use singletons here :( )
                var found = ApplicationContext.Current.Services.UserService.GetUserGroupByAlias(value.Alias);
                if (found == null)
                    throw new InvalidOperationException("No user group was found with the alias " + value.Alias + ", this API (IUser.UserType) is obsolete, use user groups instead");

                //if it's found, all we can do is add it, we can't really replace them
                AddGroup(found.ToReadOnlyGroup());
            }
        }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IUser.RemoveAllowedSection(string sectionAlias)
        {
            //don't do anything if they aren't allowed it already
            if (AllowedSections.Contains(sectionAlias) == false)
                return;

            var groups = Groups.ToArray();
            //our only option here is to check if a custom group is created for this user, if so we can remove it from that group, otherwise we'll throw
            //now we'll check if the user has a special 1:1 user group created for itself. This will occur if this method is used and also during an upgrade.
            //this comes in the alias form of userName + 'Group'
            var customUserGroup = groups.FirstOrDefault(x => x.Alias == (Username + "Group"));
            if (customUserGroup != null)
            {
                //if the group isn't IUserGroup we'll need to look it up
                var realGroup = customUserGroup as IUserGroup ?? ApplicationContext.Current.Services.UserService.GetUserGroupById(customUserGroup.Id);
                realGroup.RemoveAllowedSection(sectionAlias);
                //now we need to flag this for saving (hack!) 
                GroupsToSave.Add(realGroup);
            }
            else
            {
                throw new InvalidOperationException("Cannot remove the allowed section using this obsolete API. Modify the user's groups instead");
            }
            
        }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IUser.AddAllowedSection(string sectionAlias)
        {
            //don't do anything if they are allowed it already
            if (AllowedSections.Contains(sectionAlias))
                return;

            //This is here for backwards compat only.
            //First we'll check if the user is part of the 'admin' group. If so then we can ensure that the admin group has this section available to it.
            //otherwise, the only thing we can do is create a custom user group for this user and add this section.
            //We are checking for admin here because if the user is an admin and an allowed section is being added, then it's assumed it's to be added 
            //for the whole admin group (i.e. Forms installer does this for admins)
            var groups = Groups.ToArray();
            var admin = groups.FirstOrDefault(x => x.Alias == Constants.Security.AdminGroupAlias);
            if (admin != null)
            {
                //if the group isn't IUserGroup we'll need to look it up
                var realGroup = admin as IUserGroup ?? ApplicationContext.Current.Services.UserService.GetUserGroupById(admin.Id);
                realGroup.AddAllowedSection(sectionAlias);
                //now we need to flag this for saving (hack!) 
                GroupsToSave.Add(realGroup);
            }
            else
            {
                //now we'll check if the user has a special 1:1 user group created for itself. This will occur if this method is used and also during an upgrade.
                //this comes in the alias form of userName + 'Group'
                var customUserGroup = groups.FirstOrDefault(x => x.Alias == (Username + "Group"));
                if (customUserGroup != null)
                {
                    //if the group isn't IUserGroup we'll need to look it up
                    var realGroup = customUserGroup as IUserGroup ?? ApplicationContext.Current.Services.UserService.GetUserGroupById(customUserGroup.Id);
                    realGroup.AddAllowedSection(sectionAlias);
                    //now we need to flag this for saving (hack!) 
                    GroupsToSave.Add(realGroup);
                }

                //ok, so the user doesn't have a 1:1 group, we'll need to flag it for creation
                var newUserGroup = new UserGroup
                {
                    Alias = Username + "Group",
                    Name = "Group for " + Username
                };
                newUserGroup.AddAllowedSection(sectionAlias);
                //add this user to this new group
                AddGroup(newUserGroup);
                GroupsToSave.Add(newUserGroup);
            }
        }

        /// <summary>
        /// This used purely for hacking backwards compatibility into this class for &lt; 7.7 compat
        /// </summary>
        [DoNotClone]
        [IgnoreDataMember]
        internal List<IUserGroup> GroupsToSave = new List<IUserGroup>();        

        public IProfile ProfileData
        {
            get { return new WrappedUserProfile(this); }
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

        [DataMember]
        public string Avatar
        {
            get { return _avatar; }
            set { SetPropertyValueAndDetectChanges(value, ref _avatar, Ps.Value.AvatarSelector); }
        }

        /// <summary>
        /// A Json blob stored for recording tour data for a user
        /// </summary>
        [DataMember]
        public string TourData
        {
            get { return _tourData; }
            set { SetPropertyValueAndDetectChanges(value, ref _tourData, Ps.Value.TourDataSelector); }
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

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int IUser.StartContentId
        {
            get
            {
                var calculatedStartNodes = this.CalculateContentStartNodeIds(ApplicationContext.Current.Services.EntityService);
                return calculatedStartNodes.Length == 0 ? -1 : calculatedStartNodes[0];
            }
            set { StartContentIds = new[] { value }; }
        }

        /// <summary>
        /// Gets or sets the start content id.
        /// </summary>
        /// <value>
        /// The start content id.
        /// </value>
        [DataMember]
        [DoNotClone]
        public int[] StartContentIds
        {
            get { return _startContentIds; }
            set { SetPropertyValueAndDetectChanges(value, ref _startContentIds, Ps.Value.StartContentIdSelector, Ps.Value.IntegerEnumerableComparer); }
        }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int IUser.StartMediaId
        {
            get
            {
                var calculatedStartNodes = this.CalculateMediaStartNodeIds(ApplicationContext.Current.Services.EntityService);
                return calculatedStartNodes.Length == 0 ? -1 : calculatedStartNodes[0];
            }
            set { StartMediaIds = new[] {value}; }
        }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>
        /// The start media id.
        /// </value>
        [DataMember]
        [DoNotClone]
        public int[] StartMediaIds
        {
            get { return _startMediaIds; }
            set { SetPropertyValueAndDetectChanges(value, ref _startMediaIds, Ps.Value.StartMediaIdSelector, Ps.Value.IntegerEnumerableComparer); }
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
        /// Gets the groups that user is part of
        /// </summary>
        [DataMember]
        public IEnumerable<IReadOnlyUserGroup> Groups
        {
            get { return _userGroups; }
        }
        
        public void RemoveGroup(string group)
        {
            foreach (var userGroup in _userGroups.ToArray())
            {
                if (userGroup.Alias == group)
                {
                    _userGroups.Remove(userGroup);
                    //reset this flag so it's rebuilt with the assigned groups
                    _allowedSections = null;
                    OnPropertyChanged(Ps.Value.UserGroupsSelector);
                }
            }
        }

        public void ClearGroups()
        {
            if (_userGroups.Count > 0)
            {
                _userGroups.Clear();
                //reset this flag so it's rebuilt with the assigned groups
                _allowedSections = null;
                OnPropertyChanged(Ps.Value.UserGroupsSelector);
            }        
        }

        public void AddGroup(IReadOnlyUserGroup group)
        {
            if (_userGroups.Add(group))
            {
                //reset this flag so it's rebuilt with the assigned groups
                _allowedSections = null;
                OnPropertyChanged(Ps.Value.UserGroupsSelector);
            }            
        }

        #endregion

        /// <summary>
        /// This is used as an internal cache for this entity - specifically for calculating start nodes so we don't re-calculated all of the time
        /// </summary>
        [IgnoreDataMember]
        [DoNotClone]
        internal IDictionary<string, object> AdditionalData
        {
            get
            {
                lock (_additionalDataLock)
                {
                    return _additionalData ?? (_additionalData = new Dictionary<string, object>());
                }
            }
        }

        [IgnoreDataMember]
        [DoNotClone]
        internal object AdditionalDataLock { get { return _additionalDataLock; } }

        public override object DeepClone()
        {
            var clone = (User)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //manually clone the start node props
            clone._startContentIds = _startContentIds.ToArray();
            clone._startMediaIds = _startMediaIds.ToArray();

            // this value has been cloned and points to the same object
            // which obviously is bad - needs to point to a new object
            clone._additionalDataLock = new object();

            if (_additionalData != null)
            {
                // clone._additionalData points to the same dictionary, which is bad, because
                // changing one clone impacts all of them - so we need to reset it with a fresh
                // dictionary that will contain the same values - and, if some values are deep
                // cloneable, they should be deep-cloned too
                var cloneAdditionalData = clone._additionalData = new Dictionary<string, object>();

                lock (_additionalDataLock)
                {
                    foreach (var kvp in _additionalData)
                    {
                        var deepCloneable = kvp.Value as IDeepCloneable;
                        cloneAdditionalData[kvp.Key] = deepCloneable == null ? kvp.Value : deepCloneable.DeepClone();
                    } 
                }
            }
                   
            //need to create new collections otherwise they'll get copied by ref
            clone._userGroups = new HashSet<IReadOnlyUserGroup>(_userGroups);
            clone._allowedSections = _allowedSections != null ? new List<string>(_allowedSections) : null;
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
        private class WrappedUserProfile : IProfile
        {
            private readonly IUser _user;

            public WrappedUserProfile(IUser user)
            {
                _user = user;
            }

            public int Id
            {
                get { return _user.Id; }
            }

            public string Name
            {
                get { return _user.Name; }
            }

            private bool Equals(WrappedUserProfile other)
            {
                return _user.Equals(other._user);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((WrappedUserProfile) obj);
            }

            public override int GetHashCode()
            {
                return _user.GetHashCode();
            }
        }

    }
}
