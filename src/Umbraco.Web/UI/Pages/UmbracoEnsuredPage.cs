using System;
using System.Security;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Composing;

namespace Umbraco.Web.UI.Pages
{
    /// <summary>
    /// UmbracoEnsuredPage is the standard protected page in the umbraco backend, and forces authentication.
    /// </summary>
    public class UmbracoEnsuredPage : BasePage
    {
        public UmbracoEnsuredPage()
        {
            //Assign security automatically if the attribute is found
            var treeAuth = this.GetType().GetCustomAttribute<WebformsPageTreeAuthorizeAttribute>(true);
            if (treeAuth != null)
            {
                var treeByAlias = Current.Services.ApplicationTreeService
                    .GetByAlias(treeAuth.TreeAlias);
                if (treeByAlias != null)
                {
                    CurrentApp = treeByAlias.ApplicationAlias;
                }
            }
        }

        private bool _hasValidated = false;

        /// <summary>
        /// Authorizes the user
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Checks if the page exists outside of the /umbraco route, in which case the request will not have been authenticated for the back office 
        /// so we'll force authentication.
        /// </remarks>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            //If this is not a back office request, then the module won't have authenticated it, in this case we
            // need to do the auth manually and since this is an UmbracoEnsuredPage, this is the anticipated behavior
            // TODO: When we implement Identity, this process might not work anymore, will be an interesting challenge
            if (Context.Request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath) == false)
            {
                var http = new HttpContextWrapper(Context);
                var ticket = http.GetUmbracoAuthTicket();
                http.AuthenticateCurrentRequest(ticket, true);
            }

            try
            {
                Security.ValidateCurrentUser(true);
                _hasValidated = true;

                if (!Security.ValidateUserApp(CurrentApp))
                {
                    var ex = new SecurityException(String.Format("The current user doesn't have access to the section/app '{0}'", CurrentApp));
                    Current.Logger.Error<UmbracoEnsuredPage>(String.Format("Tried to access '{0}'", CurrentApp), ex);
                    throw ex;
                }

            }
            catch
            {
                // Clear content as .NET transfers rendered content.
                Response.Clear();

                // Some umbraco pages should not be loaded on timeout, but instead reload the main application in the top window. Like the treeview for instance
                if (RedirectToUmbraco)
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?t=" + Security.GetSessionId(), true);
                else
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl) + "&t=" + Security.GetSessionId(), true);
            }
        }

        /// <summary>
        /// Gets/sets the app that this page is assigned to
        /// </summary>
        protected string CurrentApp { get; set; }

        /// <summary>
        /// If true then umbraco will force any window/frame to reload umbraco in the main window
        /// </summary>
        protected bool RedirectToUmbraco { get; set; }
      
        /// <summary>
        /// Used to assign a webforms page's security to a specific tree which will in turn check to see
        /// if the current user has access to the specified tree's registered section
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public sealed class WebformsPageTreeAuthorizeAttribute : Attribute
        {
            public string TreeAlias { get; private set; }

            public WebformsPageTreeAuthorizeAttribute(string treeAlias)
            {
                TreeAlias = treeAlias;
            }
        }
    }
}