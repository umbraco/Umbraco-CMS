using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Install;

public class InstallAreaRoutes : IAreaRoutes
{
    private readonly IRuntimeState _runtime;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly LinkGenerator _linkGenerator;

    public InstallAreaRoutes(IRuntimeState runtime, IHostingEnvironment hostingEnvironment, LinkGenerator linkGenerator)
    {
        _runtime = runtime;
        _hostingEnvironment = hostingEnvironment;
        _linkGenerator = linkGenerator;
    }

    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtime.EnableInstaller())
        {
            var installPathSegment = _hostingEnvironment.ToAbsolute(Constants.SystemDirectories.Install).TrimStart('/');

            endpoints.MapUmbracoRoute<InstallApiController>(installPathSegment, Constants.Web.Mvc.InstallArea, "api", includeControllerNameInRoute: false);
            endpoints.MapUmbracoRoute<InstallController>(installPathSegment, Constants.Web.Mvc.InstallArea, string.Empty, includeControllerNameInRoute: false);

            // Register catch all because if we are in install/upgrade mode then we'll catch everything and redirect
            endpoints.MapFallbackToAreaController(nameof(InstallController.Redirect), ControllerExtensions.GetControllerName<InstallController>(), Constants.Web.Mvc.InstallArea);
        }
    }
}
