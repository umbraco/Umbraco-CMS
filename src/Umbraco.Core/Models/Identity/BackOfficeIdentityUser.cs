using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

namespace Umbraco.Core.Models.Identity
{
    public class BackOfficeIdentityUser : IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>, IRememberBeingDirty
    {
        /// <summary>
        /// Used to construct a new instance without an identity
        /// </summary>
        /// <returns></returns>
        internal static BackOfficeIdentityUser CreateNew(string username, string culture)
        {
            var user = new BackOfficeIdentityUser();
            user.DisableChangeTracking();
            user._userName = username;
            //we are setting minvalue here because the default is "0" which is the id of the admin user
            //which we cannot allow because the admin user will always exist
            user._id = int.MinValue;
            user._hasIdentity = false;
            user._culture = culture;
            user.EnableChangeTracking();
            return user;
        }

        public BackOfficeIdentityUser()
        {
            _startMediaIds = new int[] { };
            _startContentIds = new int[] { };
            _groups = new IReadOnlyUserGroup[] { };
            _allowedSections = new string[] { };
            _culture = Configuration.GlobalSettings.DefaultUILanguage;
        }

        private int[] _allStartContentIds;
        private int[] _allStartMediaIds;

        public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(BackOfficeUserManager<BackOfficeIdentityUser> manager)
        {
            // NOTE the authenticationType must match the umbraco one
            // defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, Constants.Security.BackOfficeAuthenticationType);
            return userIdentity;
        }

        /// <summary>
        /// Returns true if an Id has been set on this object this will be false if the object is new and not peristed to the database
        /// </summary>
        public bool HasIdentity
        {
            get { return _hasIdentity; }
        }

        public override int Id
        {
            get { return _id; }
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
            get { return _email; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _email, Ps.Value.EmailSelector); }
        }

