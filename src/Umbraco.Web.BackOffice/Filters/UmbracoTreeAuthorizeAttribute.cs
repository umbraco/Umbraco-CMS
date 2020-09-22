using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security;
using Umbraco.Web.Services;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    ///     Ensures that the current user has access to the application for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    ///     This would allow a tree to be moved between sections
    /// </remarks>
    public class UmbracoTreeAuthorizeAttribute : TypeFilterAttribute
    {
        public UmbracoTreeAuthorizeAttribute(params string[] treeAliases) : base(typeof(UmbracoTreeAuthorizeFilter))
        {
            Arguments = new object[]
            {
                treeAliases
            };
        }

        private sealed class UmbracoTreeAuthorizeFilter : IAuthorizationFilter
        {
            /// <summary>
            ///     Can be used by unit tests to enable/disable this filter
            /// </summary>
            internal static readonly bool Enable = true;

            private readonly string[] _treeAliases;

            private readonly ITreeService _treeService;
            private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;

            /// <summary>
            ///     Constructor to set authorization to be based on a tree alias for which application security will be applied
            /// </summary>
            /// <param name="treeService"></param>
            /// <param name="backofficeSecurityAccessor"></param>
            /// <param name="treeAliases">
            ///     If the user has access to the application that the treeAlias is specified in, they will be authorized.
            ///     Multiple trees may be specified.
            /// </param>
            public UmbracoTreeAuthorizeFilter(ITreeService treeService, IBackofficeSecurityAccessor backofficeSecurityAccessor,
                params string[] treeAliases)
            {
                _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _treeAliases = treeAliases;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!IsAuthorized())
                {
                    context.Result = new ForbidResult();
                }
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

                return _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser != null
                       && apps.Any(app => _backofficeSecurityAccessor.BackofficeSecurity.UserHasSectionAccess(
                           app, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser));
            }
        }
    }
}
