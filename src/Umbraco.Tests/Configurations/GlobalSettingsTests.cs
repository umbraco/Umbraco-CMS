using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations
{
    [TestFixture]
    public class GlobalSettingsTests : BaseWebTest
    {
        private string _root;

        public override void SetUp()
        {
            base.SetUp();
            _root = SystemDirectories.Root;
        }

        public override void TearDown()
        {
            base.TearDown();
            SystemDirectories.Root = _root;
        }

        [Test]
        public void Is_Debug_Mode()
        {
            Assert.That(GlobalSettings.DebugMode, Is.EqualTo(true));
        }

        [Ignore("fixme - ignored test")]
        [Test]
        public void Is_Version_From_Assembly_Correct()
        {
            Assert.That(UmbracoVersion.SemanticVersion, Is.EqualTo("6.0.0"));
        }

        [TestCase("~/umbraco", "/", "umbraco")]
        [TestCase("~/umbraco", "/MyVirtualDir", "umbraco")]
        [TestCase("~/customPath", "/MyVirtualDir/", "custompath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir", "some-wacky-nestedpath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir/NestedVDir/", "some-wacky-nestedpath")]
        public void Umbraco_Mvc_Area(string path, string rootPath, string outcome)
        {
            var globalSettingsMock = Mock.Get(TestObjects.GetGlobalSettings()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.Path).Returns(IOHelper.ResolveUrl(path));
            SettingsForTests.ConfigureSettings(globalSettingsMock.Object);

            SystemDirectories.Root = rootPath;
            Assert.AreEqual(outcome, Current.Config.Global().GetUmbracoMvcArea());
        }

        [TestCase("/umbraco/umbraco.aspx")]
        [TestCase("/umbraco/editContent.aspx")]
        [TestCase("/install/default.aspx")]
        [TestCase("/install/")]
        [TestCase("/install")]
        [TestCase("/install/?installStep=asdf")]
        [TestCase("/install/test.aspx")]
        [TestCase("/config/splashes/booting.aspx")]
        public void Is_Reserved_Path_Or_Url(string url)
        {
            var globalSettings = TestObjects.GetGlobalSettings();
            Assert.IsTrue(globalSettings.IsReservedPathOrUrl(url));
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
            Assert.IsFalse(globalSettings.IsReservedPathOrUrl(url));
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
            var globalSettingsMock = Mock.Get(TestObjects.GetGlobalSettings()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.ReservedPaths).Returns("");
            globalSettingsMock.Setup(x => x.ReservedUrls).Returns("");
            SettingsForTests.ConfigureSettings(globalSettingsMock.Object);

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
                globalSettingsMock.Object.IsReservedPathOrUrl(url, context.HttpContext, routes));
        }
    }
}
