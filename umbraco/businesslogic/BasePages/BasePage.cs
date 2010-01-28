using System;
using System.Data;
using System.Web;

using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.IO;

namespace umbraco.BasePages {
    /// <summary>
    /// umbraco.BasePages.BasePage is the default page type for the umbraco backend.
    /// The basepage keeps track of the current user and the page context. But does not 
    /// Restrict access to the page itself.
    /// The keep the page secure, the umbracoEnsuredPage class should be used instead
    /// </summary>
    public class BasePage : System.Web.UI.Page {
        private User _user;
        private bool _userisValidated = false;
		private ClientTools m_clientTools;

        // ticks per minute 600,000,000 
        private static long _ticksPrMinute = 600000000;
        private static int _umbracoTimeOutInMinutes = GlobalSettings.TimeOutInMinutes;

        /// <summary>
        /// The path to the umbraco root folder
        /// </summary>
        protected string UmbracoPath = SystemDirectories.Umbraco;

        /// <summary>
        /// The current user ID
        /// </summary>
        protected int uid = 0;

        /// <summary>
        /// The page timeout in seconds.
        /// </summary>
        protected long timeout = 0;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper {
            get { return umbraco.BusinessLogic.Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePage"/> class.
        /// </summary>
        public BasePage() {
        }

		/// <summary>
		/// Returns the current BasePage for the current request. 
		/// This assumes that the current page is a BasePage, otherwise, returns null;
		/// </summary>
		public static BasePage Current
		{
			get
			{
				return HttpContext.Current.CurrentHandler as BasePage;
			}
		}

		/// <summary>
		/// Returns a refernce of an instance of ClientTools for access to the pages client API
		/// </summary>
		public ClientTools ClientTools
		{
			get
			{
				if (m_clientTools == null)
					m_clientTools = new ClientTools(this);
				return m_clientTools;
			}
		}

		[Obsolete("Use ClientTools instead")]
        public void RefreshPage(int Seconds) 
		{
			ClientTools.RefreshAdmin(Seconds);
        }

        private void validateUser() {
            if ((umbracoUserContextID != "")) {
                uid = GetUserId(umbracoUserContextID);
                timeout = GetTimeout(umbracoUserContextID);

                if (timeout > DateTime.Now.Ticks) {
                    _user = BusinessLogic.User.GetUser(uid);

                    // Check for console access
                    if (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(HttpContext.Current) && !GlobalSettings.RequestIsLiveEditRedirector(HttpContext.Current))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    } 
                    else
                    {
                        _userisValidated = true;
                        updateLogin();
                    }

                } else {
                    throw new ArgumentException("User has timed out!!");
                }
            } else
                throw new ArgumentException("The user has no umbraco contextid - try logging in");
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="umbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        public static int GetUserId(string umbracoUserContextID) {
            try {
                if (System.Web.HttpRuntime.Cache["UmbracoUserContext" + umbracoUserContextID] == null) {
                    System.Web.HttpRuntime.Cache.Insert(
                        "UmbracoUserContext" + umbracoUserContextID,
                        SqlHelper.ExecuteScalar<int>("select userID from umbracoUserLogins where contextID = @contextId",
                                      SqlHelper.CreateParameter("@contextId", new Guid(umbracoUserContextID))
                        ),
                        null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration,
    new TimeSpan(0, (int)(_umbracoTimeOutInMinutes / 10), 0));


                }

                return (int)System.Web.HttpRuntime.Cache["UmbracoUserContext" + umbracoUserContextID];

            } catch {
                return -1;
            }
        }


        // Added by NH to use with webservices authentications
        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="umbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        public static bool ValidateUserContextID(string umbracoUserContextID) {
            if ((umbracoUserContextID != "")) {
                int uid = GetUserId(umbracoUserContextID);
                long timeout = GetTimeout(umbracoUserContextID);

                if (timeout > DateTime.Now.Ticks) {
                    return true;
                } else {
                    BusinessLogic.Log.Add(BusinessLogic.LogTypes.Logout, BusinessLogic.User.GetUser(uid), -1, "");

                    return false;
                }
            } else
                return false;
        }

