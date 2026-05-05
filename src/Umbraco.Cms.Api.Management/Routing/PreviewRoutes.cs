using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Preview;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
///     Creates routes for the preview hub
/// </summary>
public sealed class PreviewRoutes : SignalRRoutesBase, IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Routing.PreviewRoutes"/> class, configuring preview routing based on the application's runtime state.
    /// </summary>
    /// <param name="runtimeState">An instance representing the current runtime state of the Umbraco application.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PreviewRoutes(IRuntimeState runtimeState)
        : this(
            runtimeState,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<SignalRSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Routing.PreviewRoutes"/> class, configuring preview routing based on the application's runtime state.
    /// </summary>
    /// <param name="runtimeState">An instance representing the current runtime state of the Umbraco application.</param>
    /// <param name="signalRSettings">The SignalR settings options.</param>
    public PreviewRoutes(IRuntimeState runtimeState, IOptions<SignalRSettings> signalRSettings)
        : base(signalRSettings)
    {
        _runtimeState = runtimeState;
    }

    /// <summary>
    /// Creates the preview routes on the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to add routes to.</param>
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Upgrading or RuntimeLevel.Run)
        {
            endpoints.MapHub<PreviewHub>(GetPreviewHubRoute(), ConfigureHubEndpoint);
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

