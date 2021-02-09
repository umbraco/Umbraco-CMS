using System;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Cms.Core.Web;
using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    public class TestControllerActivator : TestControllerActivatorBase
    {
        private readonly Func<HttpRequestMessage, IUmbracoContextAccessor, ApiController> _factory;

        public TestControllerActivator(Func<HttpRequestMessage, IUmbracoContextAccessor, ApiController> factory)
        {
            _factory = factory;
        }

        protected override ApiController CreateController(Type controllerType, HttpRequestMessage msg, IUmbracoContextAccessor umbracoContextAccessor)
        {
            return _factory(msg, umbracoContextAccessor);
        }
    }
}
