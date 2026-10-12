using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Preview;

/// <summary>
/// Represents a SignalR hub used for managing content preview functionality within Umbraco CMS.
/// Enables real-time communication for preview operations.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class PreviewHub : Hub<IPreviewHub>
{
}
