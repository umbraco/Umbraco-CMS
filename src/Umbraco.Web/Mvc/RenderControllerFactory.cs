using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A controller factory for the render pipeline of Umbraco. This controller factory tries to create a controller with the supplied
	/// name, and falls back to UmbracoController if none was found.
	/// </summary>
	/// <remarks></remarks>
	public class RenderControllerFactory : IFilteredControllerFactory
	{
		private readonly OverridenDefaultControllerFactory _innerFactory = new OverridenDefaultControllerFactory();

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public RenderControllerFactory()
		{

		}


		/// <summary>
		/// Determines whether this instance can handle the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns><c>true</c> if this instance can handle the specified request; otherwise, <c>false</c>.</returns>
		/// <remarks></remarks>
		public bool CanHandle(RequestContext request)
		{
			var dataToken = request.RouteData.DataTokens["area"];
			return dataToken == null || string.IsNullOrWhiteSpace(dataToken.ToString());
		}

		/// <summary>
		/// Creates the specified controller by using the specified request context.
		/// </summary>
		/// <returns>
		/// The controller.
		/// </returns>
		/// <param name="requestContext">The request context.</param><param name="controllerName">The name of the controller.</param>
		public IController CreateController(RequestContext requestContext, string controllerName)
		{
			Type controllerType = _innerFactory.GetControllerType(requestContext, controllerName) ??
			                      _innerFactory.GetControllerType(requestContext, ControllerExtensions.GetControllerName(typeof(RenderMvcController)));

			return _innerFactory.GetControllerInstance(requestContext, controllerType);
		}

		/// <summary>
		/// Gets the controller's session behavior.
		/// </summary>
		/// <returns>
		/// The controller's session behavior.
		/// </returns>
		/// <param name="requestContext">The request context.</param><param name="controllerName">The name of the controller whose session behavior you want to get.</param>
		public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
		{
			return ((IControllerFactory)_innerFactory).GetControllerSessionBehavior(requestContext, controllerName);
		}

		/// <summary>
		/// Releases the specified controller.
		/// </summary>
		/// <param name="controller">The controller.</param>
		public void ReleaseController(IController controller)
		{
			_innerFactory.ReleaseController(controller);
		}

		/// <summary>
		/// By default, <see cref="DefaultControllerFactory"/> only exposes <see cref="IControllerFactory.CreateController"/> which throws an exception
		/// if the controller is not found. Since we want to try creating a controller, and then fall back to <see cref="RenderMvcController"/> if one isn't found,
		/// this nested class changes the visibility of <see cref="DefaultControllerFactory"/>'s internal methods in order to not have to rely on a try-catch.
		/// </summary>
		/// <remarks></remarks>
		public class OverridenDefaultControllerFactory : DefaultControllerFactory
		{
			public new IController GetControllerInstance(RequestContext requestContext, Type controllerType)
			{
				return base.GetControllerInstance(requestContext, controllerType);
			}

			public new Type GetControllerType(RequestContext requestContext, string controllerName)
			{
				return base.GetControllerType(requestContext, controllerName);
			}
		}
	}
}