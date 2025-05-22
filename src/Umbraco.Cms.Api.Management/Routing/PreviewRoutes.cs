using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Api.Management.Preview;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
///     Creates routes for the preview hub
/// </summary>
public sealed class PreviewRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;

    public PreviewRoutes(IRuntimeState runtimeState)
        => _runtimeState = runtimeState;

    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Run)
        {
            endpoints.MapHub<PreviewHub>(GetPreviewHubRoute());
        }
    }

    /// <summary>
    /// Gets the path to the SignalR hub used for preview.
    /// </summary>
    /// <returns>
    /// The path to the SignalR hub used for preview.
    /// </returns>
    public string GetPreviewHubRoute() => $"/{Constants.System.UmbracoPathSegment}/{nameof(PreviewHub)}";
}
