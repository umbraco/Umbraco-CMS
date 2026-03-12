using Microsoft.AspNetCore.SignalR;

namespace Umbraco.Cms.Api.Management.Preview;

/// <summary>
/// Represents a SignalR hub used for managing content preview functionality within Umbraco CMS.
/// Enables real-time communication for preview operations.
/// </summary>
public class PreviewHub : Hub<IPreviewHub>
{
}
