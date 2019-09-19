using System;
using System.Collections;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using umbraco.DataLayer;
using Umbraco.Core.Persistence.Querying;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// represents a Umbraco back end user
    /// </summary>
    [Obsolete("Use the UserService instead")]
    public class User
    {
        internal IUser UserEntity;
        private int? _lazyId;
        private bool? _defaultToLiveEditing;
        
        private readonly Hashtable _notifications = new Hashtable();
        private bool _notificationsInitialized = false;
        
        internal User(IUser user)
        {
            UserEntity = user;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        public User(int ID)
        {
            SetupUser(ID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="noSetup">if set to <c>true</c> [no setup].</param>
        public User(int ID, bool noSetup)
        {
            _lazyId = ID;            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        /// <param name="Password">The password.</param>
        public User(string Login, string Password)
        {
            SetupUser(getUserId(Login, Password));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        public User(string Login)
        {
            SetupUser(getUserId(Login));
        }
        
        private void SetupUser(int ID)
        {
            UserEntity = ApplicationContext.Current.Services.UserService.GetUserById(ID);
            if (UserEntity == null)
            {
                throw new ArgumentException("No User exists with ID " + ID);
            }
        }

        /// <summary>
        /// Used to persist object changes to the database.
        /// </summary>
        public void Save()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            ApplicationContext.Current.Services.UserService.Save(UserEntity);

            OnSaving(EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the users name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.Name;
            }
            set
            {
                UserEntity.Name = value;
                
            }
        }

        /// <summary>
        /// Gets or sets the users email.
        /// </summary>
        /// <value>The email.</value>
        public string Email
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.Email;
            }
            set
            {
                UserEntity.Email = value;
            }
        }

        /// <summary>
        /// Gets or sets the users language.
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.Language;
            }
            set
            {
                UserEntity.Language = value;
            }
        }

        /// <summary>
        /// Gets or sets the users password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get
            {
                return GetPassword();
            }
            set
            {
                UserEntity.RawPasswordValue = value;
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            return UserEntity.RawPasswordValue;
        }

        /// <summary>
        /// Determines whether this user is an admin.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this user is admin; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("Use Umbraco.Core.Models.IsAdmin extension method instead", false)]
        public bool IsAdmin()
        {
            return UserEntity.IsAdmin();
        }

        [Obsolete("Do not use this method to validate credentials, use the user's membership provider to do authentication. This method will not work if the password format is 'Encrypted'")]
        public bool ValidatePassword(string password)
        {
            var userLogin = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<string>(
                "SELECT userLogin FROM umbracoUser WHERE userLogin = @login AND UserPasword = @password",
                new {login = LoginName, password = password});

            return userLogin == this.LoginName;
        }

        /// <summary>
        /// Determines whether this user is the root (super user).
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this user is root; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRoot()
        {
            return Id == 0;
        }

        /// <summary>
        /// Gets the applications which the user has access to.
        /// </summary>
        /// <value>The users applications.</value>
        public Application[] Applications
        {
            get
            {
                return GetApplications().ToArray();
            }
        }

        /// <summary>
        /// Get the application which the user has access to as a List
        /// </summary>
        /// <returns></returns>
        public List<Application> GetApplications()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            var allApps = Application.getAll();
            var apps = new List<Application>();

            var sections = UserEntity.AllowedSections;

            foreach (var s in sections)
            {
                var app = allApps.SingleOrDefault(x => x.alias == s);
                if (app != null)
                    apps.Add(app);
            }

            return apps;
        }

        /// <summary>
        /// Gets or sets the users  login name
        /// </summary>
        /// <value>The loginname.</value>
        public string LoginName
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.Username;
            }
            set
            {
                if (EnsureUniqueLoginName(value, this) == false)
                    throw new Exception(String.Format("A user with the login '{0}' already exists", value));

                UserEntity.Username = value;
            }
        }

        /// <summary>
        /// Gets the security stamp
        /// </summary>
        /// <value>The loginname.</value>
        internal string SecurityStamp
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.SecurityStamp;
            }            
        }

        private static bool EnsureUniqueLoginName(string loginName, User currentUser)
        {
            User[] u = User.getAllByLoginName(loginName);
            if (u.Length != 0)
            {
                if (u[0].Id != currentUser.Id)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the users credentials.
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <returns></returns>
        [Obsolete("Do not use this method to validate credentials, use the user's membership provider to do authentication. This method will not work if the password format is 'Encrypted'")]
        public static bool validateCredentials(string lname, string passw)
        {
            return validateCredentials(lname, passw, true);
        }

        /// <summary>
        /// Validates the users credentials.
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <param name="checkForUmbracoConsoleAccess">if set to <c>true</c> [check for umbraco console access].</param>
        /// <returns></returns>
        [Obsolete("Do not use this method to validate credentials, use the user's membership provider to do authentication. This method will not work if the password format is 'Encrypted'")]
        public static bool validateCredentials(string lname, string passw, bool checkForUmbracoConsoleAccess)
        {
            string consoleCheckSql = "";
            if (checkForUmbracoConsoleAccess)
                consoleCheckSql = "and userNoConsole = 0 ";

            using (var sqlHelper = Application.SqlHelper)
            {
                object tmp = sqlHelper.ExecuteScalar<object>(
                    "select id from umbracoUser where userDisabled = 0 " + consoleCheckSql + " and userLogin = @login and userPassword = @pw",
                    sqlHelper.CreateParameter("@login", lname),
                    sqlHelper.CreateParameter("@pw", passw)
                );

                // Logging
                if (tmp == null)
                {
                    LogHelper.Info<User>("Login: '" + lname + "' failed, from IP: " + System.Web.HttpContext.Current.Request.UserHostAddress);
                }

                return (tmp != null);
            }
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        public static User[] getAll()
        {
            int totalRecs;
            var users = ApplicationContext.Current.Services.UserService.GetAll(
                0, int.MaxValue, out totalRecs);

            return users.Select(x => new User(x))
                .OrderBy(x => x.Name)
                .ToArray();
        }


        /// <summary>
        /// Gets the current user (logged in)
        /// </summary>
        /// <returns>A user or null</returns>
        public static User GetCurrent()
        {
            try
            {
                if (BasePages.BasePage.umbracoUserContextID != "")
                    return GetUser(BasePages.BasePage.GetUserId(BasePages.BasePage.umbracoUserContextID));
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

		/// <summary>
        /// Gets all users by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
		public static User[] getAllByEmail(string email)
		{
			return getAllByEmail(email, false);
		}

        /// <summary>
        /// Gets all users by email.
        /// </summary>
        /// <param name="email">The email.</param>
       /// <param name="useExactMatch">match exact email address or partial email address.</param>
        /// <returns></returns>
        public static User[] getAllByEmail(string email, bool useExactMatch)
        {
            int totalRecs;
            if (useExactMatch)
            {
                return ApplicationContext.Current.Services.UserService.FindByEmail(
                    email, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Exact)
                    .Select(x => new User(x))
                    .ToArray();
            }
            else
            {
                return ApplicationContext.Current.Services.UserService.FindByEmail(
                    string.Format("%{0}%", email), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Wildcard)
                    .Select(x => new User(x))
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets all users by login name.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public static User[] getAllByLoginName(string login)
        {
            return GetAllByLoginName(login, false).ToArray();
        }

		/// <summary>
		/// Gets all users by login name.
		/// </summary>
		/// <param name="login">The login.</param>
        /// <param name="partialMatch">whether to use a partial match</param>
		/// <returns></returns>
		public static User[] getAllByLoginName(string login, bool partialMatch)
		{
			return GetAllByLoginName(login, partialMatch).ToArray();
		}

        public static IEnumerable<User> GetAllByLoginName(string login, bool partialMatch)
        {
            int totalRecs;
            if (partialMatch)
            {
                return ApplicationContext.Current.Services.UserService.FindByUsername(
                    string.Format("%{0}%", login), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Wildcard)
                    .Select(x => new User(x))
                    .ToArray();
            }
            else
            {
                return ApplicationContext.Current.Services.UserService.FindByUsername(
                    login, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Exact)
                    .Select(x => new User(x))
                    .ToArray();
            }
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="name">The full name.</param>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        public static User MakeNew(string name, string lname, string passw)
        {
            var user = new Umbraco.Core.Models.Membership.User(name, "", lname, passw);
            ApplicationContext.Current.Services.UserService.Save(user);

            var u = new User(user);
            u.OnNew(EventArgs.Empty);

            return u;
        }


        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="lname">The lname.</param>
        /// <param name="passw">The passw.</param>
        /// <param name="email">The email.</param>
        public static User MakeNew(string name, string lname, string passw, string email)
        {
            var user = new Umbraco.Core.Models.Membership.User(name, email, lname, passw);
            ApplicationContext.Current.Services.UserService.Save(user);

            var u = new User(user);
            u.OnNew(EventArgs.Empty);

            return u;
        }


        /// <summary>
        /// Updates the name, login name and password for the user with the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="lname">The lname.</param>
        /// <param name="email">The email.</param>
        public static void Update(int id, string name, string lname, string email)
        {
            if (EnsureUniqueLoginName(lname, GetUser(id)) == false)
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));

            var found = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (found == null) return;
            found.Name = name;
            found.Username = lname;
            found.Email = email;
            ApplicationContext.Current.Services.UserService.Save(found);
        }

        public static void Update(int id, string name, string lname, string email, bool disabled, bool noConsole)
        {
            if (EnsureUniqueLoginName(lname, GetUser(id)) == false)
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));

            var found = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (found == null) return;
            found.Name = name;
            found.Username = lname;
            found.Email = email;
            found.IsApproved = disabled == false;
            found.IsLockedOut = noConsole;
            ApplicationContext.Current.Services.UserService.Save(found);
        }

        /// <summary>
        /// Updates the membership provider properties
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="email"></param>
        /// <param name="disabled"></param>
        /// <param name="noConsole"></param>        
        public static void Update(int id, string email, bool disabled, bool noConsole)
        {
            var found = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (found == null) return;
            
            found.Email = email;            
            found.IsApproved = disabled == false;
            found.IsLockedOut = noConsole;
            ApplicationContext.Current.Services.UserService.Save(found);
        }
        
        /// <summary>
        /// Gets the ID from the user with the specified login name and password
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname, string passw)
        {
            var found = ApplicationContext.Current.Services.UserService.GetByUsername(lname);
            return found.RawPasswordValue == passw ? found.Id : -1;
        }

        /// <summary>
        /// Gets the ID from the user with the specified login name
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname)
        {
            var found = ApplicationContext.Current.Services.UserService.GetByUsername(lname);
            return found == null ? -1 : found.Id;
        }
        
        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [Obsolete("Deleting users are NOT supported as history needs to be kept. Please use the disable() method instead")]
        public void delete()
        {
            //make sure you cannot delete the admin user!
            if (this.Id == 0)
                throw new InvalidOperationException("The Administrator account cannot be deleted");

            OnDeleting(EventArgs.Empty);

            ApplicationContext.Current.Services.UserService.Delete(UserEntity, true);

            FlushFromCache();
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        public void disable()
        {
            OnDisabling(EventArgs.Empty);

            //delete without the true overload will perform the disable operation
            ApplicationContext.Current.Services.UserService.Delete(UserEntity);
        }

        /// <summary>
        /// Gets the users permissions based on a nodes path
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string GetPermissions(string path)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            var userService = ApplicationContext.Current.Services.UserService;
            return string.Join("",
                userService.GetPermissionsForPath(UserEntity, path).GetAllPermissions());
        }

        /// <summary>
        /// Initializes the user node permissions
        /// </summary>
        [Obsolete("This method doesn't do anything whatsoever and will be removed in future versions")]
        public void initCruds()
        {            
        }

        /// <summary>
        /// Gets a users notifications for a specified node path.
        /// </summary>
        /// <param name="Path">The node path.</param>
        /// <returns></returns>
        public string GetNotifications(string Path)
        {
            string notifications = "";

            if (_notificationsInitialized == false)
                initNotifications();

            foreach (string nodeId in Path.Split(','))
            {
                if (_notifications.ContainsKey(int.Parse(nodeId)))
                    notifications = _notifications[int.Parse(nodeId)].ToString();
            }

            return notifications;
        }

        /// <summary>
        /// Clears the internal hashtable containing cached information about notifications for the user
        /// </summary>
        public void resetNotificationCache()
        {
            _notificationsInitialized = false;
            _notifications.Clear();
        }

        /// <summary>
        /// Initializes the notifications and caches them.
        /// </summary>
        public void initNotifications()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            var notifications = ApplicationContext.Current.Services.NotificationService.GetUserNotifications(UserEntity);
            foreach (var n in notifications.OrderBy(x => x.EntityId))
            {
                int nodeId = n.EntityId;
                if (_notifications.ContainsKey(nodeId) == false)
                {
                    _notifications.Add(nodeId, string.Empty);
                }
                _notifications[nodeId] += n.Action;
            }

            _notificationsInitialized = true;
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return UserEntity.Id; }
        }

        /// <summary>
        /// Clears the list of groups the user is in, ensure to call Save afterwords
        /// </summary>
        public void ClearGroups()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            UserEntity.ClearGroups();
        }

        /// <summary>
        /// Adds a group to the list of groups for the user, ensure to call Save() afterwords
        /// </summary>
        public void AddGroup(string groupAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            var group = ApplicationContext.Current.Services.UserService.GetUserGroupByAlias(groupAlias);
            if (group != null)
                UserEntity.AddGroup(group.ToReadOnlyGroup());
        }

        /// <summary>
        /// Returns the assigned user group aliases for the user
        /// </summary>
        /// <returns></returns>
        public string[] GetGroups()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            return UserEntity.Groups.Select(x => x.Alias).ToArray();
        }


        /// <summary>
        /// Gets or sets a value indicating whether the user has access to the Umbraco back end.
        /// </summary>
        /// <value><c>true</c> if the user has access to the back end; otherwise, <c>false</c>.</value>
        public bool NoConsole
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.IsLockedOut;
            }
            set
            {
                UserEntity.IsLockedOut = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="User"/> is disabled.
        /// </summary>
        /// <value><c>true</c> if disabled; otherwise, <c>false</c>.</value>
        public bool Disabled
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.IsApproved == false;
            }
            set
            {
                UserEntity.IsApproved = value == false;
            }
        }
        
        [Obsolete("This should not be used, it will return invalid data because a user can have multiple start nodes, this will only return the first")]
        public int StartNodeId
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.StartContentIds == null || UserEntity.StartContentIds.Length == 0 ? -1 : UserEntity.StartContentIds[0];
            }
            set
            {
                UserEntity.StartContentIds = new int[] { value };
            }
        }

        [Obsolete("This should not be used, it will return invalid data because a user can have multiple start nodes, this will only return the first")]
        public int StartMediaId
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.StartMediaIds == null || UserEntity.StartMediaIds.Length == 0 ? -1 : UserEntity.StartMediaIds[0];
            }
            set
            {
                UserEntity.StartMediaIds = new int[] { value };
            }
        }

        /// <summary>
        /// Flushes the user from cache.
        /// </summary>
        [Obsolete("This method should not be used, cache flushing is handled automatically by event handling in the web application and ensures that all servers are notified, this will not notify all servers in a load balanced environment")]
        public void FlushFromCache()
        {
            OnFlushingFromCache(EventArgs.Empty);
            ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.ClearCache<IUser>();
        }

        /// <summary>
        /// Gets the user with a specified ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Obsolete("The legacy user object should no longer be used, use the WebSecurity class to access the current user or the UserService to retrieve a user by id")]
        public static User GetUser(int id)
        {
            var result = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (result == null)
            {
                throw new ArgumentException("No user found with id " + id);
            }
            return new User(result);
        }

        [Obsolete("This should not be used it exists for legacy reasons only, use user groups and the IUserService instead, it will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddApplication(string appAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            UserEntity.AddAllowedSection(appAlias);
        }

        [Obsolete("This method will implicitly cause a multiple database saves and will reset the current user's dirty property, do not use this method, use the AddApplication method instead and then call Save() when you are done performing all user changes to persist the chagnes in one transaction")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void addApplication(string AppAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            UserEntity.AddAllowedSection(AppAlias);
            //For backwards compatibility this requires an implicit save
            ApplicationContext.Current.Services.UserService.Save(UserEntity);
        }

        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SavingEventHandler(User sender, EventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(User sender, EventArgs e);
        /// <summary>
        /// The disable event handler
        /// </summary>
        public delegate void DisablingEventHandler(User sender, EventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeletingEventHandler(User sender, EventArgs e);
        /// <summary>
        /// The Flush User from cache event handler
        /// </summary>
        public delegate void FlushingFromCacheEventHandler(User sender, EventArgs e);

        /// <summary>
        /// Occurs when [saving].
        /// </summary>
        public static event SavingEventHandler Saving;
        /// <summary>
        /// Raises the <see cref="E:Saving"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSaving(EventArgs e)
        {
            if (Saving != null)
                Saving(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnNew(EventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        /// <summary>
        /// Occurs when [disabling].
        /// </summary>
        public static event DisablingEventHandler Disabling;
        /// <summary>
        /// Raises the <see cref="E:Disabling"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnDisabling(EventArgs e)
        {
            if (Disabling != null)
                Disabling(this, e);
        }

        /// <summary>
        /// Occurs when [deleting].
        /// </summary>
        public static event DeletingEventHandler Deleting;
        /// <summary>
        /// Raises the <see cref="E:Deleting"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnDeleting(EventArgs e)
        {
            if (Deleting != null)
                Deleting(this, e);
        }

        /// <summary>
        /// Occurs when [flushing from cache].
        /// </summary>
        public static event FlushingFromCacheEventHandler FlushingFromCache;
        /// <summary>
        /// Raises the <see cref="E:FlushingFromCache"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnFlushingFromCache(EventArgs e)
        {
            if (FlushingFromCache != null)
                FlushingFromCache(this, e);
        }


    }
}
