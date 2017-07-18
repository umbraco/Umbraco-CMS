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

        public BackOfficeIdentityUser()
        {
            StartMediaIds = new int[] { };
            StartContentIds = new int[] { };
            Groups = new IReadOnlyUserGroup[] { };
            AllowedSections = new string[] { };
            Culture = Configuration.GlobalSettings.DefaultUILanguage;
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
            set { _tracker.SetPropertyValueAndDetectChanges(value, ref _userName, Ps.Value.UsernameSelector); }
        }

        /// <summary>
        /// Gets/sets the user's real name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartContentIds { get; set; }

        /// <summary>
        /// Media start nodes assigned to the User (not ones assigned to the user's groups)
        /// </summary>
        public int[] StartMediaIds { get; set; }
        public string[] AllowedSections { get; set; }
        public IReadOnlyUserGroup[] Groups { get; set; }
        public string Culture { get; set; }

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

        private ObservableCollection<IIdentityUserLogin> _logins;
        private Lazy<IEnumerable<IIdentityUserLogin>> _getLogins;

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
        /// <summary>
        /// Since this class only has change tracking turned on for Email/Username this will return true if either of those have changed
        /// </summary>
        /// <returns></returns>
        bool ICanBeDirty.IsDirty()
        {
            return _tracker.IsDirty();
        }

        /// <summary>
        /// Returns true if the specified property is dirty
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        bool ICanBeDirty.IsPropertyDirty(string propName)
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

        void IRememberBeingDirty.ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _tracker.ResetDirtyProperties(rememberPreviouslyChangedProperties);
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();
        private class PropertySelectors
        {
            public readonly PropertyInfo EmailSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.Email);
            public readonly PropertyInfo UsernameSelector = ExpressionHelper.GetPropertyInfo<BackOfficeIdentityUser, string>(x => x.UserName);
        }

        private readonly ChangeTracker _tracker = new ChangeTracker();
        private string _email;
        private string _userName;

        /// <summary>
        /// internal class used to track changes for properties that have it enabled
        /// </summary>        
        private class ChangeTracker : TracksChangesEntityBase
        {
        } 
        #endregion


    }
}