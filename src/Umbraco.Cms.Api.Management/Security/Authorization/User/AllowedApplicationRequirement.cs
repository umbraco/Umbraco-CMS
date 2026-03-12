using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorization requirement for the <see cref="AllowedApplicationHandler" />.
/// </summary>
internal sealed class AllowedApplicationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the list of allowed application names for the user requirement.
    /// </summary>
    public string[] Applications { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedApplicationRequirement"/> class, specifying which applications are allowed for the requirement.
    /// </summary>
    /// <param name="applications">An array of application names that are permitted.</param>
    public AllowedApplicationRequirement(params string[] applications)
        => Applications = applications;
}
