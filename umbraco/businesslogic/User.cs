using System;
using System.Data;
using System.Configuration;
using System.Collections;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Linq;

namespace umbraco.BusinessLogic {
    /// <summary>
    /// represents a Umbraco back end user
    /// </summary>
    public class User {
        // private SqlConnection _conn = new SqlConnection(GlobalSettings.DbDSN);


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
        private bool _defaultToLiveEditing;

        private Hashtable _cruds = new Hashtable();
        private bool _crudsInitialized = false;

        private Hashtable _notifications = new Hashtable();
        private bool _notificationsInitialized = false;

        private static ISqlHelper SqlHelper {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        public User(int ID) {
            setupUser(ID);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="noSetup">if set to <c>true</c> [no setup].</param>
        public User(int ID, bool noSetup) {
            _id = ID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        /// <param name="Password">The password.</param>
        public User(string Login, string Password) {
            setupUser(getUserId(Login, Password));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="Login">The login.</param>
        public User(string Login) {
            setupUser(getUserId(Login));
        }

        private void setupUser(int ID) {
            _id = ID;

            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                "Select userNoConsole, userDisabled, userType,startStructureID, startMediaId, userName,userLogin,userEmail,userDefaultPermissions, userLanguage, defaultToLiveEditing from umbracoUser where id = @id",
                SqlHelper.CreateParameter("@id", ID))) {
                if (dr.Read()) {
                    _userNoConsole = dr.GetBoolean("usernoconsole");
                    _userDisabled = dr.GetBoolean("userDisabled");
                    _name = dr.GetString("userName");
                    _loginname = dr.GetString("userLogin");
                    _email = dr.GetString("userEmail");
                    _language = dr.GetString("userLanguage");
                    _startnodeid = dr.GetInt("startStructureID");
                    if (!dr.IsNull("startMediaId"))
                        _startmediaid = dr.GetInt("startMediaID");
                    _usertype = UserType.GetUserType(dr.GetShort("UserType"));
                    _defaultToLiveEditing = dr.GetBoolean("defaultToLiveEditing");
                }
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public void Save() {
            OnSaving(EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the users name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _name;
            }
            set {
                _name = value;
                SqlHelper.ExecuteNonQuery("Update umbracoUser set UserName = @userName where id = @id", SqlHelper.CreateParameter("@userName", value), SqlHelper.CreateParameter("@id", Id));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets or sets the users email.
        /// </summary>
        /// <value>The email.</value>
        public string Email {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _email;
            }
            set {
                _email = value;
                SqlHelper.ExecuteNonQuery("Update umbracoUser set UserEmail = @email where id = @id", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@email", value));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets or sets the users language.
        /// </summary>
        /// <value>The language.</value>
        public string Language {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _language;
            }
            set {
                _language = value;
                SqlHelper.ExecuteNonQuery("Update umbracoUser set userLanguage = @language where id = @id", SqlHelper.CreateParameter("@language", value), SqlHelper.CreateParameter("@id", Id));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets or sets the users password.
        /// </summary>
        /// <value>The password.</value>
        public string Password {
            get {
                return GetPassword();
            }
            set {
                SqlHelper.ExecuteNonQuery("Update umbracoUser set UserPassword = @pw where id = @id", SqlHelper.CreateParameter("@pw", value), SqlHelper.CreateParameter("@id", Id));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns></returns>
        public string GetPassword() { 
            return 
                SqlHelper.ExecuteScalar<string>("select UserPassword from umbracoUser where id = @id",
                SqlHelper.CreateParameter("@id", this.Id));
        }
        static string _connstring = GlobalSettings.DbDSN;

        /// <summary>
        /// Determines whether this user is an admin.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this user is admin; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAdmin() {
            return UserType.Alias == "admin";
        }

        public bool ValidatePassword(string password) {
            string userLogin =
                SqlHelper.ExecuteScalar<string>("select userLogin from umbracoUser where userLogin = @login and UserPassword = @pw", 
                SqlHelper.CreateParameter("@pw", password),
                SqlHelper.CreateParameter("@login", LoginName)
                );
            return userLogin == this.LoginName;
        }

        /// <summary>
        /// Determines whether this user is the root (super user).
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this user is root; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRoot() {
            return Id == 0;
        }

        /// <summary>
        /// Gets the applications which the user has access to.
        /// </summary>
        /// <value>The users applications.</value>
        public Application[] Applications {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                ArrayList al = new ArrayList();

                using (IRecordsReader appIcons = SqlHelper.ExecuteReader("select appAlias, appIcon, appname from umbracoApp app join umbracoUser2app u2a on u2a.app = app.appAlias and u2a.[user] = @userID order by app.sortOrder", SqlHelper.CreateParameter("@userID", this.Id))) {
                    while (appIcons.Read()) {
                        Application tmp = new Application();
                        tmp.name = appIcons.GetString("appName");
                        tmp.icon = appIcons.GetString("appIcon");
                        tmp.alias = appIcons.GetString("appAlias");
                        al.Add(tmp);
                    }
                }

                Application[] retVal = new Application[al.Count];

                for (int i = 0; i < al.Count; i++) {
                    retVal[i] = (Application)al[i];
                }
                return retVal;
            }
        }

        /// <summary>
        /// Gets or sets the users  login name
        /// </summary>
        /// <value>The loginname.</value>
        public string LoginName {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _loginname;
            }
            set {
                if (!ensureUniqueLoginName(value, this))
                    throw new Exception(String.Format("A user with the login '{0}' already exists", value));
                _loginname = value;
                SqlHelper.ExecuteNonQuery("Update umbracoUser set UserLogin = @login where id = @id", SqlHelper.CreateParameter("@login", value), SqlHelper.CreateParameter("@id", Id));
                FlushFromCache();
            }
        }

        private static bool ensureUniqueLoginName(string loginName, User currentUser) {
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
        public static bool validateCredentials(string lname, string passw) {
            return validateCredentials(lname, passw, true);
        }

        /// <summary>
        /// Validates the users credentials.
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <param name="checkForUmbracoConsoleAccess">if set to <c>true</c> [check for umbraco console access].</param>
        /// <returns></returns>
        public static bool validateCredentials(string lname, string passw, bool checkForUmbracoConsoleAccess) {
            string consoleCheckSql = "";
            if (checkForUmbracoConsoleAccess)
                consoleCheckSql = "and userNoConsole = 0 ";

            object tmp = SqlHelper.ExecuteScalar<object>(
                "select id from umbracoUser where userDisabled = 0 " + consoleCheckSql + " and userLogin = @login and userPassword = @pw", SqlHelper.CreateParameter("@login", lname), SqlHelper.CreateParameter("@pw", passw)
                );

            // Logging
            if (tmp == null)
                BusinessLogic.Log.Add(BusinessLogic.LogTypes.LoginFailure, BusinessLogic.User.GetUser(0), -1, "Login: '" + lname + "' failed, from IP: " + System.Web.HttpContext.Current.Request.UserHostAddress);
            return (tmp != null);
        }

        /// <summary>
        /// Gets or sets the type of the user.
        /// </summary>
        /// <value>The type of the user.</value>
        public UserType UserType {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _usertype;
            }
            set {
                _usertype = value;
                SqlHelper.ExecuteNonQuery(
                    @"Update umbracoUser set userType = @type where id = @id",
                    SqlHelper.CreateParameter("@type", value.Id),
                    SqlHelper.CreateParameter("@id", Id));
                FlushFromCache();
            }
        }


        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns></returns>
        public static User[] getAll() {            

            IRecordsReader dr;
            dr = SqlHelper.ExecuteReader("Select id from umbracoUser");

            List<User> users = new List<User>();

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
            if (umbraco.BasePages.BasePage.umbracoUserContextID != "")
                return BusinessLogic.User.GetUser(umbraco.BasePages.BasePage.GetUserId(umbraco.BasePages.BasePage.umbracoUserContextID));
            else
                return null;

        }

        /// <summary>
        /// Gets all users by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static User[] getAllByEmail(string email) {
            System.Collections.ArrayList tmpContainer = new System.Collections.ArrayList();

            IRecordsReader dr;
            dr = SqlHelper.ExecuteReader(
                "Select id from umbracoUser where userEmail LIKE %@email%", SqlHelper.CreateParameter("@email", email));

            while (dr.Read()) {
                tmpContainer.Add(BusinessLogic.User.GetUser(dr.GetInt("id")));
            }
            dr.Close();
            User[] retVal = new User[tmpContainer.Count];

            int c = 0;
            foreach (User u in tmpContainer) {
                retVal[c] = u;
                c++;
            }
            return retVal;
        }

        /// <summary>
        /// Gets all users by login name.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public static User[] getAllByLoginName(string login) {
            System.Collections.ArrayList tmpContainer = new System.Collections.ArrayList();

            IRecordsReader dr;
            dr = SqlHelper.ExecuteReader(
                "Select id from umbracoUser where userLogin LIKE @login", SqlHelper.CreateParameter("@login", String.Format("%{0}%", login)));

            while (dr.Read()) {
                tmpContainer.Add(BusinessLogic.User.GetUser(dr.GetInt("id")));
            }
            dr.Close();
            User[] retVal = new User[tmpContainer.Count];

            int c = 0;
            foreach (User u in tmpContainer) {
                retVal[c] = u;
                c++;
            }
            return retVal;
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="name">The full name.</param>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <param name="ut">The user type.</param>
        public static void MakeNew(string name, string lname, string passw, UserType ut) {

            SqlHelper.ExecuteNonQuery(@"
				insert into umbracoUser 
				(UserType,startStructureId,startMediaId, UserName, userLogin, userPassword, userEmail,userLanguage) 
				values (@type,-1,-1,@name,@lname,@pw,'',@lang)",
                SqlHelper.CreateParameter("@lang", GlobalSettings.DefaultUILanguage),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@pw", passw));

            new User(lname).OnNew(EventArgs.Empty);
        }


        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="lname">The lname.</param>
        /// <param name="passw">The passw.</param>
        /// <param name="email">The email.</param>
        /// <param name="ut">The ut.</param>
        public static void MakeNew(string name, string lname, string passw, string email, UserType ut) {
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

            new User(lname).OnNew(EventArgs.Empty);
        }


        /// <summary>
        /// Updates the name, login name and password for the user with the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="lname">The lname.</param>
        /// <param name="email">The email.</param>
        /// <param name="ut">The ut.</param>
        public static void Update(int id, string name, string lname, string email, UserType ut) {
            if (!ensureUniqueLoginName(lname, User.GetUser(id)))
                throw new Exception(String.Format("A user with the login '{0}' already exists", lname));

            
            SqlHelper.ExecuteNonQuery(@"Update umbracoUser set userName=@name, userLogin=@lname, userEmail=@email, UserType=@type where id = @id",
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@lname", lname),
                SqlHelper.CreateParameter("@email", email),
                SqlHelper.CreateParameter("@type", ut.Id),
                SqlHelper.CreateParameter("@id", id));
        }

        /// <summary>
        /// Gets the ID from the user with the specified login name and password
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <param name="passw">The password.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname, string passw) {
            return getUserId("select id from umbracoUser where userDisabled = 0 and userNoConsole = 0 and userLogin = @login and userPassword = @pw",
                SqlHelper.CreateParameter("@login", lname),
                SqlHelper.CreateParameter("@pw", passw));
        }

        /// <summary>
        /// Gets the ID from the user with the specified login name
        /// </summary>
        /// <param name="lname">The login name.</param>
        /// <returns>a user ID</returns>
        public static int getUserId(string lname) {
            return getUserId("select id from umbracoUser where userLogin = @login",
                 SqlHelper.CreateParameter("@login", lname));
        }

        private static int getUserId(string query, params IParameter[] parameterValues) {
            object userId = SqlHelper.ExecuteScalar<object>(query, parameterValues);
            return (userId != null && userId!=DBNull.Value) ? int.Parse(userId.ToString()) : -1;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void delete() {
            OnDeleting(EventArgs.Empty);

            SqlHelper.ExecuteNonQuery("delete from umbracoUser where id = @id", SqlHelper.CreateParameter("@id", Id));
            FlushFromCache();
        }

        /// <summary>
        /// Disables this instance.
        /// </summary>
        public void disable() {
            OnDisabling(EventArgs.Empty);
            this.Disabled = true;
        }

        /// <summary>
        /// Gets the users permissions based on a nodes path
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns></returns>
        public string GetPermissions(string Path) {
            if (!_isInitialized)
                setupUser(_id);
            string cruds = UserType.DefaultPermissions;

            if (!_crudsInitialized)
                initCruds();

            foreach (string nodeId in Path.Split(',')) {
                if (_cruds.ContainsKey(int.Parse(nodeId)))
                    cruds = _cruds[int.Parse(nodeId)].ToString();
            }

            return cruds;
        }

        /// <summary>
        /// Initializes the user node permissions
        /// </summary>
        public void initCruds() {
            if (!_isInitialized)
                setupUser(_id);

            // clear cruds
            System.Web.HttpContext.Current.Application.Lock();
            _cruds.Clear();
            System.Web.HttpContext.Current.Application.UnLock();

            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodePermission where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", this.Id))) {
                //	int currentId = -1;
                while (dr.Read()) {
                    if (!_cruds.ContainsKey(dr.GetInt("nodeId")))
                        _cruds.Add(dr.GetInt("nodeId"), String.Empty);

                    _cruds[dr.GetInt("nodeId")] += dr.GetString("permission");
                }
            }
            _crudsInitialized = true;
        }

        /// <summary>
        /// Gets a users notifications for a specified node path.
        /// </summary>
        /// <param name="Path">The node path.</param>
        /// <returns></returns>
        public string GetNotifications(string Path) {
            string notifications = "";

            if (!_notificationsInitialized)
                initNotifications();

            foreach (string nodeId in Path.Split(',')) {
                if (_notifications.ContainsKey(int.Parse(nodeId)))
                    notifications = _notifications[int.Parse(nodeId)].ToString();
            }

            return notifications;
        }

        /// <summary>
        /// Clears the internal hashtable containing cached information about notifications for the user
        /// </summary>
        public void resetNotificationCache() {
            _notificationsInitialized = false;
            _notifications.Clear();
        }

        /// <summary>
        /// Initializes the notifications and caches them.
        /// </summary>
        public void initNotifications() {
            if (!_isInitialized)
                setupUser(_id);

            using (IRecordsReader dr = SqlHelper.ExecuteReader("select * from umbracoUser2NodeNotify where userId = @userId order by nodeId", SqlHelper.CreateParameter("@userId", this.Id))) {
                while (dr.Read()) {
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
        public int Id {
            get { return _id; }
        }

        /// <summary>
        /// Clears the list of applications the user has access to.
        /// </summary>
        public void clearApplications() {
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2app where [user] = @id", SqlHelper.CreateParameter("@id", this.Id));
        }

        /// <summary>
        /// Adds a application to the list of allowed applications
        /// </summary>
        /// <param name="AppAlias">The app alias.</param>
        public void addApplication(string AppAlias) {
            SqlHelper.ExecuteNonQuery("insert into umbracoUser2app ([user],app) values (@id, @app)", SqlHelper.CreateParameter("@id", this.Id), SqlHelper.CreateParameter("@app", AppAlias));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has access to the Umbraco back end.
        /// </summary>
        /// <value><c>true</c> if the user has access to the back end; otherwise, <c>false</c>.</value>
        public bool NoConsole {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _userNoConsole;
            }
            set {
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
        /// Gets or sets a value indicating whether a user should be redirected to liveediting by default.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if defaults to live editing; otherwise, <c>false</c>.
        /// </value>
        public bool DefaultToLiveEditing
        {
            get
            {
                if (!_isInitialized)
                    setupUser(_id);
                return _defaultToLiveEditing;
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
        public int StartNodeId {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _startnodeid;
            }
            set {

                _startnodeid = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set  startStructureId = @start where id = @id", SqlHelper.CreateParameter("@start", value), SqlHelper.CreateParameter("@id", this.Id));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>The start media id.</value>
        public int StartMediaId {
            get {
                if (!_isInitialized)
                    setupUser(_id);
                return _startmediaid;
            }
            set {

                _startmediaid = value;
                SqlHelper.ExecuteNonQuery("update umbracoUser set  startMediaId = @start where id = @id", SqlHelper.CreateParameter("@start", value), SqlHelper.CreateParameter("@id", this.Id));
                FlushFromCache();
            }
        }

        /// <summary>
        /// Flushes the user from cache.
        /// </summary>
        protected void FlushFromCache() {
            OnFlushingFromCache(EventArgs.Empty);

            if (System.Web.HttpRuntime.Cache[string.Format("UmbracoUser{0}", Id.ToString())] != null)
                System.Web.HttpRuntime.Cache.Remove(string.Format("UmbracoUser{0}", Id.ToString()));
        }

        /// <summary>
        /// Gets the user with a specified ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static User GetUser(int id) {
            if (System.Web.HttpRuntime.Cache[string.Format("UmbracoUser{0}", id.ToString())] == null) {
                User u = new User(id);
                System.Web.HttpRuntime.Cache.Insert(string.Format("UmbracoUser{0}", id.ToString()), u);
            }
            return (User)System.Web.HttpRuntime.Cache[string.Format("UmbracoUser{0}", id.ToString())];
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
        protected virtual void OnSaving(EventArgs e) {
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
        protected virtual void OnNew(EventArgs e) {
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
        protected virtual void OnDisabling(EventArgs e) {
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
        protected virtual void OnDeleting(EventArgs e) {
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
        protected virtual void OnFlushingFromCache(EventArgs e) {
            if (FlushingFromCache != null)
                FlushingFromCache(this, e);
        }


    }
}
