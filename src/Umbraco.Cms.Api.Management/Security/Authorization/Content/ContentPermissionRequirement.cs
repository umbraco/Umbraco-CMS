using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorization requirement for the <see cref="ContentPermissionHandler" />,
///     <see cref="ContentRecycleBinPermissionHandler" /> and <see cref="ContentRootPermissionHandler" />.
/// </summary>
public class ContentPermissionRequirement : IAuthorizationRequirement
{
}
