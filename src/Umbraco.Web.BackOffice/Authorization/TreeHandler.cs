using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using Umbraco.Core;
using System.Threading.Tasks;
using Umbraco.Core.Security;
using Umbraco.Web.Services;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// Ensures that the current user has access to the section for which the specified tree(s) belongs
    /// </summary>
    /// <remarks>
    /// This would allow a tree to be moved between sections.
    /// The user only needs access to one of the trees specified, not all of the trees.
    /// </remarks>
    public class TreeHandler : AuthorizationHandler<TreeRequirement>
    {

        private readonly ITreeService _treeService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        /// <summary>
        ///     Constructor to set authorization to be based on a tree alias for which application security will be applied
        /// </summary>
        /// <param name="treeService"></param>
        /// <param name="backofficeSecurityAccessor"></param>
        /// <param name="treeAliases">
        ///     If the user has access to the application that the treeAlias is specified in, they will be authorized.
        ///     Multiple trees may be specified.
        /// </param>
        public TreeHandler(ITreeService treeService, IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TreeRequirement requirement)
        {
            if (IsAuthorized(requirement))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }

        private bool IsAuthorized(TreeRequirement requirement)
        {
            var apps = requirement.TreeAliases.Select(x => _treeService
                    .GetByAlias(x))
                .WhereNotNull()
                .Select(x => x.SectionAlias)
                .Distinct()
                .ToArray();

            return _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser != null
                   && apps.Any(app => _backofficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                       app, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser));
        }
    }
}
