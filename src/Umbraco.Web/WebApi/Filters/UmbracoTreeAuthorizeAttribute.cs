using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections
    /// </remarks>
    public sealed class UmbracoTreeAuthorizeAttribute : OverridableAuthorizationAttribute
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly string[] _treeAliases;

        /// <summary>
        /// Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// </summary>
        /// <param name="treeAliases">
        /// If the user has access to the application that the treeAlias is specified in, they will be authorized.
        /// Multiple trees may be specified.
        /// </param>
        public UmbracoTreeAuthorizeAttribute(params string[] treeAliases)
        {
            _treeAliases = treeAliases;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            var apps = _treeAliases.Select(x => ApplicationContext.Current.Services.ApplicationTreeService
                .GetByAlias(x))
                .WhereNotNull()
                .Select(x => x.ApplicationAlias)
                .Distinct()
                .ToArray();

            return UmbracoContext.Current.Security.CurrentUser != null
                   && apps.Any(app => UmbracoContext.Current.Security.UserHasAppAccess(
                       app, UmbracoContext.Current.Security.CurrentUser));
        }
    }
}