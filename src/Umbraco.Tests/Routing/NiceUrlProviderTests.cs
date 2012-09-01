using System.Configuration;
using NUnit.Framework;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class NiceUrlProviderTests : BaseRoutingTest
	{

		[TestCase(1046, "/home.aspx")]
		[TestCase(1173, "/home/sub1.aspx")]
		[TestCase(1174, "/home/sub1/sub2.aspx")]
		[TestCase(1176, "/home/sub1/sub-3.aspx")]
		[TestCase(1177, "/home/sub1/custom-sub-1.aspx")]
		[TestCase(1178, "/home/sub1/custom-sub-2.aspx")]
		[TestCase(1175, "/home/sub-2.aspx")]
		[TestCase(1172, "/test-page.aspx")]
		public void Get_Nice_Url_Not_Hiding_Top_Level_No_Directory_Urls(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);
			
			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

			Assert.AreEqual(niceUrlMatch, result);
		}

		[TestCase(1046, "/home")]
		[TestCase(1173, "/home/sub1")]
		[TestCase(1174, "/home/sub1/sub2")]
		[TestCase(1176, "/home/sub1/sub-3")]
		[TestCase(1177, "/home/sub1/custom-sub-1")]
		[TestCase(1178, "/home/sub1/custom-sub-2")]
		[TestCase(1175, "/home/sub-2")]
		[TestCase(1172, "/test-page")]
		public void Get_Nice_Url_Not_Hiding_Top_Level_With_Directory_Urls(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");

			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

			Assert.AreEqual(niceUrlMatch, result);
		}

		[TestCase(1046, "/")]
		[TestCase(1173, "/sub1.aspx")]
		[TestCase(1174, "/sub1/sub2.aspx")]
		[TestCase(1176, "/sub1/sub-3.aspx")]
		[TestCase(1177, "/sub1/custom-sub-1.aspx")]
		[TestCase(1178, "/sub1/custom-sub-2.aspx")]
		[TestCase(1175, "/sub-2.aspx")]
		[TestCase(1172, "/test-page.aspx")]
		public void Get_Nice_Url_Hiding_Top_Level_No_Directory_Urls(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "true");

			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

			Assert.AreEqual(niceUrlMatch, result);
		}

		[TestCase(1046, "/")]
		[TestCase(1173, "/sub1")]
		[TestCase(1174, "/sub1/sub2")]
		[TestCase(1176, "/sub1/sub-3")]
		[TestCase(1177, "/sub1/custom-sub-1")]
		[TestCase(1178, "/sub1/custom-sub-2")]
		[TestCase(1175, "/sub-2")]
		[TestCase(1172, "/test-page")]
		public void Get_Nice_Url_Hiding_Top_Level_With_Directory_Urls(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "true");
			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");

			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

			Assert.AreEqual(niceUrlMatch, result);
		}
	}
}