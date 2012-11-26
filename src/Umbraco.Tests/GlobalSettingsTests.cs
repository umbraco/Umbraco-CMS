using System.Configuration;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using System.Web.Mvc;

namespace Umbraco.Tests
{
	[TestFixture]
	public class GlobalSettingsTests : BaseWebTest
	{
		protected override bool RequiresDbSetup
		{
			get { return false; }
		}

		public override void Initialize()
		{
			base.Initialize();
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "~/umbraco,~/install/");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd");
		}

		public override void TearDown()
		{
			base.TearDown();
			//reset the app config		
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "");
		}

        [Test]
        public void Is_Version_From_Assembly_Correct()
        {
            Assert.That(Umbraco.Core.Configuration.GlobalSettings.Version.ToString(3), Is.EqualTo("6.0.0"));
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
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "");

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