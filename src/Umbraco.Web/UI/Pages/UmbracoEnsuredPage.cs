using System;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Security;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;


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

        /// <summary>
        /// Performs an authorization check for the user against the requested entity/path and permission set, this is only relevant to content and media
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="objectType"></param>
        /// <param name="actionToCheck"></param>
        protected void CheckPathAndPermissions(int entityId, UmbracoObjectTypes objectType, IAction actionToCheck)
        {
            if (objectType != UmbracoObjectTypes.Document && objectType != UmbracoObjectTypes.Media)
                return;

            //check path access

            var entity = entityId == Constants.System.Root
                ? EntitySlim.Root
                : Services.EntityService.Get(entityId, objectType);
            var hasAccess = objectType == UmbracoObjectTypes.Document
                ? Security.CurrentUser.HasContentPathAccess(entity, Services.EntityService)
                : Security.CurrentUser.HasMediaPathAccess(entity, Services.EntityService);
            if (hasAccess == false)
                throw new AuthorizationException($"The current user doesn't have access to the path '{entity.Path}'");

            //only documents have action permissions
            if (objectType == UmbracoObjectTypes.Document)
            {
                var allActions = Current.Actions;
                var perms = Security.CurrentUser.GetPermissions(entity.Path, Services.UserService);
                var actions = perms
                    .Select(x => allActions.FirstOrDefault(y => y.Letter.ToString(CultureInfo.InvariantCulture) == x))
                    .WhereNotNull();
                if (actions.Contains(actionToCheck) == false)
                    throw new AuthorizationException($"The current user doesn't have permission to {actionToCheck.Alias} on the path '{entity.Path}'");
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
            if (Context.Request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath, UmbracoConfig.For.GlobalSettings()) == false)
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
                    Current.Logger.Error<UmbracoEnsuredPage>(ex, "Tried to access '{CurrentApp}'", CurrentApp);
                    throw ex;
                }

            }
            catch
            {
                // Clear content as .NET transfers rendered content.
                Response.Clear();

                // Ensure the person is definitely logged out
                UmbracoContext.Current.Security.ClearCurrentLogin();

                // Redirect to the login page
                Response.Redirect(SystemDirectories.Umbraco + "#/login", true);
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
