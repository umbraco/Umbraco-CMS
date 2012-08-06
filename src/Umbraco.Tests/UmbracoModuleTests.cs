using System.Configuration;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests
{
	[TestFixture]
	public class UmbracoModuleTests
	{
		private UmbracoModule _module;

		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();
			ApplicationContext.Current = new ApplicationContext()
				{
					IsReady = true
				};
			_module = new UmbracoModule();
			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", Umbraco.Core.Configuration.GlobalSettings.CurrentVersion);
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "~/umbraco,~/install/");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "~/config/splashes/booting.aspx,~/install/default.aspx,~/config/splashes/noNodes.aspx,~/VSEnterpriseHelper.axd");
		}

		[TearDown]
		public void TearDown()
		{
			_module.Dispose();
			//reset the context on global settings
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = null;
			//reset the app context
			ApplicationContext.Current = null;
			//reset the app config
			ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedPaths", "");
			ConfigurationManager.AppSettings.Set("umbracoReservedUrls", "");
		}

		[TestCase("/umbraco_client/Tree/treeIcons.css", false)]
		[TestCase("/umbraco_client/Tree/Themes/umbraco/style.css?cdv=37", false)]
		[TestCase("/umbraco_client/scrollingmenu/style.css?cdv=37", false)]
		[TestCase("/umbraco/umbraco.aspx", false)]
		[TestCase("/umbraco/editContent.aspx", false)]
		[TestCase("/install/default.aspx", false)]
		[TestCase("/install/test.aspx", false)]
		[TestCase("/base/somebasehandler", false)]
		[TestCase("/", true)]
		[TestCase("/home.aspx", true)]
		public void Ensure_Request_Routable(string url, bool assert)
		{
			var httpContextFactory = new FakeHttpContextFactory(url);
			var httpContext = httpContextFactory.HttpContext;
			//set the context on global settings
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = httpContext;
			var uri = httpContext.Request.Url;
			var lpath = uri.AbsolutePath.ToLower();

			var result = _module.EnsureRequestRoutable(uri, lpath, httpContext);

			Assert.AreEqual(assert, result);
		}

	}
}