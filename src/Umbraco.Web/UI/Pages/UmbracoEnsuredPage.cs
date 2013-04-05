using System;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Security;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using Umbraco.Core;

namespace Umbraco.Web.UI.Pages
{
    /// <summary>
    /// UmbracoEnsuredPage is the standard protected page in the umbraco backend, and forces authentication.
    /// </summary>
    public class UmbracoEnsuredPage : BasePage
    {
        private bool _hasValidated = false;

        /// <summary>
        /// Authorizes the user
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            try
            {
                ValidateUser();

                if (!ValidateUserApp(CurrentApp))
                {
                    var ex = new UserAuthorizationException(String.Format("The current user doesn't have access to the section/app '{0}'", CurrentApp));
                    LogHelper.Error<UmbracoEnsuredPage>(String.Format("Tried to access '{0}'", CurrentApp), ex);
                    throw ex;
                }

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
        }
       
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(ui.Culture(Security.CurrentUser));
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
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
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <returns></returns>
        private bool ValidateUserApp(string app)
        {
            //if it is empty, don't validate
            if (app.IsNullOrWhiteSpace())
            {
                return true;
            }
            return Security.CurrentUser.Applications.Any(uApp => uApp.alias == app);
        }

        private void ValidateUser()
        {
            //validate the current user, if failed then throw exceptions
            var attempt = Security.ValidateCurrentUser(new HttpContextWrapper(Context));
            _hasValidated = true;
            switch (attempt)
            {
                case ValidateUserAttempt.FailedNoPrivileges:
                    throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                case ValidateUserAttempt.FailedTimedOut:
                    throw new ArgumentException("User has timed out!!");
                case ValidateUserAttempt.FailedNoContextId:
                    throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
            }            
        }
        
        /// <summary>
        /// Returns the current user
        /// </summary>
        protected User UmbracoUser
        {
            get
            {
                if (!_hasValidated) ValidateUser();
                return Security.CurrentUser;
            }
        }
        
    }
}