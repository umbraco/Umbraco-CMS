using System;
using System.Collections;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

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
        public bool IsAdmin()
        {
            return UserType.Alias == "admin";
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

            object tmp = SqlHelper.ExecuteScalar<object>(
                "select id from umbracoUser where userDisabled = 0 " + consoleCheckSql + " and userLogin = @login and userPassword = @pw", 
                SqlHelper.CreateParameter("@login", lname), 
                SqlHelper.CreateParameter("@pw", passw)
                );

            // Logging
            if (tmp == null)
            {
				LogHelper.Info<User>("Login: '" + lname + "' failed, from IP: " + System.Web.HttpContext.Current.Request.UserHostAddress);
            }
                
            return (tmp != null);
        }

        /// <summary>
        /// Gets or sets the type of the user.
        /// </summary>
        /// <value>The type of the user.</value>
        public UserType UserType
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return new UserType(UserEntity.UserType);
            }
            set
            {
                UserEntity.UserType = value.UserTypeItem;
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
        /// <param name="ut">The user type.</param>
        public static User MakeNew(string name, string lname, string passw, UserType ut)
        {
            var user = new Umbraco.Core.Models.Membership.User(name, "", lname, passw, ut.UserTypeItem);
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
        /// <param name="ut">The ut.</param>        
        public static User MakeNew(string name, string lname, string passw, string email, UserType ut)
        {
            var user = new Umbraco.Core.Models.Membership.User(name, email, lname, passw, ut.UserTypeItem);
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
        /// <param name="ut">The ut.</param>
        public static void Update(int id, string name, string lname, string email, UserType ut)
        {
            if (EnsureUniqueLoginName(lname, GetUser(id)) == false)
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));

            var found = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (found == null) return;
            found.Name = name;
            found.Username = lname;
            found.Email = email;
            found.UserType = ut.UserTypeItem;
            ApplicationContext.Current.Services.UserService.Save(found);
        }

        public static void Update(int id, string name, string lname, string email, bool disabled, bool noConsole, UserType ut)
        {
            if (EnsureUniqueLoginName(lname, GetUser(id)) == false)
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));

            var found = ApplicationContext.Current.Services.UserService.GetUserById(id);
            if (found == null) return;
            found.Name = name;
            found.Username = lname;
            found.Email = email;
            found.UserType = ut.UserTypeItem;
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
        /// <param name="Path">The path.</param>
        /// <returns></returns>
        public string GetPermissions(string Path)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            var defaultPermissions = UserType.DefaultPermissions;

            var cachedPermissions = ApplicationContext.Current.Services.UserService.GetPermissions(UserEntity)
                .ToArray();

            // NH 4.7.1 changing default permission behavior to default to User Type permissions IF no specific permissions has been
            // set for the current node
            var nodeId = Path.Contains(",") ? int.Parse(Path.Substring(Path.LastIndexOf(",", StringComparison.Ordinal) + 1)) : int.Parse(Path);
            if (cachedPermissions.Any(x => x.EntityId == nodeId))
            {
                var found = cachedPermissions.First(x => x.EntityId == nodeId);
                return string.Join("", found.AssignedPermissions);
            }

            // exception to everything. If default cruds is empty and we're on root node; allow browse of root node
            if (string.IsNullOrEmpty(defaultPermissions) && Path == "-1")
                defaultPermissions = "F";

            // else return default user type cruds
            return defaultPermissions;
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
        /// Clears the list of applications the user has access to, ensure to call Save afterwords
        /// </summary>
        public void ClearApplications()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            foreach (var s in UserEntity.AllowedSections.ToArray())
            {
                UserEntity.RemoveAllowedSection(s);
            }
        }

        /// <summary>
        /// Clears the list of applications the user has access to.
        /// </summary>
        [Obsolete("This method will implicitly cause a database save and will reset the current user's dirty property, do not use this method, use the ClearApplications method instead and then call Save() when you are done performing all user changes to persist the chagnes in one transaction")]
        public void clearApplications()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            foreach (var s in UserEntity.AllowedSections.ToArray())
            {
                UserEntity.RemoveAllowedSection(s);
            }

            //For backwards compatibility this requires an implicit save
            ApplicationContext.Current.Services.UserService.Save(UserEntity);
        }

        /// <summary>
        /// Adds a application to the list of allowed applications, ensure to call Save() afterwords
        /// </summary>
        /// <param name="appAlias"></param>
        public void AddApplication(string appAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            UserEntity.AddAllowedSection(appAlias);
        }

        /// <summary>
        /// Adds a application to the list of allowed applications
        /// </summary>
        /// <param name="AppAlias">The app alias.</param>
        [Obsolete("This method will implicitly cause a multiple database saves and will reset the current user's dirty property, do not use this method, use the AddApplication method instead and then call Save() when you are done performing all user changes to persist the chagnes in one transaction")]
        public void addApplication(string AppAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            UserEntity.AddAllowedSection(AppAlias);

            //For backwards compatibility this requires an implicit save
            ApplicationContext.Current.Services.UserService.Save(UserEntity);
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

        /// <summary>
        /// <summary>
        /// Gets or sets the start content node id.
        /// </summary>
        /// <value>The start node id.</value>
        public int StartNodeId
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.StartContentId;
            }
            set
            {
                UserEntity.StartContentId = value;
            }
        }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>The start media id.</value>
        public int StartMediaId
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return UserEntity.StartMediaId;
            }
            set
            {
                UserEntity.StartMediaId = value;
            }
        }

        /// <summary>
        /// Flushes the user from cache.
        /// </summary>
        [Obsolete("This method should not be used, cache flushing is handled automatically by event handling in the web application and ensures that all servers are notified, this will not notify all servers in a load balanced environment")]
        public void FlushFromCache()
        {
            OnFlushingFromCache(EventArgs.Empty);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IUser>();
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
