using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorization requirement for the <see cref="AllowedApplicationHandler" />.
/// </summary>
internal sealed class AllowedApplicationRequirement : IAuthorizationRequirement
{
    public string[] Applications { get; }

    public AllowedApplicationRequirement(params string[] applications)
        => Applications = applications;
}
