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
using Umbraco.Web.BackOffice.SignalR;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.Routing
{

    [TestFixture]
    public class PreviewRoutesTests
    {
        [TestCase(RuntimeLevel.Install)]
        [TestCase(RuntimeLevel.BootFailed)]
        [TestCase(RuntimeLevel.Unknown)]
        [TestCase(RuntimeLevel.Boot)]
        [TestCase(RuntimeLevel.Upgrade)]
        public void RuntimeState_No_Routes(RuntimeLevel level)
        {
            var routes = GetRoutes(level);
            var endpoints = new TestRouteBuilder();
            routes.CreateRoutes(endpoints);

            Assert.AreEqual(0, endpoints.DataSources.Count);
        }

     
        [Test]
        public void RuntimeState_Run()
        {
            var routes = GetRoutes(RuntimeLevel.Run);
            var endpoints = new TestRouteBuilder();
            routes.CreateRoutes(endpoints);

            Assert.AreEqual(1, endpoints.DataSources.Count);
            var route = endpoints.DataSources.First();
            Assert.AreEqual(2, route.Endpoints.Count);

            var endpoint0 = (RouteEndpoint) route.Endpoints[0];
            Assert.AreEqual($"{routes.GetPreviewHubRoute()}/negotiate", endpoint0.RoutePattern.RawText); 
            var endpoint1 = (RouteEndpoint) route.Endpoints[1];
            Assert.AreEqual($"{routes.GetPreviewHubRoute()}", endpoint1.RoutePattern.RawText);


        }
        private PreviewRoutes GetRoutes(RuntimeLevel level)
        {
            var globalSettings = new GlobalSettings();
            var routes = new PreviewRoutes(
                Options.Create(globalSettings),
                Mock.Of<IHostingEnvironment>(x =>
                    x.ToAbsolute(It.IsAny<string>()) == "/umbraco" && x.ApplicationVirtualPath == string.Empty),
                Mock.Of<IRuntimeState>(x => x.Level == level));

            return routes;
        }
    }
}
