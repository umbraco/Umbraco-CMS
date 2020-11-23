using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// The user must have access to all descendant nodes of the content item in order to continue
    /// </summary>
    public class ContentPermissionsPublishBranchHandler : AuthorizationHandler<ContentPermissionsPublishBranchRequirement, IContent>
    {
        private readonly IEntityService _entityService;
        private readonly ContentPermissions _contentPermissions;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ContentPermissionsPublishBranchHandler(
            IEntityService entityService,
            ContentPermissions contentPermissions,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _entityService = entityService;
            _contentPermissions = contentPermissions;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContentPermissionsPublishBranchRequirement requirement, IContent resource)
        {
            var denied = new List<IUmbracoEntity>();
            var page = 0;
            const int pageSize = 500;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                var descendants = _entityService.GetPagedDescendants(resource.Id, UmbracoObjectTypes.Document, page++, pageSize, out total,
                                //order by shallowest to deepest, this allows us to check permissions from top to bottom so we can exit
                                //early if a permission higher up fails
                                ordering: Ordering.By("path", Direction.Ascending));

                foreach (var c in descendants)
                {
                    //if this item's path has already been denied or if the user doesn't have access to it, add to the deny list
                    if (denied.Any(x => c.Path.StartsWith($"{x.Path},"))
                        || (_contentPermissions.CheckPermissions(c,
                            _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                            requirement.Permission) == ContentPermissions.ContentAccess.Denied))
                    {
                        denied.Add(c);
                    }
                }
            }

            if (denied.Count == 0)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
