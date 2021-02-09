using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Web;
using Umbraco.Core;
using Umbraco.Web;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    /// <summary>
    /// Used in place of the UmbracoControllerFactory which relies on BuildManager which throws exceptions in a unit test context
    /// </summary>
    internal class TestControllerFactory : IControllerFactory
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<TestControllerFactory> _logger;
        private readonly Func<RequestContext, IController> _factory;

        public TestControllerFactory(IUmbracoContextAccessor umbracoContextAccessor, ILogger<TestControllerFactory> logger)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        public TestControllerFactory(IUmbracoContextAccessor umbracoContextAccessor, ILogger<TestControllerFactory> logger, Func<RequestContext, IController> factory)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
            _factory = factory;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            if (_factory != null) return _factory(requestContext);

            var typeFinder = TestHelper.GetTypeFinder();
            var types = typeFinder.FindClassesOfType<ControllerBase>(new[] { Assembly.GetExecutingAssembly() });

            var controllerTypes = types.Where(x => x.Name.Equals(controllerName + "Controller", StringComparison.InvariantCultureIgnoreCase));
            var t = controllerTypes.SingleOrDefault();

            if (t == null)
                return null;

            var possibleParams = new object[]
            {
                _umbracoContextAccessor, _logger
            };
            var ctors = t.GetConstructors();
            foreach (var ctor in ctors.OrderByDescending(x => x.GetParameters().Length))
            {
                var args = new List<object>();
                var allParams = ctor.GetParameters().ToArray();
                foreach (var parameter in allParams)
                {
                    var found = possibleParams.SingleOrDefault(x => x.GetType() == parameter.ParameterType)
                             ?? Current.Factory.GetRequiredService(parameter.ParameterType);
                    if (found != null) args.Add(found);
                }
                if (args.Count == allParams.Length)
                {
                    return Activator.CreateInstance(t, args.ToArray()) as IController;
                }
            }
            return null;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Disabled;
        }

        public void ReleaseController(IController controller)
        {
            controller.DisposeIfDisposable();
        }
    }
}