        /// <summary>
        /// Override UserName so we can track changes to it
        /// </summary>
        public override string UserName
        {
            get { return _userName; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _userName, Ps.Value.UserNameSelector); }
        }

        /// <summary>
        /// Override LastLoginDateUtc so we can track changes to it
        /// </summary>
        public override DateTime? LastLoginDateUtc
        {
            get { return _lastLoginDateUtc; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _lastLoginDateUtc, Ps.Value.LastLoginDateUtcSelector); }            
        }

        /// <summary>
        /// Override EmailConfirmed so we can track changes to it
        /// </summary>
        public override bool EmailConfirmed
        {
            get { return _emailConfirmed; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _emailConfirmed, Ps.Value.EmailConfirmedSelector); }            
        }

        /// <summary>
        /// Gets/sets the user's real name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }            
        }

        /// <summary>
        /// Override AccessFailedCount so we can track changes to it
        /// </summary>
        public override int AccessFailedCount
        {
            get { return _accessFailedCount; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _accessFailedCount, Ps.Value.AccessFailedCountSelector); }
        }

        /// <summary>
        /// Override PasswordHash so we can track changes to it
        /// </summary>
        public override string PasswordHash
        {
            get { return _passwordHash; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _passwordHash, Ps.Value.PasswordHashSelector); }
        }


        /// <summary>
        /// Content start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartContentIds
        {
            get { return _startContentIds; }
            set
            {
                if (value == null) value = new int[0];
                _tracker.SetPropertyValueAndDetectChanges(value, ref _startContentIds, Ps.Value.StartContentIdsSelector, Ps.Value.StartIdsComparer);
            }
        }

        /// <summary>
        /// Media start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartMediaIds
        {
            get { return _startMediaIds; }
            set
            {
                if (value == null) value = new int[0];
                _tracker.SetPropertyValueAndDetectChanges(value, ref _startMediaIds, Ps.Value.StartMediaIdsSelector, Ps.Value.StartIdsComparer);
            }
        }

        /// <summary>
        /// This is a readonly list of the user's allowed sections which are based on it's user groups
        /// </summary>        
        public string[] AllowedSections
        {
            get { return _allowedSections ?? (_allowedSections = _groups.Select(x => x.Alias).ToArray()); }
        }

        public IReadOnlyUserGroup[] Groups
        {
            get { return _groups; }
            set
            {
                if (value == null) value = new IReadOnlyUserGroup[0];
                _tracker.SetPropertyValueAndDetectChanges(value, ref _groups, Ps.Value.GroupsSelector, Ps.Value.GroupsComparer);
            }
        }

        public string Culture
        {
            get { return _culture; }
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _culture, Ps.Value.CultureSelector); }
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
                var isLocked = (LockoutEndDateUtc.HasValue && LockoutEndDateUtc.Value.ToLocalTime() >= DateTime.Now);
                return isLocked;
            }
        }

        /// <summary>
        /// Overridden to make the retrieval lazy
        /// </summary>
        public override ICollection<IIdentityUserLogin> Logins
        {
            get
            {
                if (_getLogins != null && _getLogins.IsValueCreated == false)
                {
                    _logins = new ObservableCollection<IIdentityUserLogin>();
                    foreach (var l in _getLogins.Value)
                    {
                        _logins.Add(l);
                    }
                    //now assign events
                    _logins.CollectionChanged += Logins_CollectionChanged;
                }
                return _logins;
            }
        }

        public bool LoginsChanged { get; private set; }

        void Logins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LoginsChanged = true;
        }

        private List<IdentityUserRole<string>> _roles;

        //TODO: We need to override this but need to wait until the rest of the PRs are merged in
        ///// <summary>
        ///// Override Roles because the value of these are the user's group aliases
        ///// </summary>
        //public override ICollection<IdentityUserRole<string>> Roles
        //{
        //    get { return _roles ?? (_roles = Groups.Select(x => x.Alias).ToArray()); }
        //}
        /// <summary>
        /// Used to set a lazy call back to populate the user's Login list
        /// </summary>
        /// <param name="callback"></param>
        public void SetLoginsCallback(Lazy<IEnumerable<IIdentityUserLogin>> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            _getLogins = callback;
        }

        /// <summary>
        /// Returns all start node Ids assigned to the user based on both the explicit start node ids assigned to the user and any start node Ids assigned to it's user groups
        /// </summary>
        public int[] AllStartContentIds
        {
            get { return _allStartContentIds ?? (_allStartContentIds = StartContentIds.Concat(Groups.Where(x => x.StartContentId.HasValue).Select(x => x.StartContentId.Value)).Distinct().ToArray()); }
        }

        /// <summary>
        /// Returns all start node Ids assigned to the user based on both the explicit start node ids assigned to the user and any start node Ids assigned to it's user groups
        /// </summary>
        public int[] AllStartMediaIds
        {
            get { return _allStartMediaIds ?? (_allStartMediaIds = StartMediaIds.Concat(Groups.Where(x => x.StartMediaId.HasValue).Select(x => x.StartMediaId.Value)).Distinct().ToArray()); }
        }

        #region Change tracking

        public void DisableChangeTracking()
        {
            _tracker.DisableChangeTracking();
        }

        public void EnableChangeTracking()
        {
            _tracker.EnableChangeTracking();
        }

        /// <summary>
        /// Since this class only has change tracking turned on for Email/Username this will return true if either of those have changed
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return _tracker.IsDirty();
        }

        /// <summary>
        /// Returns true if the specified property is dirty
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public bool IsPropertyDirty(string propName)
        {
            return _tracker.IsPropertyDirty(propName);
        }

        /// <summary>
        /// Resets dirty properties
        /// </summary>
        void ICanBeDirty.ResetDirtyProperties()
        {
            _tracker.ResetDirtyProperties();
        }

        bool IRememberBeingDirty.WasDirty()
        {
            return _tracker.WasDirty();
        }

        bool IRememberBeingDirty.WasPropertyDirty(string propertyName)
        {
            return _tracker.WasPropertyDirty(propertyName);
        }

        void IRememberBeingDirty.ForgetPreviouslyDirtyProperties()
        {
            _tracker.ForgetPreviouslyDirtyProperties();
        }

        public void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _tracker.ResetDirtyProperties(rememberPreviouslyChangedProperties);
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();
        private class PropertySelectors
        {
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.Email);
            public readonly PropertyInfo UserNameSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.UserName);
            public readonly PropertyInfo LastLoginDateUtcSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, DateTime?>(x => x.LastLoginDateUtc);
            public readonly PropertyInfo EmailConfirmedSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, bool>(x => x.EmailConfirmed);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.Name);
            public readonly PropertyInfo AccessFailedCountSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, int>(x => x.AccessFailedCount);
            public readonly PropertyInfo PasswordHashSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.PasswordHash);
            public readonly PropertyInfo CultureSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.Culture);
            public readonly PropertyInfo StartMediaIdsSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, int[]>(x => x.StartMediaIds);
            public readonly PropertyInfo StartContentIdsSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, int[]>(x => x.StartContentIds);
            public readonly PropertyInfo GroupsSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, IReadOnlyUserGroup[]>(x => x.Groups);

            //Custom comparer for enumerables
            public readonly DelegateEqualityComparer<IReadOnlyUserGroup[]> GroupsComparer = new DelegateEqualityComparer<IReadOnlyUserGroup[]>(
                (groups, enumerable) => groups.Select(x => x.Alias).UnsortedSequenceEqual(enumerable.Select(x => x.Alias)),
                groups => groups.GetHashCode());
            public readonly DelegateEqualityComparer<int[]> StartIdsComparer = new DelegateEqualityComparer<int[]>(
                (groups, enumerable) => groups.UnsortedSequenceEqual(enumerable),
                groups => groups.GetHashCode());
        }

        private readonly ChangeTracker _tracker = new ChangeTracker();
        private string _email;
        private string _userName;
        private int _id;
        private bool _hasIdentity = false;
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

        /// <summary>
        /// internal class used to track changes for properties that have it enabled
        /// </summary>        
        private class ChangeTracker : TracksChangesEntityBase
        {
        } 
        #endregion


    }
}