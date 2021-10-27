using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Umbraco.Core.Collections;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models.Identity
{
    public class BackOfficeIdentityUser : IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>, IRememberBeingDirty
    {
        private string _email;
        private string _userName;
        private int _id;
        private bool _hasIdentity;
        private DateTime? _lastLoginDateUtc;
        private bool _emailConfirmed;
        private string _name;
        private int _accessFailedCount;
        private string _passwordHash;
        private string _culture;
        private ObservableCollection<IIdentityUserLogin> _logins;
        private Lazy<IEnumerable<IIdentityUserLogin>> _getLogins;
        private IReadOnlyUserGroup[] _groups;
        private string[] _allowedSections;
        private int[] _startMediaIds;
        private int[] _startContentIds;
        private DateTime? _lastPasswordChangeDateUtc;

        /// <summary>
        ///  Used to construct a new instance without an identity
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email">This is allowed to be null (but would need to be filled in if trying to persist this instance)</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static BackOfficeIdentityUser CreateNew(string username, string email, string culture)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));

            var user = new BackOfficeIdentityUser(Array.Empty<IReadOnlyUserGroup>());
            user.DisableChangeTracking();
            user._userName = username;
            user._email = email;
            //we are setting minvalue here because the default is "0" which is the id of the admin user
            //which we cannot allow because the admin user will always exist
            user._id = int.MinValue;
            user._hasIdentity = false;
            user._culture = culture;
            user.EnableChangeTracking();
            return user;
        }

        private BackOfficeIdentityUser(IReadOnlyUserGroup[] groups)
        {
            _startMediaIds = Array.Empty<int>();
            _startContentIds = Array.Empty<int>();
            _allowedSections = Array.Empty<string>();
            _culture = Current.Configs.Global().DefaultUILanguage; // TODO: inject

            // must initialize before setting groups
            _roles = new EventClearingObservableCollection<IdentityUserRole<string>>();
            _roles.CollectionChanged += _roles_CollectionChanged;

            // use the property setters - they do more than just setting a field
            Groups = groups;
        }

        /// <summary>
        /// Creates an existing user with the specified groups
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groups"></param>
        public BackOfficeIdentityUser(int userId, IEnumerable<IReadOnlyUserGroup> groups)
            : this(groups.ToArray())
        {
            // use the property setters - they do more than just setting a field
            Id = userId;
        }

        /// <summary>
        /// Returns true if an Id has been set on this object this will be false if the object is new and not persisted to the database
        /// </summary>
        public bool HasIdentity => _hasIdentity;

        public int[] CalculatedMediaStartNodeIds { get; internal set; }
        public int[] CalculatedContentStartNodeIds { get; internal set; }

        public override int Id
        {
            get => _id;
            set
            {
                _id = value;
                _hasIdentity = true;
            }
        }

        /// <summary>
        /// Override Email so we can track changes to it
        /// </summary>
        public override string Email
        {
            get => _email;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _email, nameof(Email));
        }

        /// <summary>
        /// Override UserName so we can track changes to it
        /// </summary>
        public override string UserName
        {
            get => _userName;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _userName, nameof(UserName));
        }

        /// <summary>
        /// LastPasswordChangeDateUtc so we can track changes to it
        /// </summary>
        public override DateTime? LastPasswordChangeDateUtc
        {
            get { return _lastPasswordChangeDateUtc; }
            set { _beingDirty.SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangeDateUtc, nameof(LastPasswordChangeDateUtc)); }
        }

        /// <summary>
        /// Override LastLoginDateUtc so we can track changes to it
        /// </summary>
        public override DateTime? LastLoginDateUtc
        {
            get => _lastLoginDateUtc;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _lastLoginDateUtc, nameof(LastLoginDateUtc));
        }

        /// <summary>
        /// Override EmailConfirmed so we can track changes to it
        /// </summary>
        public override bool EmailConfirmed
        {
            get => _emailConfirmed;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _emailConfirmed, nameof(EmailConfirmed));
        }

        /// <summary>
        /// Gets/sets the user's real name
        /// </summary>
        public string Name
        {
            get => _name;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Override AccessFailedCount so we can track changes to it
        /// </summary>
        public override int AccessFailedCount
        {
            get => _accessFailedCount;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _accessFailedCount, nameof(AccessFailedCount));
        }

        /// <summary>
        /// Override PasswordHash so we can track changes to it
        /// </summary>
        public override string PasswordHash
        {
            get => _passwordHash;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _passwordHash, nameof(PasswordHash));
        }


        /// <summary>
        /// Content start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartContentIds
        {
            get => _startContentIds;
            set
            {
                if (value == null) value = new int[0];
                _beingDirty.SetPropertyValueAndDetectChanges(value, ref _startContentIds, nameof(StartContentIds), StartIdsComparer);
            }
        }

        /// <summary>
        /// Media start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartMediaIds
        {
            get => _startMediaIds;
            set
            {
                if (value == null) value = new int[0];
                _beingDirty.SetPropertyValueAndDetectChanges(value, ref _startMediaIds, nameof(StartMediaIds), StartIdsComparer);
            }
        }

        /// <summary>
        /// This is a readonly list of the user's allowed sections which are based on it's user groups
        /// </summary>
        public string[] AllowedSections
        {
            get { return _allowedSections ?? (_allowedSections = _groups.SelectMany(x => x.AllowedSections).Distinct().ToArray()); }
        }

        public string Culture
        {
            get => _culture;
            set => _beingDirty.SetPropertyValueAndDetectChanges(value, ref _culture, nameof(Culture));
        }

        public IReadOnlyUserGroup[] Groups
        {
            get => _groups;
            set
            {
                //so they recalculate
                _allowedSections = null;

                _groups = value;

                //now clear all roles and re-add them
                _roles.ClearCollectionChangedEvents();
                _roles.Clear();
                foreach (var identityUserRole in _groups.Select(x => new IdentityUserRole<string>
                {
                    RoleId = x.Alias,
                    UserId = Id.ToString()
                }))
                {
                    _roles.Add(identityUserRole);
                }
                _roles.CollectionChanged += _roles_CollectionChanged;

                _beingDirty.SetPropertyValueAndDetectChanges(value, ref _groups, nameof(Groups), GroupsComparer);
            }
        }

        /// <summary>
        /// Lockout is always enabled
        /// </summary>
        public override bool LockoutEnabled
        {
            get { return true; }
            set
            {
                //do nothing
            }
        }

        /// <summary>
        /// Based on the user's lockout end date, this will determine if they are locked out
        /// </summary>
        internal bool IsLockedOut
        {
            get
            {
                var isLocked = LockoutEndDateUtc.HasValue && LockoutEndDateUtc.Value.ToLocalTime() > DateTime.Now;
                return isLocked;
            }
        }

        /// <summary>
        /// This is a 1:1 mapping with IUser.IsApproved
        /// </summary>
        internal bool IsApproved { get; set; }

        /// <summary>
        /// Overridden to make the retrieval lazy
        /// </summary>
        public override ICollection<IIdentityUserLogin> Logins
        {
            get
            {
                // return if it exists
                if (_logins != null) return _logins;

                _logins = new ObservableCollection<IIdentityUserLogin>();

                // if the callback is there and hasn't been created yet then execute it and populate the logins
                if (_getLogins != null && !_getLogins.IsValueCreated)
                {
                    foreach (var l in _getLogins.Value)
                    {
                        _logins.Add(l);
                    }
                }

                //now assign events
                _logins.CollectionChanged += Logins_CollectionChanged;

                return _logins;
            }
        }

        void Logins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _beingDirty.OnPropertyChanged(nameof(Logins));
        }

        private void _roles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _beingDirty.OnPropertyChanged(nameof(Roles));
        }

        private readonly EventClearingObservableCollection<IdentityUserRole<string>> _roles;

        /// <summary>
        /// helper method to easily add a role without having to deal with IdentityUserRole{T}
        /// </summary>
        /// <param name="role"></param>
        /// <remarks>
        /// Adding a role this way will not reflect on the user's group's collection or it's allowed sections until the user is persisted
        /// </remarks>
        public void AddRole(string role)
        {
            Roles.Add(new IdentityUserRole<string>
            {
                UserId = Id.ToString(),
                RoleId = role
            });
        }

        /// <summary>
        /// Override Roles because the value of these are the user's group aliases
        /// </summary>
        public override ICollection<IdentityUserRole<string>> Roles => _roles;

        /// <summary>
        /// Used to set a lazy call back to populate the user's Login list
        /// </summary>
        /// <param name="callback"></param>
        public void SetLoginsCallback(Lazy<IEnumerable<IIdentityUserLogin>> callback)
        {
            _getLogins = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        #region BeingDirty

        private readonly BeingDirty _beingDirty = new BeingDirty();

        /// <inheritdoc />
        public bool IsDirty()
        {
            return _beingDirty.IsDirty();
        }

        /// <inheritdoc />
        public bool IsPropertyDirty(string propName)
        {
            return _beingDirty.IsPropertyDirty(propName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDirtyProperties()
        {
            return _beingDirty.GetDirtyProperties();
        }

        /// <inheritdoc />
        public void ResetDirtyProperties()
        {
            _beingDirty.ResetDirtyProperties();
        }

        /// <inheritdoc />
        public bool WasDirty()
        {
            return _beingDirty.WasDirty();
        }

        /// <inheritdoc />
        public bool WasPropertyDirty(string propertyName)
        {
            return _beingDirty.WasPropertyDirty(propertyName);
        }

        /// <inheritdoc />
        public void ResetWereDirtyProperties()
        {
            _beingDirty.ResetWereDirtyProperties();
        }

        /// <inheritdoc />
        public void ResetDirtyProperties(bool rememberDirty)
        {
            _beingDirty.ResetDirtyProperties(rememberDirty);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetWereDirtyProperties()
            => _beingDirty.GetWereDirtyProperties();

        /// <summary>
        /// Disables change tracking.
        /// </summary>
        public void DisableChangeTracking()
        {
            _beingDirty.DisableChangeTracking();
        }

        /// <summary>
        /// Enables change tracking.
        /// </summary>
        public void EnableChangeTracking()
        {
            _beingDirty.EnableChangeTracking();
        }

        #endregion

        //Custom comparer for enumerables
        private static readonly DelegateEqualityComparer<IReadOnlyUserGroup[]> GroupsComparer = new DelegateEqualityComparer<IReadOnlyUserGroup[]>(
            (groups, enumerable) => groups.Select(x => x.Alias).UnsortedSequenceEqual(enumerable.Select(x => x.Alias)),
            groups => groups.GetHashCode());

        private static readonly DelegateEqualityComparer<int[]> StartIdsComparer = new DelegateEqualityComparer<int[]>(
            (groups, enumerable) => groups.UnsortedSequenceEqual(enumerable),
            groups => groups.GetHashCode());
    }
}
