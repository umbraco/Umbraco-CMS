using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.BackOffice.Routing
{
    public class BackOfficeAreaRoutes : IAreaRoutes
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public BackOfficeAreaRoutes(IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            var umbracoPath = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);

            // TODO: We need to auto-route "Umbraco Api Controllers" for the back office

            // TODO: We will also need to detect runtime state here and redirect to the installer,
            // Potentially switch this to dynamic routing so we can essentially disable/overwrite the back office routes to redirect to install
            // when required, example https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/

            endpoints.MapAreaControllerRoute(
                "Umbraco_back_office", // TODO: Same name as before but we should change these so they have a convention
                Constants.Web.Mvc.BackOfficeArea,
                $"{umbracoPath}/{{Action}}/{{id?}}",
                new { controller = ControllerExtensions.GetControllerName<BackOfficeController>(), action = "Default" },
                // Limit the action/id to only allow characters - this is so this route doesn't hog all other
                // routes like: /umbraco/channels/word.aspx, etc...
                // (Not that we have to worry about too many of those these days, there still might be a need for these constraints).
                new
                {
                    action = @"[a-zA-Z]*",
                    id = @"[a-zA-Z]*"
                });

            endpoints.MapAreaControllerRoute(
                "Umbraco_preview", // TODO: Same name as before but we should change these so they have a convention
                Constants.Web.Mvc.BackOfficeArea,
                $"{umbracoPath}/preview/{{Action}}/{{editor?}}",
                // TODO: Change this to use ControllerExtensions.GetControllerName once the PreviewController is moved to Umbraco.Web.BackOffice.Controllers
                new { controller = "Preview", action = "Index" });
        }
    }
}
