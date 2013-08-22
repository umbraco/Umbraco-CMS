using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace umbraco.BasePages
{
    /// <summary>
    /// umbraco.BasePages.BasePage is the default page type for the umbraco backend.
    /// The basepage keeps track of the current user and the page context. But does not 
    /// Restrict access to the page itself.
    /// The keep the page secure, the umbracoEnsuredPage class should be used instead
    /// </summary>
    [Obsolete("This class has been superceded by Umbraco.Web.UI.Pages.BasePage")]
    public class BasePage : System.Web.UI.Page
    {
        private User _user;
        private bool _userisValidated = false;
        private ClientTools _clientTools;

        // ticks per minute 600,000,000 
        private const long TicksPrMinute = 600000000;
        private static readonly int UmbracoTimeOutInMinutes = GlobalSettings.TimeOutInMinutes;

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
        protected static ISqlHelper SqlHelper
        {
            get { return BusinessLogic.Application.SqlHelper; }
        }        

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get { return ApplicationContext.Services; }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get { return ApplicationContext.DatabaseContext; }
        }

        /// <summary>
        /// Returns the current BasePage for the current request. 
        /// This assumes that the current page is a BasePage, otherwise, returns null;
        /// </summary>
        [Obsolete("Should use the Umbraco.Web.UmbracoContext.Current singleton instead to access common methods and properties")]
        public static BasePage Current
        {
            get
            {
                var page = HttpContext.Current.CurrentHandler as BasePage;
                if (page != null) return page;
                //the current handler is not BasePage but people might be expecting this to be the case if they 
                // are using this singleton accesor... which is legacy code now and shouldn't be used. When people
                // start using Umbraco.Web.UI.Pages.BasePage then this will not be the case! So, we'll just return a 
                // new instance of BasePage as a hack to make it work.
                if (HttpContext.Current.Items["umbraco.BasePages.BasePage"] == null)
                {
                    HttpContext.Current.Items["umbraco.BasePages.BasePage"] = new BasePage();
                }
                return (BasePage)HttpContext.Current.Items["umbraco.BasePages.BasePage"];
            }
        }

	    private UrlHelper _url;
		/// <summary>
		/// Returns a UrlHelper
		/// </summary>
		/// <remarks>
		/// This URL helper is created without any route data and an empty request context
		/// </remarks>
	    public UrlHelper Url
	    {
		    get { return _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()))); }
	    }

        /// <summary>
        /// Returns a refernce of an instance of ClientTools for access to the pages client API
        /// </summary>
        public ClientTools ClientTools
        {
            get
            {
                if (_clientTools == null)
                    _clientTools = new ClientTools(this);
                return _clientTools;
            }
        }

        [Obsolete("Use ClientTools instead")]
        public void RefreshPage(int Seconds)
        {
            ClientTools.RefreshAdmin(Seconds);
        }

        private void ValidateUser()
        {
            if ((umbracoUserContextID != ""))
            {
                uid = GetUserId(umbracoUserContextID);
                timeout = GetTimeout(umbracoUserContextID);

                if (timeout > DateTime.Now.Ticks)
                {
                    _user = BusinessLogic.User.GetUser(uid);

                    // Check for console access
                    if (_user.Disabled || (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(HttpContext.Current) && !GlobalSettings.RequestIsLiveEditRedirector(HttpContext.Current)))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    }
                    else
                    {
                        _userisValidated = true;
                        UpdateLogin();
                    }

                }
                else
                {
                    throw new ArgumentException("User has timed out!!");
                }
            }
            else
            {
                throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
            }

        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="umbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.GetUserId instead")]
        public static int GetUserId(string umbracoUserContextID)
        {
            //need to parse to guid
            Guid gid;
            if (!Guid.TryParse(umbracoUserContextID, out gid))
            {
                return -1;
            }

            var id = ApplicationContext.Current.ApplicationCache.GetCacheItem<int?>(
                CacheKeys.UserContextCacheKey + umbracoUserContextID,
                new TimeSpan(0, UmbracoTimeOutInMinutes / 10, 0),
                () => SqlHelper.ExecuteScalar<int?>(
                    "select userID from umbracoUserLogins where contextID = @contextId",
                    SqlHelper.CreateParameter("@contextId", gid)));
            if (id == null)
                return -1;
            return id.Value;    
        }


        // Added by NH to use with webservices authentications
        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="currentUmbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.ValidateUserContextId instead")]
        public static bool ValidateUserContextID(string currentUmbracoUserContextID)
        {
            if (!currentUmbracoUserContextID.IsNullOrWhiteSpace())
            {
                var uid = GetUserId(currentUmbracoUserContextID);
                var timeout = GetTimeout(currentUmbracoUserContextID);

                if (timeout > DateTime.Now.Ticks)
                {
                    return true;
                }
	            var user = BusinessLogic.User.GetUser(uid);
                //TODO: We don't actually log anyone out here, not sure why we're logging ??
				LogHelper.Info<BasePage>("User {0} (Id:{1}) logged out", () => user.Name, () => user.Id);
            }
            return false;
        }

        internal static long GetTimeout(string umbracoUserContextId)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                CacheKeys.UserContextTimeoutCacheKey + umbracoUserContextId,
                new TimeSpan(0, UmbracoTimeOutInMinutes / 10, 0),
                () => GetTimeout(true));
        }

        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.GetTimeout instead")]
        public static long GetTimeout(bool bypassCache)
        {
            if (UmbracoSettings.KeepUserLoggedIn)
                RenewLoginTimeout();

            if (bypassCache)
            {
                return SqlHelper.ExecuteScalar<long>("select timeout from umbracoUserLogins where contextId=@contextId",
                                                          SqlHelper.CreateParameter("@contextId", new Guid(umbracoUserContextID))
                                        );
            }
            else
                return GetTimeout(umbracoUserContextID);
        }

        // Changed to public by NH to help with webservice authentication
        /// <summary>
        /// Gets or sets the umbraco user context ID.
        /// </summary>
        /// <value>The umbraco user context ID.</value>
        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.UmbracoUserContextId instead")]
        public static string umbracoUserContextID
        {
            get
            {
                var authTicket = HttpContext.Current.GetUmbracoAuthTicket();
                if (authTicket == null)
                {
                    return "";
                }
                var identity = authTicket.CreateUmbracoIdentity();
                if (identity == null)
                {
                    HttpContext.Current.UmbracoLogout();
                    return "";
                }
                return identity.UserContextId;
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                {
                    HttpContext.Current.UmbracoLogout();
                }
                else
                {
                    var uid = GetUserId(value);
                    if (uid == -1)
                    {
                        HttpContext.Current.UmbracoLogout();
                    }
                    else
                    {
                        var user = BusinessLogic.User.GetUser(uid);
                        HttpContext.Current.CreateUmbracoAuthTicket(
                            new UserData
                                {
                                    Id = uid,
                                    AllowedApplications = user.Applications.Select(x => x.alias).ToArray(),
                                    Culture = ui.Culture(user),
                                    RealName = user.Name,
                                    Roles = new string[] {user.UserType.Alias},
                                    StartContentNode = user.StartNodeId,
                                    StartMediaNode = user.StartMediaId,
                                    UserContextId = value,
                                    Username = user.LoginName
                                });
                    }
                }
            }
        }


        /// <summary>
        /// Clears the login.
        /// </summary>
        public void ClearLogin()
        {
            DeleteLogin();
            umbracoUserContextID = "";
        }

        private void DeleteLogin()
        {
            // Added try-catch in case login doesn't exist in the database
            // Either due to old cookie or running multiple sessions on localhost with different port number
            try
            {
                SqlHelper.ExecuteNonQuery(
                "DELETE FROM umbracoUserLogins WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
            }
            catch (Exception ex)
            {
                LogHelper.Error<BasePage>(string.Format("Login with contextId {0} didn't exist in the database", umbracoUserContextID), ex);
            }
        }

        private void UpdateLogin()
        {
            // only call update if more than 1/10 of the timeout has passed
            if (timeout - (((TicksPrMinute * UmbracoTimeOutInMinutes) * 0.8)) < DateTime.Now.Ticks)
                SqlHelper.ExecuteNonQuery(
                    "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                    SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                    SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
        }

        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.RenewLoginTimeout instead")]
        public static void RenewLoginTimeout()
        {
            // only call update if more than 1/10 of the timeout has passed
            SqlHelper.ExecuteNonQuery(
                "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
        }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="u">The user</param>
        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.PerformLogin instead")]
        public static void doLogin(User u)
        {
            Guid retVal = Guid.NewGuid();
            SqlHelper.ExecuteNonQuery(
                                      "insert into umbracoUserLogins (contextID, userID, timeout) values (@contextId,'" + u.Id + "','" +
                                      (DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)).ToString() +
                                      "') ",
                                      SqlHelper.CreateParameter("@contextId", retVal));
            umbracoUserContextID = retVal.ToString();

			LogHelper.Info<BasePage>("User {0} (Id: {1}) logged in", () => u.Name, () => u.Id);
        }


        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use UmbracoUser property instead.")]
        public User getUser()
        {
            return UmbracoUser;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <value></value>
        public User UmbracoUser
        {
            get
            {
                if (!_userisValidated) ValidateUser();
                return _user;
            }
        }

        /// <summary>
        /// Ensures the page context.
        /// </summary>
        public void ensureContext()
        {
            ValidateUser();
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
        [Obsolete("This has been superceded by Umbraco.Web.UI.SpeechBubbleIcon but that requires the use of the Umbraco.Web.UI.Pages.BasePage or Umbraco.Web.UI.Pages.EnsuredPage objects")]
        public enum speechBubbleIcon
        {
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
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Request.IsSecureConnection && GlobalSettings.UseSSL)
            {
                string serverName = HttpUtility.UrlEncode(Request.ServerVariables["SERVER_NAME"]);
                Response.Redirect(string.Format("https://{0}{1}", serverName, Request.FilePath));
            }
        }

        /// <summary>
        /// Override client target.
        /// </summary>
        [Obsolete("This is no longer supported")]
        public bool OverrideClientTarget = false;
    }
}
