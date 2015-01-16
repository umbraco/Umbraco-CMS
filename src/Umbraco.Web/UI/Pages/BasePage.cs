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
using Umbraco.Core.Profiling;
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
        
        private ClientTools _clientTools;
        

        //We won't expose this... people should be using the DatabaseContext for custom queries if they need them.

        ///// <summary>
        ///// Gets the SQL helper.
        ///// </summary>
        ///// <value>The SQL helper.</value>
        //protected ISqlHelper SqlHelper
        //{
        //    get { return global::umbraco.BusinessLogic.Application.SqlHelper; }
        //}

        /// <summary>
        /// Returns an ILogger
        /// </summary>
        public ILogger Logger
        {
            get { return ProfilingLogger.Logger; }
        }

        /// <summary>
        /// Returns a ProfilingLogger
        /// </summary>
        public ProfilingLogger ProfilingLogger
        {
            get { return _logger ?? (_logger = new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler)); }
        }

        private ProfilingLogger _logger;
        

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

        private HtmlHelper _html;
        /// <summary>
        /// Returns a HtmlHelper
        /// </summary>        
        /// <remarks>
        /// This html helper is created with an empty context and page so it may not have all of the functionality expected.
        /// </remarks>
        public HtmlHelper Html
        {
            get { return _html ?? (_html = new HtmlHelper(new ViewContext(), new ViewPage())); }
        }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get { return UmbracoContext.Current; }
        }

        /// <summary>
        /// Returns the current WebSecurity instance
        /// </summary>
        public WebSecurity Security
        {
            get { return UmbracoContext.Security; }
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
