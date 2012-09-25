using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A custom area for surface controller routes
	/// </summary>
	internal class SurfaceControllerArea : AreaRegistration
	{
		private readonly IEnumerable<SurfaceController> _surfaceControllers;

		public SurfaceControllerArea(IEnumerable<SurfaceController> surfaceControllers)
		{
			_surfaceControllers = surfaceControllers;
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			MapRouteSurfaceControllers(context.Routes, _surfaceControllers);
		}

		public override string AreaName
		{
			get { return "Surface"; }
		}

		/// <summary>
		/// Registers all surface controller routes
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="surfaceControllers"></param>
		private void MapRouteSurfaceControllers(RouteCollection routes, IEnumerable<SurfaceController> surfaceControllers)
		{
			var areaName = GlobalSettings.UmbracoMvcArea;

			//local surface controllers do not contain the attribute 			
			var localSurfaceControlleres = surfaceControllers.Where(x => TypeExtensions.GetCustomAttribute<SurfaceControllerAttribute>(x.GetType(), false) == null);
			foreach (var s in localSurfaceControlleres)
			{
				var meta = s.GetMetadata();
				var route = routes.MapRoute(
					string.Format("umbraco-{0}-{1}", "surface", meta.ControllerName),
					areaName + "/Surface/" + meta.ControllerName + "/{action}/{id}",//url to match
					new { controller = meta.ControllerName, action = "Index", id = UrlParameter.Optional },
					new[] { meta.ControllerNamespace }); //only match this namespace
				route.DataTokens.Add("area", areaName); //only match this area
				route.DataTokens.Add("umbraco", "surface"); //ensure the umbraco token is set
			}

			var pluginSurfaceControllers = surfaceControllers.Where(x => x.GetType().GetCustomAttribute<SurfaceControllerAttribute>(false) != null);
			foreach (var s in pluginSurfaceControllers)
			{
				var meta = s.GetMetadata();
				this.RouteControllerPlugin(meta.ControllerId.Value, meta.ControllerName, meta.ControllerType, routes, "surfaceId", "Surface", "", "Index", UrlParameter.Optional, "surface");
			}
		}
	}
}