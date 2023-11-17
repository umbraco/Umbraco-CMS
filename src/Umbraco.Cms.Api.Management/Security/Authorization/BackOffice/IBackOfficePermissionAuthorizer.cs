using System.Security.Principal;

namespace Umbraco.Cms.Api.Management.Security.Authorization.BackOffice;

/// <summary>
///     Authorizes back-office access.
/// </summary>
public interface IBackOfficePermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the back-office.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="requireApproval">A value indicating whether the user needs to be approved.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, bool requireApproval);
}
