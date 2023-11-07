using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Security.Authorization.Content.Branch;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorization requirement for the <see cref="ContentPermissionHandler" />
///     and <see cref="ContentBranchPermissionHandler" />.
/// </summary>
public class ContentPermissionRequirement : IAuthorizationRequirement
{
}
