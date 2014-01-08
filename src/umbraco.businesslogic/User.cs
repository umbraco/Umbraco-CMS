using System;
using System.Collections;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
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
        private int _id;
        private bool _isInitialized;
        private string _name;
        private string _loginname;
        private int _startnodeid;
        private int _startmediaid;
        private string _email;
        private string _language = "";
        private UserType _usertype;
        private bool _userNoConsole;
        private bool _userDisabled;
        
        private Hashtable _notifications = new Hashtable();
        private bool _notificationsInitialized = false;

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        internal User(IUser user)
        {
            _id = (int)user.Id;
            _userNoConsole = user.IsLockedOut;
            _userDisabled = user.IsApproved;
            _name = user.Name;
            _loginname = user.Username;
            _email = user.Email;
            _language = user.Language;
            _startnodeid = user.StartContentId;
            _startmediaid = user.StartMediaId;
            //this is cached, so should be 'ok'
            _usertype = UserType.GetUserType(user.UserType.Id);

            _isInitialized = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        public User(int ID)
        {
            setupUser(ID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="noSetup">if set to <c>true</c> [no setup].</param>
        public User(int ID, bool noSetup)
        {
            _id = ID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        /// <param name="Password">The password.</param>
        public User(string Login, string Password)
        {
            setupUser(getUserId(Login, Password));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        public User(string Login)
        {
            setupUser(getUserId(Login));
        }

        private void setupUser(int ID)
        {
            _id = ID;

            var dto = ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<UserDto>("WHERE id = @id", new { id = ID});
            if (dto != null)
            {
                _userNoConsole = dto.NoConsole;
                _userDisabled = dto.Disabled;
                _name = dto.UserName;
                _loginname = dto.Login;
                _email = dto.Email;
                _language = dto.UserLanguage;
                _startnodeid = dto.ContentStartId;
                if (dto.MediaStartId.HasValue)
                    _startmediaid = dto.MediaStartId.Value;
                _usertype = UserType.GetUserType(dto.Type);
            }
            else
            {
                throw new ArgumentException("No User exists with ID " + ID);
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public void Save()
        {
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
                if (_isInitialized == false)
                    setupUser(_id);
                return _name;
            }
            set
            {
                _name = value;

                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET UserName = @userName WHERE id = @id", new { userName = value, id = Id});

                FlushFromCache();
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
                if (_isInitialized == false)
                    setupUser(_id);
                return _email;
            }
            set
            {
                _email = value;

                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET UserEmail = @email WHERE id = @id", new { email = value, id = Id });

                FlushFromCache();
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
                if (_isInitialized == false)
                    setupUser(_id);
                return _language;
            }
            set
            {
                _language = value;

                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET userLanguage = @language WHERE id = @id", new { language = value, id = Id });

                FlushFromCache();
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
                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET UserPassword = @pw WHERE id = @id", new { pw = value, id = Id });

                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns></returns>
        public string GetPassword()
        {
            return ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<string>(
                "SELECT UserPassword FROM umbracoUser WHERE id = @id", new {id = Id});
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
            if (_isInitialized == false)
                setupUser(_id);

            var allApps = Application.getAll();
            var apps = new List<Application>();

            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<User2AppDto>(
                "SELECT * FROM umbracoUser2app WHERE [user] = @userID", new {userID = Id});
            foreach (var dto in dtos)
            {
                var app = allApps.SingleOrDefault(x => x.alias == dto.AppAlias);
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
                if (_isInitialized == false)
                    setupUser(_id);
                return _loginname;
            }
            set
            {
                if (EnsureUniqueLoginName(value, this) == false)
                    throw new Exception(String.Format("A user with the login '{0}' already exists", value));

                _loginname = value;

                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET UserLogin = @login WHERE id = @id", new { login = value, id = Id });

                FlushFromCache();
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
                if (_isInitialized == false)
                    setupUser(_id);
                return _usertype;
            }
            set
            {
                _usertype = value;

                ApplicationContext.Current.DatabaseContext.Database.Update<UserDto>(
                    "SET userType = @type WHERE id = @id", new { type = value.Id, id = Id });

                FlushFromCache();
            }
        }


        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        public static User[] getAll()
        {
            IRecordsReader dr = SqlHelper.ExecuteReader("Select id from umbracoUser");

            var users = new List<User>();

            while (dr.Read())
            {
                users.Add(User.GetUser(dr.GetInt("id")));
            }
            dr.Close();

            return users.OrderBy(x => x.Name).ToArray();
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
            var retVal = new List<User>();
            var tmpContainer = new ArrayList();

            IRecordsReader dr = useExactMatch
                ? SqlHelper.ExecuteReader("Select id from umbracoUser where userEmail = @email",
                    SqlHelper.CreateParameter("@email", email))
                : SqlHelper.ExecuteReader("Select id from umbracoUser where userEmail LIKE {0} @email",
                    SqlHelper.CreateParameter("@email", String.Format("%{0}%", email)));
            
            while (dr.Read())
            {
                retVal.Add(GetUser(dr.GetInt("id")));
            }
            dr.Close();

            return retVal.ToArray();
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
		/// <param name="">whether to use a partial match</param>
		/// <returns></returns>
		public static User[] getAllByLoginName(string login, bool partialMatch)
		{
			return GetAllByLoginName(login, partialMatch).ToArray();
		}

        public static IEnumerable<User> GetAllByLoginName(string login, bool partialMatch)
        {

            var users = new List<User>();

            if (partialMatch)
            {
                using (var dr = SqlHelper.ExecuteReader(
                    "Select id from umbracoUser where userLogin LIKE @login", SqlHelper.CreateParameter("@login", String.Format("%{0}%", login))))
                {
                    while (dr.Read())
                    {
                        users.Add(BusinessLogic.User.GetUser(dr.GetInt("id")));
                    }
                }

            }
            else
            {
                using (var dr = SqlHelper.ExecuteReader(
                    "Select id from umbracoUser where userLogin=@login", SqlHelper.CreateParameter("@login", login)))
                {
                    while (dr.Read())
                    {
                        users.Add(BusinessLogic.User.GetUser(dr.GetInt("id")));
                    }
                }
            }

            return users;


        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="name">The full name.</param>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <param name="ut">The user type.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static User MakeNew(string name, string lname, string passw, UserType ut)
        {

            SqlHelper.ExecuteNonQuery(@"
				insert into umbracoUser 
				(UserType,startStructureId,startMediaId, UserName, userLogin, userPassword, userEmail,userLanguage) 
				values (@type,-1,-1,@name,@lname,@pw,'',@lang)",
                SqlHelper.CreateParameter("@lang", GlobalSettings.DefaultUILanguage),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@pw", passw));

            var u = new User(lname);
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static User MakeNew(string name, string lname, string passw, string email, UserType ut)
        {
            SqlHelper.ExecuteNonQuery(@"
				insert into umbracoUser 
				(UserType,startStructureId,startMediaId, UserName, userLogin, userPassword, userEmail,userLanguage) 
				values (@type,-1,-1,@name,@lname,@pw,@email,@lang)",
                SqlHelper.CreateParameter("@lang", GlobalSettings.DefaultUILanguage),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@email", email),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@pw", passw));

            var u = new User(lname);
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
            if (!EnsureUniqueLoginName(lname, User.GetUser(id)))
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));


            SqlHelper.ExecuteNonQuery(@"Update umbracoUser set userName=@name, userLogin=@lname, userEmail=@email, UserType=@type where id = @id",
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@email", email),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@id", id));
        }

        public static void Update(int id, string name, string lname, string email, bool disabled, bool noConsole, UserType ut)
        {
            if (!EnsureUniqueLoginName(lname, User.GetUser(id)))
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));


            SqlHelper.ExecuteNonQuery(@"Update umbracoUser set userName=@name, userLogin=@lname, userEmail=@email, UserType=@type, userDisabled=@disabled, userNoConsole=@noconsole where id = @id",
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@email", email),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@disabled", disabled),
                SqlHelper.CreateParameter("@noconsole", noConsole),
                SqlHelper.CreateParameter("@id", id));
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
            SqlHelper.ExecuteNonQuery(@"Update umbracoUser set userEmail=@email, userDisabled=@disabled, userNoConsole=@noconsole where id = @id",
                SqlHelper.CreateParameter("@email", email),
                SqlHelper.CreateParameter("@disabled", disabled),
                SqlHelper.CreateParameter("@noconsole", noConsole),
                SqlHelper.CreateParameter("@id", id));
        }
        
        /// <summary>
        /// Gets the ID from the user with the specified login name and password
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname, string passw)
        {
            return getUserId("select id from umbracoUser where userDisabled = 0 and userNoConsole = 0 and userLogin = @login and userPassword = @pw",
                SqlHelper.CreateParameter("@login", lname),
                SqlHelper.CreateParameter("@pw", passw));
        }

        /// <summary>
        /// Gets the ID from the user with the specified login name
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname)
        {
            return getUserId("select id from umbracoUser where userLogin = @login",
                 SqlHelper.CreateParameter("@login", lname));
        }

        private static int getUserId(string query, params IParameter[] parameterValues)
        {
            object userId = SqlHelper.ExecuteScalar<object>(query, parameterValues);
            return (userId != null && userId != DBNull.Value) ? int.Parse(userId.ToString()) : -1;
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

            //would be better in the notifications class but since we can't reference the cms project (poorly architected) we need to use raw sql
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodeNotify where userId = @userId", SqlHelper.CreateParameter("@userId", Id));

            //would be better in the permissions class but since we can't reference the cms project (poorly architected) we need to use raw sql
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2NodePermission where userId = @userId", SqlHelper.CreateParameter("@userId", Id));

            //delete the assigned applications
            clearApplications();

            SqlHelper.ExecuteNonQuery("delete from umbracoUserLogins where userID = @id", SqlHelper.CreateParameter("@id", Id));

            SqlHelper.ExecuteNonQuery("delete from umbracoUser where id = @id", SqlHelper.CreateParameter("@id", Id));
            FlushFromCache();
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        public void disable()
        {
            OnDisabling(EventArgs.Empty);
            //change disabled and userLogin (prefix with yyyyMMdd_ )
            this.Disabled = true;
            //MUST clear out the umbraco logins otherwise if they are still logged in they can still do stuff:
            //http://issues.umbraco.org/issue/U4-2042
            SqlHelper.ExecuteNonQuery("delete from umbracoUserLogins where userID = @id", SqlHelper.CreateParameter("@id", Id));
            //can't rename if it's going to take up too many chars
            if (this.LoginName.Length + 9 <= 125)
            {
                this.LoginName = DateTime.Now.ToString("yyyyMMdd") + "_" + this.LoginName;
            }
            this.Save();
        }

        /// <summary>
        /// Gets the users permissions based on a nodes path
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns></returns>
        public string GetPermissions(string Path)
        {
            if (!_isInitialized)
                setupUser(_id);
            
            // NH 4.7.1 changing default permission behavior to default to User Type permissions IF no specific permissions has been
            // set for the current node
            var nodeId = Path.Contains(",") ? int.Parse(Path.Substring(Path.LastIndexOf(",", StringComparison.Ordinal)+1)) : int.Parse(Path);
            return GetPermissions(nodeId);
        }

        [Obsolete("Do not use this, implement something in the service layer!! And make sure we can mock/test it")]
        internal string GetPermissions(int nodeId)
        {
            if (!_isInitialized)
                setupUser(_id);

            string defaultPermissions = UserType.DefaultPermissions;

            //get the cached permissions for the user
            var cachedPermissions = ApplicationContext.Current.ApplicationCache.GetCacheItem(
                string.Format("{0}{1}", CacheKeys.UserPermissionsCacheKey, _id),
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
            if (cachedPermissions.ContainsKey(nodeId))
            {
                return cachedPermissions[nodeId].ToString();
            }

            // exception to everything. If default cruds is empty and we're on root node; allow browse of root node
            if (string.IsNullOrEmpty(defaultPermissions) && nodeId == -1)
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

            if (!_notificationsInitialized)
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
            if (!_isInitialized)
                setupUser(_id);

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
            get { return _id; }
        }

        /// <summary>
        /// Clears the list of applications the user has access to.
        /// </summary>
        public void clearApplications()
        {
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2app where [user] = @id", SqlHelper.CreateParameter("@id", this.Id));
        }

        /// <summary>
        /// Adds a application to the list of allowed applications
        /// </summary>
        /// <param name="AppAlias">The app alias.</param>
        public void addApplication(string AppAlias)
        {
            SqlHelper.ExecuteNonQuery("insert into umbracoUser2app ([user],app) values (@id, @app)", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@app", AppAlias));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has access to the Umbraco back end.
        /// </summary>
        /// <value><c>true</c> if the user has access to the back end; otherwise, <c>false</c>.</value>
        public bool NoConsole
        {
            get
            {
                if (!_isInitialized)
                    setupUser(_id);
                return _userNoConsole;
            }
            set
            {
                _userNoConsole = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set userNoConsole = @userNoConsole where id = @id", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@userNoConsole", _userNoConsole));
                FlushFromCache();
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
                if (!_isInitialized)
                    setupUser(_id);
                return _userDisabled;
            }
            set
            {
                _userDisabled = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set userDisabled = @userDisabled where id = @id", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@userDisabled", _userDisabled));
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
                if (!_isInitialized)
                    setupUser(_id);
                return _startnodeid;
            }
            set
            {

                _startnodeid = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set  startStructureId = @start where id = @id", SqlHelper.CreateParameter("@start", value), SqlHelper.CreateParameter("@id", this.Id));
                FlushFromCache();
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
                if (!_isInitialized)
                    setupUser(_id);
                return _startmediaid;
            }
            set
            {

                _startmediaid = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set  startMediaId = @start where id = @id", SqlHelper.CreateParameter("@start", value), SqlHelper.CreateParameter("@id", this.Id));
                FlushFromCache();
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
        [Obsolete("The legacy user object should no longer be used, use the WebSecurity class to access the current user or the UserService to retreive a user by id")]
        public static User GetUser(int id)
        {
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