        private static long GetTimeout(string umbracoUserContextID) {
            if (System.Web.HttpRuntime.Cache["UmbracoUserContextTimeout" + umbracoUserContextID] == null) {
                System.Web.HttpRuntime.Cache.Insert(
                    "UmbracoUserContextTimeout" + umbracoUserContextID,
                        SqlHelper.ExecuteScalar<long>("select timeout from umbracoUserLogins where contextId=@contextId",
                                          SqlHelper.CreateParameter("@contextId", new Guid(umbracoUserContextID))
                        ),
                    null,
                    DateTime.Now.AddMinutes(_umbracoTimeOutInMinutes / 10), System.Web.Caching.Cache.NoSlidingExpiration);


            }

            return (long)System.Web.HttpRuntime.Cache["UmbracoUserContextTimeout" + umbracoUserContextID];

        }

        // Changed to public by NH to help with webservice authentication
        /// <summary>
        /// Gets or sets the umbraco user context ID.
        /// </summary>
        /// <value>The umbraco user context ID.</value>
        public static string umbracoUserContextID {
            get {
                if (System.Web.HttpContext.Current.Request.Cookies.Get("UserContext") != null)
                    return System.Web.HttpContext.Current.Request.Cookies.Get("UserContext").Value;
                else
                    return "";
            }
            set {
                // Clearing all old cookies before setting a new one.
                try {
                    if (System.Web.HttpContext.Current.Request.Cookies["UserContext"] != null) {
                        System.Web.HttpContext.Current.Response.Cookies.Clear();
                    }
                } catch {
                }
                // Create new cookie.
                System.Web.HttpCookie c = new System.Web.HttpCookie("UserContext");
                c.Name = "UserContext";
                c.Value = value;
                c.Expires = DateTime.Now.AddDays(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(c);
            }
        }


        /// <summary>
        /// Clears the login.
        /// </summary>
        public void ClearLogin() {
            umbracoUserContextID = "";
        }

        private void updateLogin() {
            // only call update if more than 1/10 of the timeout has passed
            if (timeout - (((_ticksPrMinute * _umbracoTimeOutInMinutes) * 0.8)) < DateTime.Now.Ticks)
                SqlHelper.ExecuteNonQuery(
                    "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                    SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (_ticksPrMinute * _umbracoTimeOutInMinutes)),
                    SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
        }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="u">The user</param>
        public static void doLogin(User u) {
            Guid retVal = Guid.NewGuid();
            SqlHelper.ExecuteNonQuery(
                                      "insert into umbracoUserLogins (contextID, userID, timeout) values (@contextId,'" + u.Id + "','" +
                                      (DateTime.Now.Ticks + (_ticksPrMinute * _umbracoTimeOutInMinutes)).ToString() +
                                      "') ",
                                      SqlHelper.CreateParameter("@contextId", retVal));
            umbracoUserContextID = retVal.ToString();
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Login, u, -1, "");
        }


        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns></returns>
        public User getUser() {
            if (!_userisValidated) validateUser();
            return _user;
        }

        /// <summary>
        /// Ensures the page context.
        /// </summary>
        public void ensureContext() {
            validateUser();
        }
        
		[Obsolete("Use ClientTools instead")]
        public void speechBubble(speechBubbleIcon i, string header, string body) 
		{
			ClientTools.ShowSpeechBubble(i, header, body);
        }

		//[Obsolete("Use ClientTools instead")]
		//public void reloadParentNode() 
		//{
		//    ClientTools.ReloadParentNode(true);
		//}

        /// <summary>
        /// a collection of available speechbubble icons
        /// </summary>
        public enum speechBubbleIcon {
            /// <summary>
            /// Save icon
            /// </summary>
            save,
            /// <summary>
            /// Info icon
            /// </summary>
            info,
            /// <summary>
            /// Error icon
            /// </summary>
            error,
            /// <summary>
            /// Success icon
            /// </summary>
            success,
            /// <summary>
            /// Warning icon
            /// </summary>
            warning
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            if (OverrideClientTarget)
                ClientTarget = "uplevel";

            if (!Request.IsSecureConnection && GlobalSettings.UseSSL) {
                string serverName = HttpUtility.UrlEncode(Request.ServerVariables["SERVER_NAME"]);
                Response.Redirect(string.Format("https://{0}{1}", serverName, Request.FilePath));
            }
        }

        /// <summary>
        /// Override client target.
        /// </summary>
        public bool OverrideClientTarget = true;
    }
}
