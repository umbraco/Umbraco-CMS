// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Ensures that the current user has access to the section for which the specified tree(s) belongs
/// </summary>
/// <remarks>
///     This would allow a tree to be moved between sections.
///     The user only needs access to one of the trees specified, not all of the trees.
/// </remarks>
public class TreeHandler : MustSatisfyRequirementAuthorizationHandler<TreeRequirement>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ITreeService _treeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeHandler" /> class.
    /// </summary>
    /// <param name="treeService">Service for section tree operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    public TreeHandler(ITreeService treeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, TreeRequirement requirement)
    {
        var apps = requirement.TreeAliases
            .Select(x => _treeService.GetByAlias(x))
            .WhereNotNull()
            .Select(x => x.SectionAlias)
            .Distinct()
            .ToArray();

        var isAuth = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser != null &&
                     apps.Any(app => _backOfficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                         app, _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

        return Task.FromResult(isAuth);
    }
}
