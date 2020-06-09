using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Web.Security;
using Umbraco.Web.Services;

namespace Umbraco.Web.BackOffice.Filters
{

    public class UmbracoTreeAuthorizeAttribute : TypeFilterAttribute
    {
        public UmbracoTreeAuthorizeAttribute(params string[] treeAliases) : base(typeof(UmbracoTreeAuthorizeFilter))
        {
            base.Arguments = new object[]
            {
                treeAliases
            };
        }

         /// <summary>
    /// Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections
    /// </remarks>
    private sealed class UmbracoTreeAuthorizeFilter : IAuthorizationFilter
    {
        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        private readonly ITreeService _treeService;
        private readonly IWebSecurity _webSecurity;
        private readonly string[] _treeAliases;

        /// <summary>
        /// Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// </summary>
        /// <param name="webSecurity"></param>
        /// <param name="treeAliases">
        /// If the user has access to the application that the treeAlias is specified in, they will be authorized.
        /// Multiple trees may be specified.
        /// </param>
        /// <param name="treeService"></param>
        public UmbracoTreeAuthorizeFilter(ITreeService treeService, IWebSecurity webSecurity, params string[] treeAliases)
        {
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            _webSecurity = webSecurity ?? throw new ArgumentNullException(nameof(webSecurity));
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

            return _webSecurity.CurrentUser != null
                   && apps.Any(app => _webSecurity.UserHasSectionAccess(
                       app, _webSecurity.CurrentUser));
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


}
