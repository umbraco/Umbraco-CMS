using System;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;

namespace Umbraco.Web.UI.Pages
{
    /// <summary>
    /// UmbracoEnsuredPage is the standard protected page in the umbraco backend, and forces authentication.
    /// </summary>
    public class UmbracoEnsuredPage : BasePage
    {
        public string CurrentApp { get; set; }

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
            return UmbracoUser.Applications.Any(uApp => uApp.alias == app);
        }

        /// <summary>
        /// Validates the user node tree permissions.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool ValidateUserNodeTreePermissions(string path, string action)
        {
            string permissions = UmbracoUser.GetPermissions(path);
            if (permissions.IndexOf(action) > -1 && (path.Contains("-20") || ("," + path + ",").Contains("," + UmbracoUser.StartNodeId.ToString() + ",")))
                return true;

	        var user = UmbracoUser;
	        LogHelper.Info<UmbracoEnsuredPage>("User {0} has insufficient permissions in UmbracoEnsuredPage: '{1}', '{2}', '{3}'", () => user.Name, () => path, () => permissions, () => action);
            return false;
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
                EnsureContext();

                if (!String.IsNullOrEmpty(CurrentApp))
                {
                    if (!ValidateUserApp(CurrentApp))
                        throw new UserAuthorizationException(String.Format("The current user doesn't have access to the section/app '{0}'", CurrentApp));
                }
            }
            catch (UserAuthorizationException ex)
            {
				LogHelper.Error<UmbracoEnsuredPage>(String.Format("Tried to access '{0}'", CurrentApp), ex);
                throw;
            }
            catch
            {
                // Clear content as .NET transfers rendered content.
                Response.Clear();

                // Some umbraco pages should not be loaded on timeout, but instead reload the main application in the top window. Like the treeview for instance
                if (RedirectToUmbraco)
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?", true);
                else
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl), true);
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(ui.Culture(this.UmbracoUser));
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        }
    }
}