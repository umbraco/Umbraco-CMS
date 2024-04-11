using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeAreaRoutes" /> class.
    /// </summary>
    [Obsolete("The globalSettings and hostingEnvironment parameters are not required anymore, use the other constructor instead. This constructor will be removed in a future version.")]
    public BackOfficeAreaRoutes(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment, IRuntimeState runtimeState)
        : this(runtimeState)
    { }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Run)
        {
            MapMinimalBackOffice(endpoints);
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
$"{Constants.System.UmbracoPathSegment)}/{{**slug}}"),
            new
            {
                Controller = ControllerExtensions.GetControllerName<BackOfficeDefaultController>(),
                Action = nameof(BackOfficeDefaultController.Index),
            },
            constraints: new { slug = @"^(section.*|upgrade|install|logout)$" });
    }
}
