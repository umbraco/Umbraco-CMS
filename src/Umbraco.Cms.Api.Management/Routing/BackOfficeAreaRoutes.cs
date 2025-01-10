using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
///     Creates routes for the back office area
/// </summary>
public sealed class BackOfficeAreaRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;
    private readonly string _umbracoPathSegment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeAreaRoutes" /> class.
    /// </summary>
    public BackOfficeAreaRoutes(
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
    {
        _runtimeState = runtimeState;
        _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
    }

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 15.")]
    public BackOfficeAreaRoutes(
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        UmbracoApiControllerTypeCollection apiControllers) : this(globalSettings, hostingEnvironment, runtimeState)
    {

    }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {


        switch (_runtimeState.Level)
        {
            case RuntimeLevel.Install:
            case RuntimeLevel.Upgrade:
            case RuntimeLevel.Run:
                MapMinimalBackOffice(endpoints);
                endpoints.MapHub<BackofficeHub>(_umbracoPathSegment + Constants.Web.BackofficeSignalRHub);
                endpoints.MapHub<ServerEventHub>(_umbracoPathSegment + Constants.Web.ServerEventSignalRHub);
                break;
            case RuntimeLevel.BootFailed:
            case RuntimeLevel.Unknown:
            case RuntimeLevel.Boot:
                break;
        }
    }

    /// <summary>
    ///     Map the minimal routes required to load the back office login and auth
    /// </summary>
    private void MapMinimalBackOffice(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUmbracoRoute<BackOfficeDefaultController>(
            _umbracoPathSegment,
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
            new StringBuilder(_umbracoPathSegment).Append("/{**slug}").ToString(),
            new
            {
                Controller = ControllerExtensions.GetControllerName<BackOfficeDefaultController>(),
                Action = nameof(BackOfficeDefaultController.Index),
            },
            constraints: new { slug = @"^(section|preview|upgrade|install|oauth_complete|logout|error).*$" });
    }
}
