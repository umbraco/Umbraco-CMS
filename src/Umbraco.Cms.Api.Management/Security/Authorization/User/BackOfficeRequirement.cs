using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorization requirement for the <see cref="BackOfficeHandler" />.
/// </summary>
public class BackOfficeRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeRequirement" /> class.
    /// </summary>
    /// <param name="requireApproval">Flag for whether back-office user approval is required.</param>
    public BackOfficeRequirement(bool requireApproval = true) => RequireApproval = requireApproval;

    /// <summary>
    ///     Gets a value indicating whether back-office user approval is required.
    /// </summary>
    public bool RequireApproval { get; }
}
