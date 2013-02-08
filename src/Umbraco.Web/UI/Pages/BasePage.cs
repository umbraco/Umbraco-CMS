using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Web.UI;

namespace Umbraco.Web.UI.Pages
{
    /// <summary>
    /// umbraco.BasePages.BasePage is the default page type for the umbraco backend.
    /// The basepage keeps track of the current user and the page context. But does not 
    /// Restrict access to the page itself.
    /// The keep the page secure, the umbracoEnsuredPage class should be used instead
    /// </summary>
    public class BasePage : Page
    {
        private User _user;
        private bool _userisValidated = false;
        private ClientTools _clientTools;
        
        /// <summary>
        /// The current user ID
        /// </summary>
        private int _uid = 0;

        /// <summary>
        /// The page timeout in seconds.
        /// </summary>
        private long _timeout = 0;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return global::umbraco.BusinessLogic.Application.SqlHelper; }
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
        /// Returns a refernce of an instance of ClientTools for access to the pages client API
        /// </summary>
        public ClientTools ClientTools
        {
            get { return _clientTools ?? (_clientTools = new ClientTools(this)); }
        }
        
        private void ValidateUser()
        {
            if ((WebSecurity.UmbracoUserContextId != ""))
            {
                _uid = WebSecurity.GetUserId(WebSecurity.UmbracoUserContextId);
                _timeout = WebSecurity.GetTimeout(WebSecurity.UmbracoUserContextId);

                if (_timeout > DateTime.Now.Ticks)
                {
                    _user = global::umbraco.BusinessLogic.User.GetUser(_uid);

                    // Check for console access
                    if (_user.Disabled || (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(Context) && !GlobalSettings.RequestIsLiveEditRedirector(Context)))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    }
                    _userisValidated = true;
                    WebSecurity.UpdateLogin(_timeout);
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
        public void EnsureContext()
        {
            ValidateUser();
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

    }
}
