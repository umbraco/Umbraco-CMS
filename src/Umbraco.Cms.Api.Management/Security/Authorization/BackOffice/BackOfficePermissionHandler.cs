using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.BackOffice;

/// <summary>
///     Authorizes that the current user has the necessary back-office access.
/// </summary>
public class BackOfficePermissionHandler : MustSatisfyRequirementAuthorizationHandler<BackOfficePermissionRequirement>
{
    private readonly IBackOfficePermissionAuthorizer _backOfficePermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficePermissionHandler" /> class.
    /// </summary>
    /// <param name="backOfficePermissionAuthorizer">Authorizer for back-office access.</param>
    public BackOfficePermissionHandler(IBackOfficePermissionAuthorizer backOfficePermissionAuthorizer)
        => _backOfficePermissionAuthorizer = backOfficePermissionAuthorizer;

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, BackOfficePermissionRequirement requirement)
        => _backOfficePermissionAuthorizer.IsAuthorizedAsync(context.User, requirement.RequireApproval);
}
