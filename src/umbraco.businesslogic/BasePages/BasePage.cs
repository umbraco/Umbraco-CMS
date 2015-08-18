using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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

        //NOTE: This is basically replicated in WebSecurity because this class exists in a poorly placed assembly. - also why it is obsolete.
        private void ValidateUser()
        {
            var ticket = Context.GetUmbracoAuthTicket();

            if (ticket != null)
            {
                if (ticket.Expired == false)
                {
                    _user = BusinessLogic.User.GetUser(GetUserId(""));

                    // Check for console access
                    if (_user.Disabled || (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(Context)))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    }
                    _userisValidated = true;
                    UpdateLogin();
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
        /// <param name="umbracoUserContextID">This is not used</param>
        /// <returns></returns>
        [Obsolete("This method is no longer used, use the GetUserId() method without parameters instead")]
        public static int GetUserId(string umbracoUserContextID)
        {
            return GetUserId();
        }

        /// <summary>
        /// Gets the currnet user's id.
        /// </summary>
        /// <returns></returns>
        public static int GetUserId()
        {
            var identity = HttpContext.Current.GetCurrentIdentity(
                //DO NOT AUTO-AUTH UNLESS THE CURRENT HANDLER IS WEBFORMS!
                // Without this check, anything that is using this legacy API, like ui.Text will
                // automatically log the back office user in even if it is a front-end request (if there is 
                // a back office user logged in. This can cause problems becaues the identity is changing mid
                // request. For example: http://issues.umbraco.org/issue/U4-4010
                HttpContext.Current.CurrentHandler is Page);

            if (identity == null)
                return -1;
            return Convert.ToInt32(identity.Id);
        }

        // Added by NH to use with webservices authentications
        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="currentUmbracoUserContextID">This doesn't do anything</param>
        /// <returns></returns>
        [Obsolete("This method is no longer used, use the ValidateCurrentUser() method instead")]
        public static bool ValidateUserContextID(string currentUmbracoUserContextID)
        {
            return ValidateCurrentUser();
        }

        /// <summary>
        /// Validates the currently logged in user and ensures they are not timed out
        /// </summary>
        /// <returns></returns>
        public static bool ValidateCurrentUser()
        {
            var identity = HttpContext.Current.GetCurrentIdentity(
                //DO NOT AUTO-AUTH UNLESS THE CURRENT HANDLER IS WEBFORMS!
                // Without this check, anything that is using this legacy API, like ui.Text will
                // automatically log the back office user in even if it is a front-end request (if there is 
                // a back office user logged in. This can cause problems becaues the identity is changing mid
                // request. For example: http://issues.umbraco.org/issue/U4-4010
                HttpContext.Current.CurrentHandler is Page);

            if (identity != null)
            {
                return true;
            }
            return false;
        }

        //[Obsolete("Use Umbraco.Web.Security.WebSecurity.GetTimeout instead")]
        public static long GetTimeout(bool bypassCache)
        {
            var ticket = HttpContext.Current.GetUmbracoAuthTicket();
            if (ticket.Expired) return 0;
            var ticks = ticket.Expiration.Ticks - DateTime.Now.Ticks;         
            return ticks;
        }

        // Changed to public by NH to help with webservice authentication
        /// <summary>
        /// Gets or sets the umbraco user context ID.
        /// </summary>
        /// <value>The umbraco user context ID.</value>
        [Obsolete("Returns the current user's unique umbraco sesion id - this cannot be set and isn't intended to be used in your code")]
        public static string umbracoUserContextID
        {
            get
            {
                var identity = HttpContext.Current.GetCurrentIdentity(
                    //DO NOT AUTO-AUTH UNLESS THE CURRENT HANDLER IS WEBFORMS!
                    // Without this check, anything that is using this legacy API, like ui.Text will
                    // automatically log the back office user in even if it is a front-end request (if there is 
                    // a back office user logged in. This can cause problems becaues the identity is changing mid
                    // request. For example: http://issues.umbraco.org/issue/U4-4010
                    HttpContext.Current.CurrentHandler is Page);

                return identity == null ? "" : identity.SessionId;
            }
            set
            {
            }
        }


        /// <summary>
        /// Clears the login.
        /// </summary>
        public void ClearLogin()
        {
            Context.UmbracoLogout();
        }

        private void UpdateLogin()
        {
            Context.RenewUmbracoAuthTicket();
        }

        public static void RenewLoginTimeout()
        {
            HttpContext.Current.RenewUmbracoAuthTicket();           
        }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="u">The user</param>
        public static void doLogin(User u)
        {
            HttpContext.Current.CreateUmbracoAuthTicket(new UserData(Guid.NewGuid().ToString("N"))
            {
                Id = u.Id,
                AllowedApplications = u.GetApplications().Select(x => x.alias).ToArray(),
                RealName = u.Name,
                //currently we only have one user type!
                Roles = new[] { u.UserType.Alias },
                StartContentNode = u.StartNodeId,
                StartMediaNode = u.StartMediaId,
                Username = u.LoginName,
                Culture = ui.Culture(u)

            });
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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //This must be set on each page to mitigate CSRF attacks which ensures that this unique token 
            // is added to the viewstate of each request
            if (umbracoUserContextID.IsNullOrWhiteSpace() == false)
            {
                ViewStateUserKey = umbracoUserContextID;
            }
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
