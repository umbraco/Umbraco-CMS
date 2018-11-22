using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A custom area for controllers that are plugins
	/// </summary>	
	internal class PluginControllerArea : AreaRegistration
	{
		private readonly IEnumerable<PluginControllerMetadata> _surfaceControllers;
        private readonly IEnumerable<PluginControllerMetadata> _apiControllers;
		private readonly string _areaName;

		/// <summary>
		/// The constructor accepts all types of plugin controllers and will verify that ALL of them have the same areaName assigned to them 
		/// based on their PluginControllerAttribute. If they are not the same an exception will be thrown.
		/// </summary>
		/// <param name="pluginControllers"></param>		
		public PluginControllerArea(IEnumerable<PluginControllerMetadata> pluginControllers)
		{
			var controllers = pluginControllers.ToArray();

			if (controllers.Any(x => x.AreaName.IsNullOrWhiteSpace()))
			{
				throw new InvalidOperationException("Cannot create a PluginControllerArea unless all plugin controllers assigned have a PluginControllerAttribute assigned");
			}
			_areaName = controllers.First().AreaName;
			foreach(var c in controllers)
			{
				if (c.AreaName != _areaName)
				{
					throw new InvalidOperationException("Cannot create a PluginControllerArea unless all plugin controllers assigned have the same AreaName. The first AreaName found was " + _areaName + " however, the controller of type " + c.GetType().FullName + " has an AreaName of " + c.AreaName);
				}
			}

			//get the controllers
			_surfaceControllers = controllers.Where(x => TypeHelper.IsTypeAssignableFrom<SurfaceController>(x.ControllerType));
            _apiControllers = controllers.Where(x => TypeHelper.IsTypeAssignableFrom<UmbracoApiController>(x.ControllerType));
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			MapRouteSurfaceControllers(context.Routes, _surfaceControllers);
		    MapRouteApiControllers(context.Routes, _apiControllers);
		}

		public override string AreaName
		{
			get { return _areaName; }
		}

		/// <summary>
		/// Registers all surface controller routes
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="surfaceControllers"></param>
		/// <remarks>
		/// The routes will be:
		/// 
		/// /Umbraco/[AreaName]/[ControllerName]/[Action]/[Id]
		/// </remarks>
		private void MapRouteSurfaceControllers(RouteCollection routes, IEnumerable<PluginControllerMetadata> surfaceControllers)
		{
			foreach (var s in surfaceControllers)
			{
				var route = this.RouteControllerPlugin(s.ControllerName, s.ControllerType, routes, "", "Index", UrlParameter.Optional, "surface");
                //set the route handler to our SurfaceRouteHandler
                route.RouteHandler = new SurfaceRouteHandler();
			}
		}

        /// <summary>
        /// Registers all api controller routes
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="apiControllers"></param>
        private void MapRouteApiControllers(RouteCollection routes, IEnumerable<PluginControllerMetadata> apiControllers)
        {
            foreach (var s in apiControllers)
            {
                this.RouteControllerPlugin(s.ControllerName, s.ControllerType, routes, "", "", UrlParameter.Optional, "api", 
                    isMvc: false, 
                    areaPathPrefix: s.IsBackOffice ? "backoffice" : null);
            }
        }
	}
}