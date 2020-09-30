using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.Routing
{

    [TestFixture]
    public class BackOfficeAreaRoutesTests
    {
        [TestCase(RuntimeLevel.Install)]
        [TestCase(RuntimeLevel.BootFailed)]
        [TestCase(RuntimeLevel.Unknown)]
        [TestCase(RuntimeLevel.Boot)]
        public void RuntimeState_No_Routes(RuntimeLevel level)
        {
            var routes = GetBackOfficeAreaRoutes(level);
            var endpoints = new TestRouteBuilder();
            routes.CreateRoutes(endpoints);

            Assert.AreEqual(0, endpoints.DataSources.Count);
        }

        [Test]
        public void RuntimeState_Upgrade()
        {
            var routes = GetBackOfficeAreaRoutes(RuntimeLevel.Upgrade);
            var endpoints = new TestRouteBuilder();
            routes.CreateRoutes(endpoints);

            Assert.AreEqual(1, endpoints.DataSources.Count);
            var route = endpoints.DataSources.First();
            Assert.AreEqual(2, route.Endpoints.Count);
            AssertMinimalBackOfficeRoutes(route);
        }

        [Test]
        public void RuntimeState_Run()
        {
            var routes = GetBackOfficeAreaRoutes(RuntimeLevel.Run);
            var endpoints = new TestRouteBuilder();
            routes.CreateRoutes(endpoints);

            Assert.AreEqual(2, endpoints.DataSources.Count); // first = ControllerActionEndpointDataSource, Second = ModelEndpointDataSource (SignalR hubs)
            var route = endpoints.DataSources.First();
            Assert.AreEqual(4, route.Endpoints.Count);
            var hubs = endpoints.DataSources.Last();
            Assert.AreEqual(2, hubs.Endpoints.Count);
            
            AssertMinimalBackOfficeRoutes(route);

            var endpoint3 = (RouteEndpoint)route.Endpoints[2];
            var previewControllerName = ControllerExtensions.GetControllerName<PreviewController>();
            Assert.AreEqual($"umbraco/{previewControllerName.ToLowerInvariant()}/{{action}}/{{id?}}", endpoint3.RoutePattern.RawText);
            Assert.AreEqual(Constants.Web.Mvc.BackOfficeArea, endpoint3.RoutePattern.Defaults["area"]);
            Assert.AreEqual("Index", endpoint3.RoutePattern.Defaults["action"]);
            Assert.AreEqual(previewControllerName, endpoint3.RoutePattern.Defaults["controller"]);
            Assert.AreEqual(endpoint3.RoutePattern.Defaults["area"], typeof(PreviewController).GetCustomAttribute<AreaAttribute>(false).RouteValue);

            var endpoint4 = (RouteEndpoint)route.Endpoints[3];
            var apiControllerName = ControllerExtensions.GetControllerName<Testing1Controller>();
            Assert.AreEqual($"umbraco/backoffice/api/{apiControllerName.ToLowerInvariant()}/{{action}}/{{id?}}", endpoint4.RoutePattern.RawText);
            Assert.IsFalse(endpoint4.RoutePattern.Defaults.ContainsKey("area"));
            Assert.IsFalse(endpoint4.RoutePattern.Defaults.ContainsKey("action"));
            Assert.AreEqual(apiControllerName, endpoint4.RoutePattern.Defaults["controller"]);
        }

        private void AssertMinimalBackOfficeRoutes(EndpointDataSource route)
        {
            var endpoint1 = (RouteEndpoint)route.Endpoints[0];
            Assert.AreEqual($"umbraco/{{action}}/{{id?}}", endpoint1.RoutePattern.RawText);
            Assert.AreEqual(Constants.Web.Mvc.BackOfficeArea, endpoint1.RoutePattern.Defaults["area"]);
            Assert.AreEqual("Default", endpoint1.RoutePattern.Defaults["action"]);
            Assert.AreEqual(ControllerExtensions.GetControllerName<BackOfficeController>(), endpoint1.RoutePattern.Defaults["controller"]);
            Assert.AreEqual(endpoint1.RoutePattern.Defaults["area"], typeof(BackOfficeController).GetCustomAttribute<AreaAttribute>(false).RouteValue);

            var endpoint2 = (RouteEndpoint)route.Endpoints[1];
            var controllerName = ControllerExtensions.GetControllerName<AuthenticationController>();
            Assert.AreEqual($"umbraco/backoffice/{Constants.Web.Mvc.BackOfficeApiArea.ToLowerInvariant()}/{controllerName.ToLowerInvariant()}/{{action}}/{{id?}}", endpoint2.RoutePattern.RawText);
            Assert.AreEqual(Constants.Web.Mvc.BackOfficeApiArea, endpoint2.RoutePattern.Defaults["area"]);
            Assert.IsFalse(endpoint2.RoutePattern.Defaults.ContainsKey("action"));
            Assert.AreEqual(controllerName, endpoint2.RoutePattern.Defaults["controller"]);
            Assert.AreEqual(endpoint1.RoutePattern.Defaults["area"], typeof(BackOfficeController).GetCustomAttribute<AreaAttribute>(false).RouteValue);
        }

        private BackOfficeAreaRoutes GetBackOfficeAreaRoutes(RuntimeLevel level)
        {
            var globalSettings = new GlobalSettings();
            var routes = new BackOfficeAreaRoutes(
                Options.Create(globalSettings),
                Mock.Of<IHostingEnvironment>(x => x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
                Mock.Of<IRuntimeState>(x => x.Level == level),
                new UmbracoApiControllerTypeCollection(new[] { typeof(Testing1Controller) }));

            return routes;
        }

        [IsBackOffice]
        private class Testing1Controller : UmbracoApiController
        {

        }
    }
}
