using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using System.Web.UI;
using Umbraco.Web.Composing;

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
        private UrlHelper _url;
        private HtmlHelper _html;
        private ClientTools _clientTools;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger => Current.Logger;

        /// <summary>
        /// Gets the profiling helper.
        /// </summary>
        public IProfilingLogger ProfilingLogger => Current.ProfilingLogger;

        /// <summary>
        /// Gets the Url helper.
        /// </summary>
        /// <remarks>This URL helper is created without any route data and an empty request context.</remarks>
        public UrlHelper Url => _url ?? (_url = new UrlHelper(Context.Request.RequestContext));

        /// <summary>
        /// Gets the Html helper.
        /// </summary>
        /// <remarks>This html helper is created with an empty context and page so it may not have all of the functionality expected.</remarks>
        public HtmlHelper Html => _html ?? (_html = new HtmlHelper(new ViewContext(), new ViewPage()));

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext => Current.UmbracoContext;

        /// <summary>
        /// Gets the web security helper.
        /// </summary>
        public WebSecurity Security => UmbracoContext.Security;

        /// <summary>
        /// Gets the services context.
        /// </summary>
        public ServiceContext Services => Current.Services;

        /// <summary>
        /// Gets an instance of ClientTools for access to the pages client API.
        /// </summary>
        public ClientTools ClientTools => _clientTools ?? (_clientTools = new ClientTools(this));

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Request.IsSecureConnection || UmbracoConfig.For.GlobalSettings().UseHttps == false) return;

            var serverName = HttpUtility.UrlEncode(Request.ServerVariables["SERVER_NAME"]);
            Response.Redirect($"https://{serverName}{Request.FilePath}");
        }
    }
}
