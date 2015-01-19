using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations
{
    [TestFixture]
	public class GlobalSettingsTests : BaseWebTest
	{

		public override void Initialize()
		{            
			base.Initialize();
            SettingsForTests.UmbracoPath = "~/umbraco";
		}

		public override void TearDown()
		{
            //ensure this is reset
		    SystemDirectories.Root = null;
            SettingsForTests.UmbracoPath = "~/umbraco";
            //reset the app config		            
			base.TearDown();
			
		}

        [Test]
        public void Is_Debug_Mode()
        {
            Assert.That(Umbraco.Core.Configuration.GlobalSettings.DebugMode, Is.EqualTo(true));
        }

        [Ignore]
        [Test]
        public void Is_Version_From_Assembly_Correct()
        {
            Assert.That(UmbracoVersion.Current.ToString(3), Is.EqualTo("6.0.0"));
        }

        [TestCase("~/umbraco", "/", "umbraco")]
        [TestCase("~/umbraco", "/MyVirtualDir", "umbraco")]
        [TestCase("~/customPath", "/MyVirtualDir/", "custompath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir", "some-wacky-nestedpath")]
        [TestCase("~/some-wacky/nestedPath", "/MyVirtualDir/NestedVDir/", "some-wacky-nestedpath")]
	    public void Umbraco_Mvc_Area(string path, string rootPath, string outcome)
        {
            SettingsForTests.UmbracoPath = path;
            SystemDirectories.Root = rootPath;
            Assert.AreEqual(outcome, Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea);
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
			Assert.IsTrue(Umbraco.Core.Configuration.GlobalSettings.IsReservedPathOrUrl(url));
		}

		[TestCase("/umbraco_client/Tree/treeIcons.css")]
		[TestCase("/umbraco_client/Tree/Themes/umbraco/style.css")]
		[TestCase("/umbraco_client/scrollingmenu/style.css")]		
		[TestCase("/base/somebasehandler")]
		[TestCase("/")]
		[TestCase("/home.aspx")]
		[TestCase("/umbraco-test")]
		[TestCase("/install-test")]
		[TestCase("/install.aspx")]
		public void Is_Not_Reserved_Path_Or_Url(string url)
		{
			Assert.IsFalse(Umbraco.Core.Configuration.GlobalSettings.IsReservedPathOrUrl(url));
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
		    Umbraco.Core.Configuration.GlobalSettings.ReservedPaths = "";
		    Umbraco.Core.Configuration.GlobalSettings.ReservedUrls = "";

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
				Umbraco.Core.Configuration.GlobalSettings.IsReservedPathOrUrl(url, context.HttpContext, routes));
		}
	}
}