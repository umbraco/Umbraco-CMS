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
                Security.ValidateCurrentUser(true);
                _hasValidated = true;

                if (!Security.ValidateUserApp(CurrentApp))
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
        /// Returns the current user
        /// </summary>
        [Obsolete("This should no longer be used since it returns the legacy user object, use The Security.CurrentUser instead to return the proper user object")]
        protected User UmbracoUser
        {
            get
            {
                //throw exceptions if not valid (true)
                if (!_hasValidated)
                {
                    Security.ValidateCurrentUser(true);
                    _hasValidated = true;
                }
                
                return new User(Security.CurrentUser);
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