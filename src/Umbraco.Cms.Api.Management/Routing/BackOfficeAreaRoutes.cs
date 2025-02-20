using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Creates routes for the back office area.
/// </summary>
public sealed class BackOfficeAreaRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeAreaRoutes" /> class.
    /// </summary>
    public BackOfficeAreaRoutes(IRuntimeState runtimeState)
        => _runtimeState = runtimeState;

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Run)
        {
            MapMinimalBackOffice(endpoints);

            endpoints.MapHub<BackofficeHub>(Constants.System.UmbracoPathSegment + Constants.Web.BackofficeSignalRHub);
            endpoints.MapHub<ServerEventHub>(Constants.System.UmbracoPathSegment + Constants.Web.ServerEventSignalRHub);
        }
    }

    /// <summary>
    ///     Map the minimal routes required to load the back office login and auth
    /// </summary>
    private void MapMinimalBackOffice(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUmbracoRoute<BackOfficeDefaultController>(
            Constants.System.UmbracoPathSegment,
            null!,
            string.Empty,
            "Index",
            false,
            // Limit the action/id to only allow characters - this is so this route doesn't hog all other
            // routes like: /umbraco/channels/word.aspx, etc...
            // (Not that we have to worry about too many of those these days, there still might be a need for these constraints).
            new { action = @"[a-zA-Z]*", id = @"[a-zA-Z]*" });

        endpoints.MapControllerRoute(
            "catch-all-sections-to-client",
            $"{Constants.System.UmbracoPathSegment}/{{**slug}}",
            new
            {
                Controller = ControllerExtensions.GetControllerName<BackOfficeDefaultController>(),
                Action = nameof(BackOfficeDefaultController.Index),
            },
            constraints: new { slug = @"^(section|preview|upgrade|install|oauth_complete|logout|error).*$" });
    }
}
