using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Membership
{
    /// <summary>
    /// Represents a backoffice user
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class User : EntityBase, IUser, IProfile
    {
        /// <summary>
        /// Constructor for creating a new/empty user
        /// </summary>
        public User(GlobalSettings globalSettings)
        {
            SessionTimeout = 60;
            _userGroups = new HashSet<IReadOnlyUserGroup>();
            _language = globalSettings.DefaultUILanguage;
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
        public User(GlobalSettings globalSettings, string name, string email, string username, string rawPasswordValue)
            : this(globalSettings)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrEmpty(rawPasswordValue)) throw new ArgumentException("Value cannot be null or empty.", nameof(rawPasswordValue));

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
        /// <param name="passwordConfig"></param>
        /// <param name="userGroups"></param>
        /// <param name="startContentIds"></param>
        /// <param name="startMediaIds"></param>
        public User(GlobalSettings globalSettings, int id, string name, string email, string username,
            string rawPasswordValue, string passwordConfig,
            IEnumerable<IReadOnlyUserGroup> userGroups, int[] startContentIds, int[] startMediaIds)
            : this(globalSettings)
        {
            //we allow whitespace for this value so just check null
            if (rawPasswordValue == null) throw new ArgumentNullException(nameof(rawPasswordValue));
            if (userGroups == null) throw new ArgumentNullException(nameof(userGroups));
            if (startContentIds == null) throw new ArgumentNullException(nameof(startContentIds));
            if (startMediaIds == null) throw new ArgumentNullException(nameof(startMediaIds));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));

            Id = id;
            _name = name;
            _email = email;
            _username = username;
            _rawPasswordValue = rawPasswordValue;
            _passwordConfig = passwordConfig;
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
        private string _passwordConfig;
        private IEnumerable<string> _allowedSections;
        private HashSet<IReadOnlyUserGroup> _userGroups;
        private bool _isApproved;
        private bool _isLockedOut;
        private string _language;
        private DateTime _lastPasswordChangedDate;
        private DateTime _lastLoginDate;
        private DateTime _lastLockoutDate;
        private IDictionary<string, object> _additionalData;
        private object _additionalDataLock = new object();

        //Custom comparer for enumerable
        private static readonly DelegateEqualityComparer<IEnumerable<int>> IntegerEnumerableComparer =
            new DelegateEqualityComparer<IEnumerable<int>>(
                (enum1, enum2) => enum1.UnsortedSequenceEqual(enum2),
                enum1 => enum1.GetHashCode());


        [DataMember]
        public DateTime? EmailConfirmedDate
        {
            get => _emailConfirmedDate;
            set => SetPropertyValueAndDetectChanges(value, ref _emailConfirmedDate, nameof(EmailConfirmedDate));
        }

        [DataMember]
        public DateTime? InvitedDate
        {
            get => _invitedDate;
            set => SetPropertyValueAndDetectChanges(value, ref _invitedDate, nameof(InvitedDate));
        }

        [DataMember]
        public string Username
        {
            get => _username;
            set => SetPropertyValueAndDetectChanges(value, ref _username, nameof(Username));
        }

        [DataMember]
        public string Email
        {
            get => _email;
            set => SetPropertyValueAndDetectChanges(value, ref _email, nameof(Email));
        }

        [IgnoreDataMember]
        public string RawPasswordValue
        {
            get => _rawPasswordValue;
            set => SetPropertyValueAndDetectChanges(value, ref _rawPasswordValue, nameof(RawPasswordValue));
        }

        [IgnoreDataMember]
        public string PasswordConfiguration
        {
            get => _passwordConfig;
            set => SetPropertyValueAndDetectChanges(value, ref _passwordConfig, nameof(PasswordConfiguration));
        }

        [DataMember]
        public bool IsApproved
        {
            get => _isApproved;
            set => SetPropertyValueAndDetectChanges(value, ref _isApproved, nameof(IsApproved));
        }

        [IgnoreDataMember]
        public bool IsLockedOut
        {
            get => _isLockedOut;
            set => SetPropertyValueAndDetectChanges(value, ref _isLockedOut, nameof(IsLockedOut));
        }

        [IgnoreDataMember]
        public DateTime LastLoginDate
        {
            get => _lastLoginDate;
            set => SetPropertyValueAndDetectChanges(value, ref _lastLoginDate, nameof(LastLoginDate));
        }

        [IgnoreDataMember]
        public DateTime LastPasswordChangeDate
        {
            get => _lastPasswordChangedDate;
            set => SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangedDate, nameof(LastPasswordChangeDate));
        }

        [IgnoreDataMember]
        public DateTime LastLockoutDate
        {
            get => _lastLockoutDate;
            set => SetPropertyValueAndDetectChanges(value, ref _lastLockoutDate, nameof(LastLockoutDate));
        }

        [IgnoreDataMember]
        public int FailedPasswordAttempts
        {
            get => _failedLoginAttempts;
            set => SetPropertyValueAndDetectChanges(value, ref _failedLoginAttempts, nameof(FailedPasswordAttempts));
        }

        [IgnoreDataMember]
        public string Comments { get; set; }

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
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        public IEnumerable<string> AllowedSections
        {
            get { return _allowedSections ?? (_allowedSections = new List<string>(_userGroups.SelectMany(x => x.AllowedSections).Distinct())); }
        }

        public IProfile ProfileData => new WrappedUserProfile(this);

        /// <summary>
        /// The security stamp used by ASP.Net identity
        /// </summary>
        [IgnoreDataMember]
        public string SecurityStamp
        {
            get => _securityStamp;
            set => SetPropertyValueAndDetectChanges(value, ref _securityStamp, nameof(SecurityStamp));
        }

        [DataMember]
        public string Avatar
        {
            get => _avatar;
            set => SetPropertyValueAndDetectChanges(value, ref _avatar, nameof(Avatar));
        }

        /// <summary>
        /// A Json blob stored for recording tour data for a user
        /// </summary>
        [DataMember]
        public string TourData
        {
            get => _tourData;
            set => SetPropertyValueAndDetectChanges(value, ref _tourData, nameof(TourData));
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
            get => _sessionTimeout;
            set => SetPropertyValueAndDetectChanges(value, ref _sessionTimeout, nameof(SessionTimeout));
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
            get => _startContentIds;
            set => SetPropertyValueAndDetectChanges(value, ref _startContentIds, nameof(StartContentIds), IntegerEnumerableComparer);
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
            get => _startMediaIds;
            set => SetPropertyValueAndDetectChanges(value, ref _startMediaIds, nameof(StartMediaIds), IntegerEnumerableComparer);
        }

        [DataMember]
        public string Language
        {
            get => _language;
            set => SetPropertyValueAndDetectChanges(value, ref _language, nameof(Language));
        }

        /// <summary>
        /// Gets the groups that user is part of
        /// </summary>
        [DataMember]
        public IEnumerable<IReadOnlyUserGroup> Groups => _userGroups;

        public void RemoveGroup(string group)
        {
            foreach (var userGroup in _userGroups.ToArray())
            {
                if (userGroup.Alias == group)
                {
                    _userGroups.Remove(userGroup);
                    //reset this flag so it's rebuilt with the assigned groups
                    _allowedSections = null;
                    OnPropertyChanged(nameof(Groups));
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
                OnPropertyChanged(nameof(Groups));
            }
        }

        public void AddGroup(IReadOnlyUserGroup group)
        {
            if (_userGroups.Add(group))
            {
                //reset this flag so it's rebuilt with the assigned groups
                _allowedSections = null;
                OnPropertyChanged(nameof(Groups));
            }
        }

        public T FromUserCache<T>(string cacheKey)
            where T : class
        {
            lock (_additionalDataLock)
            {
                return AdditionalData.TryGetValue(cacheKey, out var data)
                    ? data as T
                    : null;
            }
        }

        public void ToUserCache<T>(string cacheKey, T vals)
            where T : class
        {
            lock (_additionalDataLock)
            {
                AdditionalData[cacheKey] = vals;
            }
        }

        [IgnoreDataMember]
        [DoNotClone]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This should not be used, it's currently used for only a single edge case - should probably be removed for netcore")]
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

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (User)clone;

            //manually clone the start node props
            clonedEntity._startContentIds = _startContentIds.ToArray();
            clonedEntity._startMediaIds = _startMediaIds.ToArray();

            // this value has been cloned and points to the same object
            // which obviously is bad - needs to point to a new object
            clonedEntity._additionalDataLock = new object();

            if (_additionalData != null)
            {
                // clone._additionalData points to the same dictionary, which is bad, because
                // changing one clone impacts all of them - so we need to reset it with a fresh
                // dictionary that will contain the same values - and, if some values are deep
                // cloneable, they should be deep-cloned too
                var cloneAdditionalData = clonedEntity._additionalData = new Dictionary<string, object>();

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
            clonedEntity._userGroups = new HashSet<IReadOnlyUserGroup>(_userGroups);
            clonedEntity._allowedSections = _allowedSections != null ? new List<string>(_allowedSections) : null;

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

            public int Id => _user.Id;

            public string Name => _user.Name;

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
