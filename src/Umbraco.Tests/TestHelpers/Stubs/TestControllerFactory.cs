using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core;

namespace Umbraco.Tests.TestHelpers.Stubs
{
	/// <summary>
	/// Used in place of the UmbracoControllerFactory which relies on BuildManager which throws exceptions in a unit test context
	/// </summary>
	internal class TestControllerFactory : IControllerFactory
	{

		public IController CreateController(RequestContext requestContext, string controllerName)
		{
			var types = TypeFinder.FindClassesOfType<ControllerBase>(new[] { Assembly.GetExecutingAssembly() });

			var controllerTypes = types.Where(x => x.Name.Equals(controllerName + "Controller", StringComparison.InvariantCultureIgnoreCase));
			var t = controllerTypes.SingleOrDefault();

			if (t == null)
				return null;

			return Activator.CreateInstance(t) as IController;
		}

		public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
		{
			return SessionStateBehavior.Disabled;
		}

		public void ReleaseController(IController controller)
		{
			controller.DisposeIfDisposable();
		}
	}
}