using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Security;

namespace Umbraco.Core.Models.Identity
{
    public class BackOfficeIdentityUser : IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>
    {

        public BackOfficeIdentityUser()
        {
            StartMediaId = -1;
            StartContentId = -1;
            Culture = Configuration.GlobalSettings.DefaultUILanguage;
        }

        public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(BackOfficeUserManager manager)
        {
            // NOTE the authenticationType must match the umbraco one
            // defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, Constants.Security.BackOfficeAuthenticationType);
            return userIdentity;
        }

        /// <summary>
        /// Gets/sets the user's real name
        /// </summary>
        public string Name { get; set; }
        public int StartContentId { get; set; }
        public int StartMediaId { get; set; }
        public string[] AllowedSections { get; set; }
        public string Culture { get; set; }

        public string UserTypeAlias { get; set; }

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
    }
}