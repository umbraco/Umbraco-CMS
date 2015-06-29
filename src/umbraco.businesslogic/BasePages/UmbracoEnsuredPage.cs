using System;
using Umbraco.Core.Logging;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using Umbraco.Core.Security;

namespace umbraco.BasePages
{
    /// <summary>
    /// UmbracoEnsuredPage is the standard protected page in the umbraco backend, and forces authentication.
    /// </summary>
    [Obsolete("This class has been superceded by Umbraco.Web.UI.Pages.UmbracoEnsuredPage")]
    public class UmbracoEnsuredPage : BasePage
    {
        /// <summary>
        /// Checks if the page exists outside of the /umbraco route, in which case the request will not have been authenticated for the back office 
        /// so we'll force authentication.
        /// </summary>
        /// <param name="e"></param>
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
        }

        /// <summary>
        /// Gets/sets the app for which this page belongs to so that we can validate the current user's security against it
        /// </summary>
        /// <remarks>
        /// If no app is specified then all logged in users will have access to the page
        /// </remarks>
        public string CurrentApp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoEnsuredPage"/> class.
        /// </summary>
        public UmbracoEnsuredPage()
        {
            //Assign security automatically if the attribute is found
            var treeAuth = this.GetType().GetCustomAttribute<WebformsPageTreeAuthorizeAttribute>(true);
            if (treeAuth != null)
            {
                var treeByAlias = ApplicationContext.Current.Services.ApplicationTreeService
                    .GetByAlias(treeAuth.TreeAlias);
                if (treeByAlias != null)
                {
                    CurrentApp = treeByAlias.ApplicationAlias;    
                }
            }
        }

        [Obsolete("This constructor is not used and will be removed from the codebase in the future")]
        public UmbracoEnsuredPage(string hest) : this()
        {
            
        }

        /// <summary>
        /// If true then umbraco will force any window/frame to reload umbraco in the main window
        /// </summary>
        public bool RedirectToUmbraco { get; set; }

        /// <summary>
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <returns></returns>
        public bool ValidateUserApp(string app)
        {
            return getUser().Applications.Any(uApp => uApp.alias.InvariantEquals(app));
        }

        /// <summary>
        /// Validates the user node tree permissions.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="Action">The action.</param>
        /// <returns></returns>
        public bool ValidateUserNodeTreePermissions(string Path, string Action)
        {
            string permissions = getUser().GetPermissions(Path);
            if (permissions.IndexOf(Action) > -1 && (Path.Contains("-20") || ("," + Path + ",").Contains("," + getUser().StartNodeId.ToString() + ",")))
                return true;

	        var user = getUser();
	        LogHelper.Info<UmbracoEnsuredPage>("User {0} has insufficient permissions in UmbracoEnsuredPage: '{1}', '{2}', '{3}'", () => user.Name, () => Path, () => permissions, () => Action);
            return false;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public static User CurrentUser
        {
            get
            {
                return BusinessLogic.User.GetCurrent();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"></see> event to initialize the page.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                ensureContext();

                if (!string.IsNullOrEmpty(CurrentApp))
                {
                    if (!ValidateUserApp(CurrentApp))
                        throw new UserAuthorizationException(String.Format("The current user doesn't have access to the section/app '{0}'", CurrentApp));
                }
            }
            catch (UserAuthorizationException ex)
            {
                LogHelper.Warn<UmbracoEnsuredPage>(string.Format("{0} tried to access '{1}'", CurrentUser.Id, CurrentApp));
                throw;
            }
            catch
            {
                // Clear content as .NET transfers rendered content.
                Response.Clear();

                // Some umbraco pages should not be loaded on timeout, but instead reload the main application in the top window. Like the treeview for instance
                if (RedirectToUmbraco)
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?t=" + umbracoUserContextID, true);
                else
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl) + "&t=" + umbracoUserContextID, true);
            }
        }

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