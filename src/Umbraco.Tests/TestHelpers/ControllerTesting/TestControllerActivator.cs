using System;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    public class TestControllerActivator : TestControllerActivatorBase
    {
        private readonly Func<HttpRequestMessage, IUmbracoContextAccessor, UmbracoHelper, ApiController> _factory;

        public TestControllerActivator(Func<HttpRequestMessage, IUmbracoContextAccessor, UmbracoHelper, ApiController> factory)
        {
            _factory = factory;
        }

        protected override ApiController CreateController(Type controllerType, HttpRequestMessage msg, IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper helper)
        {
            return _factory(msg, umbracoContextAccessor, helper);
        }
    }
}
