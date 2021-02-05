using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.Website.Collections;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// Creates routes for surface controllers
    /// </summary>
    public sealed class FrontEndRoutes : IAreaRoutes
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly SurfaceControllerTypeCollection _surfaceControllerTypeCollection;
        private readonly UmbracoApiControllerTypeCollection _apiControllers;
        private readonly string _umbracoPathSegment;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontEndRoutes"/> class.
        /// </summary>
        public FrontEndRoutes(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            SurfaceControllerTypeCollection surfaceControllerTypeCollection,
            UmbracoApiControllerTypeCollection apiControllers)
        {
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _runtimeState = runtimeState;
            _surfaceControllerTypeCollection = surfaceControllerTypeCollection;
            _apiControllers = apiControllers;
            _umbracoPathSegment = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);
        }

        /// <inheritdoc/>
        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return;
            }

            AutoRouteSurfaceControllers(endpoints);
            AutoRouteFrontEndApiControllers(endpoints);
        }

        /// <summary>
        /// Auto-routes all front-end surface controllers
        /// </summary>
        private void AutoRouteSurfaceControllers(IEndpointRouteBuilder endpoints)
        {
            foreach (Type controller in _surfaceControllerTypeCollection)
            {
                // exclude front-end api controllers
                PluginControllerMetadata meta = PluginController.GetMetadata(controller);

                endpoints.MapUmbracoRoute(
                    meta.ControllerType,
                    _umbracoPathSegment,
                    meta.AreaName,
                    "Surface");
            }
        }

        /// <summary>
        /// Auto-routes all front-end api controllers
        /// </summary>
        private void AutoRouteFrontEndApiControllers(IEndpointRouteBuilder endpoints)
        {
            foreach (Type controller in _apiControllers)
            {
                PluginControllerMetadata meta = PluginController.GetMetadata(controller);

                // exclude back-end api controllers
                if (meta.IsBackOffice)
                {
                    continue;
                }

                endpoints.MapUmbracoApiRoute(
                    meta.ControllerType,
                    _umbracoPathSegment,
                    meta.AreaName,
                    meta.IsBackOffice,
                    defaultAction: string.Empty); // no default action (this is what we had before)
            }
        }
    }
}
