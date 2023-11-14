using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Security.Authorization.Content.Branch;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorization requirement for the <see cref="ContentPermissionHandler" />
///     and <see cref="ContentBranchPermissionHandler" />.
/// </summary>
// TODO: ContentBranchPermissionHandler might need its own requirement and policy, once we have publishing functionality in place.
public class ContentPermissionRequirement : IAuthorizationRequirement
{
}
