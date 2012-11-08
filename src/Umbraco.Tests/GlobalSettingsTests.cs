using System.Configuration;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

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
	}
}