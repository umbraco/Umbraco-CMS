using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
/// Creates routes for the standalone basic auth login controller.
/// Only active when basic auth is enabled and <see cref="IBackOfficeSignInManager"/> is available.
/// </summary>
internal sealed class BasicAuthLoginRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtimeState;
    private readonly string _backOfficePath;

    public BasicAuthLoginRoutes(
        IRuntimeState runtimeState,
        IHostingEnvironment hostingEnvironment)
    {
        _runtimeState = runtimeState;
        _backOfficePath = hostingEnvironment.GetBackOfficePath();
    }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        var controllerName = nameof(BasicAuthLoginController)
            .Replace("Controller", string.Empty, StringComparison.Ordinal);

        endpoints.MapControllerRoute(
            name: "BasicAuthLogin",
            pattern: _backOfficePath.TrimStart('/') + "/basic-auth/{action=Login}",
            defaults: new { controller = controllerName });
    }
}
