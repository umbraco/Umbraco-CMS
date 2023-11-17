using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.BackOffice;

/// <summary>
///     Authorization requirement for the <see cref="BackOfficePermissionHandler" />.
/// </summary>
public class BackOfficePermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficePermissionRequirement" /> class.
    /// </summary>
    /// <param name="requireApproval">Flag for whether back-office user approval is required.</param>
    public BackOfficePermissionRequirement(bool requireApproval = true)
        => RequireApproval = requireApproval;

    /// <summary>
    ///     Gets a value indicating whether back-office user approval is required.
    /// </summary>
    public bool RequireApproval { get; }
}
