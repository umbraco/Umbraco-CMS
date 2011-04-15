using System;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.IO;

namespace umbraco.BasePages
{
    /// <summary>
    /// UmbracoEnsuredPage is the standard protected page in the umbraco backend, and forces authentication.
    /// </summary>
    public class UmbracoEnsuredPage : BasePage
    {
        public string CurrentApp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoEnsuredPage"/> class.
        /// </summary>
        public UmbracoEnsuredPage()
        {

        }

        public UmbracoEnsuredPage(string hest)
        {
            
        }

        private bool _redirectToUmbraco;
        /// <summary>
        /// If true then umbraco will force any window/frame to reload umbraco in the main window
        /// </summary>
        public bool RedirectToUmbraco
        {
            get { return _redirectToUmbraco; }
            set { _redirectToUmbraco = value; }
        }

        /// <summary>
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <returns></returns>
        public bool ValidateUserApp(string app)
        {

            foreach (Application uApp in getUser().Applications)
                if (uApp.alias == app)
                    return true;
            return false;
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

            Log.Add(LogTypes.LoginFailure, getUser(), -1, "Insufient permissions in UmbracoEnsuredPage: '" + Path + "', '" + permissions + "', '" + Action + "'");
            return false;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public static BusinessLogic.User CurrentUser
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

                if (!String.IsNullOrEmpty(CurrentApp))
                {
                    if (!ValidateUserApp(CurrentApp))
                        throw new UserAuthorizationException(String.Format("The current user doesn't have access to the section/app '{0}'", CurrentApp));
                }
            }
            catch (UserAuthorizationException ex)
            {
                Log.Add(LogTypes.Error, CurrentUser, -1, String.Format("Tried to access '{0}'", CurrentApp));
                throw;
            }
            catch
            {
                // Some umbraco pages should not be loaded on timeout, but instead reload the main application in the top window. Like the treeview for instance
                if (RedirectToUmbraco)
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?");
                else
                    Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl));
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(ui.Culture(this.getUser()));
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        }
    }
}