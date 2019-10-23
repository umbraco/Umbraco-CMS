﻿using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class RoutableDocumentFilterTests : BaseWebTest
    {
        [TestCase("/umbraco/editContent.aspx")]
        [TestCase("/install/default.aspx")]
        [TestCase("/install/")]
        [TestCase("/install")]
        [TestCase("/install/?installStep=asdf")]
        [TestCase("/install/test.aspx")]
        public void Is_Reserved_Path_Or_Url(string url)
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var routableDocFilter = new RoutableDocumentFilter(globalSettings);
            Assert.IsTrue(routableDocFilter.IsReservedPathOrUrl(url));
        }

        [TestCase("/base/somebasehandler")]
        [TestCase("/")]
        [TestCase("/home.aspx")]
        [TestCase("/umbraco-test")]
        [TestCase("/install-test")]
        [TestCase("/install.aspx")]
        public void Is_Not_Reserved_Path_Or_Url(string url)
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            var routableDocFilter = new RoutableDocumentFilter(globalSettings);
            Assert.IsFalse(routableDocFilter.IsReservedPathOrUrl(url));
        }

        [TestCase("/Do/Not/match", false)]
        [TestCase("/Umbraco/RenderMvcs", false)]
        [TestCase("/Umbraco/RenderMvc", true)]
        [TestCase("/Umbraco/RenderMvc/Index", true)]
        [TestCase("/Umbraco/RenderMvc/Index/1234", true)]
        [TestCase("/Umbraco/RenderMvc/Index/1234/9876", false)]
        [TestCase("/api", true)]
        [TestCase("/api/WebApiTest", true)]
        [TestCase("/api/WebApiTest/1234", true)]
        [TestCase("/api/WebApiTest/Index/1234", false)]
        public void Is_Reserved_By_Route(string url, bool shouldMatch)
        {
            //reset the app config, we only want to test routes not the hard coded paths
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.ReservedPaths).Returns("");
            globalSettingsMock.Setup(x => x.ReservedUrls).Returns("");

            var routableDocFilter = new RoutableDocumentFilter(globalSettingsMock.Object);

            var routes = new RouteCollection();

            routes.MapRoute(
                "Umbraco_default",
                "Umbraco/RenderMvc/{action}/{id}",
                new { controller = "RenderMvc", action = "Index", id = UrlParameter.Optional });
            routes.MapRoute(
                "WebAPI",
                "api/{controller}/{id}",
                new { controller = "WebApiTestController", action = "Index", id = UrlParameter.Optional });


            var context = new FakeHttpContextFactory(url);


            Assert.AreEqual(
                shouldMatch,
                routableDocFilter.IsReservedPathOrUrl(url, context.HttpContext, routes));
        }
    }
}
