using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A custom area for controllers that are plugins
	/// </summary>	
	internal class PluginControllerArea : AreaRegistration
	{
		private readonly IEnumerable<PluginControllerMetadata> _surfaceControllers;
		private readonly string _areaName;

		/// <summary>
		/// The constructor accepts all types of plugin controllers and will verify that ALL of them have the same areaName assigned to them 
		/// based on their PluginControllerAttribute. If they are not the same an exception will be thrown.
		/// </summary>
		/// <param name="pluginControllers"></param>		
		public PluginControllerArea(IEnumerable<PluginControllerMetadata> pluginControllers)
		{
			//TODO: When we have other future plugin controllers we need to combine them all into one list here to do our validation.
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

			//get the surface controllers
			_surfaceControllers = controllers.Where(x => TypeHelper.IsTypeAssignableFrom<SurfaceController>(x.ControllerType));
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			MapRouteSurfaceControllers(context.Routes, _surfaceControllers);
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
		private void MapRouteSurfaceControllers(RouteCollection routes, IEnumerable<PluginControllerMetadata> surfaceControllers)
		{
			foreach (var s in surfaceControllers)
			{
				this.RouteControllerPlugin(s.ControllerName, s.ControllerType, routes, "Surface", "Index", UrlParameter.Optional, "surface");
			}
		}
	}
}