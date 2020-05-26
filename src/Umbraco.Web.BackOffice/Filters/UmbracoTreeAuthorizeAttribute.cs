using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Web.Services;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections
    /// </remarks>
    public sealed class UmbracoTreeAuthorizeAttribute : IAuthorizationFilter
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly ITreeService _treeService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly string[] _treeAliases;

        /// <summary>
        /// Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// </summary>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="treeAliases">
        /// If the user has access to the application that the treeAlias is specified in, they will be authorized.
        /// Multiple trees may be specified.
        /// </param>
        /// <param name="treeService"></param>
        public UmbracoTreeAuthorizeAttribute(ITreeService treeService, IUmbracoContextAccessor umbracoContextAccessor, params string[] treeAliases)
        {
            _treeService = treeService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _treeAliases = treeAliases;
        }

        private bool IsAuthorized()
        {
            if (Enable == false)
            {
                return true;
            }

            var apps = _treeAliases.Select(x => _treeService
                .GetByAlias(x))
                .WhereNotNull()
                .Select(x => x.SectionAlias)
                .Distinct()
                .ToArray();

            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            return umbracoContext.Security.CurrentUser != null
                   && apps.Any(app => umbracoContext.Security.UserHasSectionAccess(
                       app, umbracoContext.Security.CurrentUser));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IsAuthorized())
            {

                context.Result = new ForbidResult();
            }
        }
    }
}
