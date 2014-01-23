using System;
using System.Collections;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// represents a Umbraco back end user
    /// </summary>
    public class User
    {
        private IUser _user;
        private int? _lazyId;

        //private int _id;
        //private bool _isInitialized;
        //private string _name;
        //private string _loginname;
        //private int _startnodeid;
        //private int _startmediaid;
        //private string _email;
        //private string _language = "";
        //private UserType _usertype;
        //private bool _userNoConsole;
        //private bool _userDisabled;
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
            _user = user;
            //_id = user.Id;
            //_userNoConsole = user.IsLockedOut;
            //_userDisabled = user.IsApproved;
            //_name = user.Name;
            //_loginname = user.Username;
            //_email = user.Email;
            //_language = user.Language;
            //_startnodeid = user.StartContentId;
            //_startmediaid = user.StartMediaId;
            
            //_usertype = new UserType(_user.UserType);

            //_isInitialized = true;
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
            _user = ApplicationContext.Current.Services.UserService.GetById(ID);
            if (_user == null)
            {
                throw new ArgumentException("No User exists with ID " + ID);
            }
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public void Save()
        {
            FlushFromCache();
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
                return _user.Name;
            }
            set
            {
                _user.Name = value;
                
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
                return _user.Email;
            }
            set
            {
                _user.Email = value;
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
                return _user.Language;
            }
            set
            {
                _user.Language = value;
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
                _user.Language = value;
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);
            return _user.Password;
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

            var sections = _user.AllowedSections;

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
                return _user.Username;
            }
            set
            {
                if (EnsureUniqueLoginName(value, this) == false)
                    throw new Exception(String.Format("A user with the login '{0}' already exists", value));

                _user.Username = value;
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
                return new UserType(_user.UserType);
            }
            set
            {
                _user.UserType = value.UserTypeItem;
            }
        }


        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        public static User[] getAll()
        {
            int totalRecs;
            var users = ApplicationContext.Current.Services.UserService.GetAllMembers(
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
                if (umbraco.BasePages.BasePage.umbracoUserContextID != "")
                    return BusinessLogic.User.GetUser(umbraco.BasePages.BasePage.GetUserId(umbraco.BasePages.BasePage.umbracoUserContextID));
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
                return ApplicationContext.Current.Services.UserService.FindMembersByEmail(
                    email, 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Exact)
                    .Select(x => new User(x))
                    .ToArray();
            }
            else
            {
                return ApplicationContext.Current.Services.UserService.FindMembersByEmail(
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
                return ApplicationContext.Current.Services.UserService.FindMembersByUsername(
                    string.Format("%{0}%", login), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.Wildcard)
                    .Select(x => new User(x))
                    .ToArray();
            }
            else
            {
                return ApplicationContext.Current.Services.UserService.FindMembersByUsername(
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

            var found = ApplicationContext.Current.Services.UserService.GetById(id);
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

            var found = ApplicationContext.Current.Services.UserService.GetById(id);
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
            var found = ApplicationContext.Current.Services.UserService.GetById(id);
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
            return found.Password == passw ? found.Id : -1;
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

            ApplicationContext.Current.Services.UserService.Delete(_user, true);

            FlushFromCache();
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        public void disable()
        {
            OnDisabling(EventArgs.Empty);

            //delete without the true overload will perform the disable operation
            ApplicationContext.Current.Services.UserService.Delete(_user);
        }

        /// <summary>
        /// Gets the users permissions based on a nodes path
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns></returns>
        public string GetPermissions(string Path)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            string defaultPermissions = UserType.DefaultPermissions;

            //TODO: Wrap all this with the new services!

            //get the cached permissions for the user
            var cachedPermissions = ApplicationContext.Current.ApplicationCache.GetCacheItem(
                string.Format("{0}{1}", CacheKeys.UserPermissionsCacheKey, _user.Id),
                //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will make this priority below average
                CacheItemPriority.BelowNormal, 
                null,
                //Since this cache can be quite large (http://issues.umbraco.org/issue/U4-2161) we will only have this exist in cache for 20 minutes, 
                // then it will refresh from the database.
                new TimeSpan(0, 20, 0),
                () =>
                    {
                        var cruds = new Hashtable();
                        using (var dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodePermission where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", this.Id)))
                        {
                            while (dr.Read())
                            {
                                if (!cruds.ContainsKey(dr.GetInt("nodeId")))
                                {
                                    cruds.Add(dr.GetInt("nodeId"), string.Empty);
                                }
                                cruds[dr.GetInt("nodeId")] += dr.GetString("permission");
                            }
                        }
                        return cruds;
                    });

            // NH 4.7.1 changing default permission behavior to default to User Type permissions IF no specific permissions has been
            // set for the current node
            var nodeId = Path.Contains(",") ? int.Parse(Path.Substring(Path.LastIndexOf(",", StringComparison.Ordinal)+1)) : int.Parse(Path);
            if (cachedPermissions.ContainsKey(nodeId))
            {
                return cachedPermissions[int.Parse(Path.Substring(Path.LastIndexOf(",", StringComparison.Ordinal) + 1))].ToString();
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
            //TODO: Wrap all this with new services!

            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodeNotify where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", this.Id)))
            {
                while (dr.Read())
                {
                    int nodeId = dr.GetInt("nodeId");
                    if (!_notifications.ContainsKey(nodeId))
                        _notifications.Add(nodeId, String.Empty);

                    _notifications[nodeId] += dr.GetString("action");
                }
            }
            _notificationsInitialized = true;
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _user.Id; }
        }

        /// <summary>
        /// Clears the list of applications the user has access to.
        /// </summary>
        public void clearApplications()
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            foreach (var s in _user.AllowedSections.ToArray())
            {
                _user.RemoveAllowedSection(s);
            }

            ApplicationContext.Current.Services.UserService.Save(_user);
        }

        /// <summary>
        /// Adds a application to the list of allowed applications
        /// </summary>
        /// <param name="AppAlias">The app alias.</param>
        public void addApplication(string AppAlias)
        {
            if (_lazyId.HasValue) SetupUser(_lazyId.Value);

            _user.AddAllowedSection(AppAlias);

            ApplicationContext.Current.Services.UserService.Save(_user);
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
                return _user.IsLockedOut;
            }
            set
            {
                _user.IsLockedOut = value;
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
                return _user.IsApproved == false;
            }
            set
            {
                _user.IsApproved = value == false;
            }
        }

        //NOTE: we cannot wrap this because it's no longer supported so we'll just go directly to the db
        public bool DefaultToLiveEditing
        {
            get
            {                
                if (_defaultToLiveEditing.HasValue == false)
                {
                    _defaultToLiveEditing = SqlHelper.ExecuteScalar<bool>("select defaultToLiveEditing from umbracoUser where id = @id",
                        SqlHelper.CreateParameter("@id", Id));
                }
                return _defaultToLiveEditing.Value;
            }
            set
            {
                _defaultToLiveEditing = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set defaultToLiveEditing = @defaultToLiveEditing where id = @id", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@defaultToLiveEditing", _defaultToLiveEditing));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets or sets the start content node id.
        /// </summary>
        /// <value>The start node id.</value>
        public int StartNodeId
        {
            get
            {
                if (_lazyId.HasValue) SetupUser(_lazyId.Value);
                return _user.StartContentId;
            }
            set
            {
                _user.StartContentId = value;
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
                return _user.StartMediaId;
            }
            set
            {
                _user.StartMediaId = value;
            }
        }

        /// <summary>
        /// Flushes the user from cache.
        /// </summary>
        [Obsolete("This method should not be used, cache flushing is handled automatically by event handling in the web application and ensures that all servers are notified, this will not notify all servers in a load balanced environment")]
        public void FlushFromCache()
        {
            OnFlushingFromCache(EventArgs.Empty);
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.UserCacheKey, Id.ToString()));            
        }

        /// <summary>
        /// Gets the user with a specified ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static User GetUser(int id)
        {
            //TODO: Ref: http://issues.umbraco.org/issue/U4-4123

            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                string.Format("{0}{1}", CacheKeys.UserCacheKey, id.ToString()), () =>
                    {
                        try
                        {
                            return new User(id);
                        }
                        catch (ArgumentException)
                        {
                            //no user was found
                            return null;
                        }
                    });
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
