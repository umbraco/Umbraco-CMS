using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorization requirement for the <see cref="ContentPermissionHandler" />,
///     <see cref="ContentRecycleBinPermissionHandler" />, <see cref="ContentRootPermissionHandler" />
///     and <see cref="ContentBranchPermissionHandler" />.
/// </summary>
public class ContentPermissionRequirement : IAuthorizationRequirement
{
}
