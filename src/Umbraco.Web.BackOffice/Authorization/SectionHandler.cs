// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Ensures that the current user has access to the section
/// </summary>
/// <remarks>
///     The user only needs access to one of the sections specified, not all of the sections.
/// </remarks>
public class SectionHandler : MustSatisfyRequirementAuthorizationHandler<SectionRequirement>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SectionHandler" /> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back-office security.</param>
    public SectionHandler(IBackOfficeSecurityAccessor backOfficeSecurityAccessor) =>
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, SectionRequirement requirement)
    {
        var authorized = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser != null &&
                         requirement.SectionAliases
                             .Any(app => _backOfficeSecurityAccessor.BackOfficeSecurity.UserHasSectionAccess(
                                 app, _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser));

        return Task.FromResult(authorized);
    }
}
