using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers.Stubs
{
	/// <summary>
	/// Used in place of the UmbracoControllerFactory which relies on BuildManager which throws exceptions in a unit test context
	/// </summary>
	internal class TestControllerFactory : IControllerFactory
	{
	    private readonly UmbracoContext _umbracoContext;
	    private readonly ILogger _logger;

	    public TestControllerFactory(UmbracoContext umbracoContext, ILogger logger)
	    {
	        _umbracoContext = umbracoContext;
	        _logger = logger;
	    }

	    public IController CreateController(RequestContext requestContext, string controllerName)
		{
			var types = TypeFinder.FindClassesOfType<ControllerBase>(new[] { Assembly.GetExecutingAssembly() });

			var controllerTypes = types.Where(x => x.Name.Equals(controllerName + "Controller", StringComparison.InvariantCultureIgnoreCase));
			var t = controllerTypes.SingleOrDefault();

			if (t == null)
				return null;

	        var possibleParams = new object[]
	        {
	            _umbracoContext, _logger
	        };
	        var ctors = t.GetConstructors();
	        foreach (var ctor in ctors.OrderByDescending(x => x.GetParameters().Count()))
	        {
	            var args = new List<object>();
	            var allParams = ctor.GetParameters().ToArray();
	            foreach (var parameter in allParams)
	            {
	                var found = possibleParams.SingleOrDefault(x => x.GetType() == parameter.ParameterType);
	                if (found != null) args.Add(found);
	            }
	            if (args.Count == allParams.Length)
	            {
                    return Activator.CreateInstance(t, args.ToArray()) as IController;
	            }
	        }
	        return null;
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