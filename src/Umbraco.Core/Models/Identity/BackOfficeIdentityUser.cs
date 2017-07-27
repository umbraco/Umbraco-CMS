using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

namespace Umbraco.Core.Models.Identity
{
    public class BackOfficeIdentityUser : IdentityUser<int, IIdentityUserLogin, IdentityUserRole<string>, IdentityUserClaim<int>>
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
            get
            {
                if (_allStartContentIds != null) return _allStartContentIds;

                var gsn = Groups.Where(x => x.StartContentId.HasValue).Select(x => x.StartContentId.Value).Distinct().ToArray();
                var usn = StartContentIds;
                return _allStartContentIds = UserExtensions.CombineStartNodes(UmbracoObjectTypes.Document, gsn, usn, ApplicationContext.Current.Services.EntityService);
            }
        }

        /// <summary>
        /// Returns all start node Ids assigned to the user based on both the explicit start node ids assigned to the user and any start node Ids assigned to it's user groups
        /// </summary>
        public int[] AllStartMediaIds
        {
            get
            {
                if (_allStartMediaIds != null) return _allStartMediaIds;

                var gsn = Groups.Where(x => x.StartMediaId.HasValue).Select(x => x.StartMediaId.Value).Distinct().ToArray();
                var usn = StartMediaIds;
                return _allStartMediaIds = UserExtensions.CombineStartNodes(UmbracoObjectTypes.Media, gsn, usn, ApplicationContext.Current.Services.EntityService);
            }
        }
    }
}