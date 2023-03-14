// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     The user must have access to all descendant nodes of the content item in order to continue.
/// </summary>
public class ContentPermissionsPublishBranchHandler : MustSatisfyRequirementAuthorizationHandler<
    ContentPermissionsPublishBranchRequirement, IContent>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ContentPermissions _contentPermissions;
    private readonly IEntityService _entityService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionsPublishBranchHandler" /> class.
    /// </summary>
    /// <param name="entityService">Service for entity operations.</param>
    /// <param name="contentPermissions">per for user content authorization checks.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    public ContentPermissionsPublishBranchHandler(
        IEntityService entityService,
        ContentPermissions contentPermissions,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _entityService = entityService;
        _contentPermissions = contentPermissions;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context,
        ContentPermissionsPublishBranchRequirement requirement, IContent resource)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        var denied = new List<IUmbracoEntity>();
        var page = 0;
        const int pageSize = 500;
        var total = long.MaxValue;

        while (page * pageSize < total)
        {
            // Order descendents by shallowest to deepest, this allows us to check permissions from top to bottom so we can exit
            // early if a permission higher up fails.
            IEnumerable<IEntitySlim> descendants = _entityService.GetPagedDescendants(
                resource.Id,
                UmbracoObjectTypes.Document,
                page++,
                pageSize,
                out total,
                ordering: Ordering.By("path"));

            foreach (IEntitySlim c in descendants)
            {
                // If this item's path has already been denied or if the user doesn't have access to it, add to the deny list.
                if (denied.Any(x => c.Path.StartsWith($"{x.Path},")) ||
                    _contentPermissions.CheckPermissions(
                        c,
                        currentUser,
                        requirement.Permission) == ContentPermissions.ContentAccess.Denied)
                {
                    denied.Add(c);
                }
            }
        }

        return Task.FromResult(denied.Count == 0);
    }
}
