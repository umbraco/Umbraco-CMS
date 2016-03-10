using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Creates SurfaceControllers
	/// </summary>
	[Obsolete("This is not used in the codebase and will be removed from the core in future versions")]
	public class SurfaceControllerFactory : RenderControllerFactory
	{
		/// <summary>
		/// Check if the correct data tokens are in the route values so that we know its a surface controller route
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public override bool CanHandle(RequestContext request)
		{
			var area = request.RouteData.DataTokens["area"];
			
			//if its a non-area route don't handle, all surface controllers will be in the 'umbraco' area
			if (area == null || string.IsNullOrWhiteSpace(area.ToString()))
				return false;

			//ensure there is an umbraco token set
			var umbracoToken = request.RouteData.DataTokens[Core.Constants.Web.UmbracoDataToken];
			if (umbracoToken == null || string.IsNullOrWhiteSpace(umbracoToken.ToString()))
				return false;

			return true;
		}

		/// <summary>
		/// Create the controller
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="controllerName"></param>
		/// <returns></returns>
		public override IController CreateController(RequestContext requestContext, string controllerName)
		{
			//first try to instantiate with the DependencyResolver, if that fails, try with the UmbracoContext as a param, if that fails try with no params.
			var controllerType = GetControllerType(requestContext, controllerName);
			if (controllerType == null)
				throw new InvalidOperationException("Could not find a controller type for the controller name " + controllerName);

			object controllerObject;
			try
			{
				controllerObject = DependencyResolver.Current.GetService(controllerType);
			}
			catch (Exception)
			{
				try
				{
					controllerObject = Activator.CreateInstance(controllerType, UmbracoContext.Current);
				}
				catch (Exception)
				{
					//if this throws an exception, we'll let it
					controllerObject = Activator.CreateInstance(controllerType);
				}
			}
			//if an exception is thrown here, we want it to be thrown as its an invalid cast.
			return (IController)controllerObject;			
		}
	}
}