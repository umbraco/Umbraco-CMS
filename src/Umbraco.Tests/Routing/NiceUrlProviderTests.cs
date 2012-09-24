using System;
using System.Configuration;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class NiceUrlProviderTests : BaseRoutingTest
	{

		public override void TearDown()
		{
			base.TearDown();

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "");
		}

		internal override IRoutesCache GetRoutesCache()
		{
			return new DefaultRoutesCache(false);
		}

		/// <summary>
		/// This checks that when we retreive a NiceUrl for multiple items that there are no issues with cache overlap 
		/// and that they are all cached correctly.
		/// </summary>
		[Test]
		public void Ensure_Cache_Is_Correct()
		{
			var routingContext = GetRoutingContext("/test", 1111);
			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");

			var samples = new Dictionary<int, string> {
				{ 1046, "/home" },
				{ 1173, "/home/sub1" },
				{ 1174, "/home/sub1/sub2" },
				{ 1176, "/home/sub1/sub-3" },
				{ 1177, "/home/sub1/custom-sub-1" },
				{ 1178, "/home/sub1/custom-sub-2" },
				{ 1175, "/home/sub-2" },
				{ 1172, "/test-page" }
			};

			foreach (var sample in samples)
			{
				var result = routingContext.NiceUrlProvider.GetNiceUrl(sample.Key);
				Assert.AreEqual(sample.Value, result);
			}

			var randomSample = new KeyValuePair<int, string>(1177, "/home/sub1/custom-sub-1");
			for (int i = 0; i < 5; i++)
			{
				var result = routingContext.NiceUrlProvider.GetNiceUrl(randomSample.Key);
				Assert.AreEqual(randomSample.Value, result);
			}

			var cachedRoutes = ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedRoutes();
			Assert.AreEqual(8, cachedRoutes.Count);

			foreach (var sample in samples)
			{
				Assert.IsTrue(cachedRoutes.ContainsKey(sample.Key));
				Assert.AreEqual(sample.Value, cachedRoutes[sample.Key]);
			}

			var cachedIds = ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedIds();
			Assert.AreEqual(8, cachedIds.Count);

			foreach (var sample in samples)
			{
				var key = sample.Value;
				Assert.IsTrue(cachedIds.ContainsKey(key));
				Assert.AreEqual(sample.Key, cachedIds[key]);
			}
		}

		//[TestCase(1046, "/home.aspx")]
		//[TestCase(1173, "/home/sub1.aspx")]
		//[TestCase(1174, "/home/sub1/sub2.aspx")]
		//[TestCase(1176, "/home/sub1/sub-3.aspx")]
		//[TestCase(1177, "/home/sub1/custom-sub-1.aspx")]
		//[TestCase(1178, "/home/sub1/custom-sub-2.aspx")]
		//[TestCase(1175, "/home/sub-2.aspx")]
		//[TestCase(1172, "/test-page.aspx")]
		//public void Get_Nice_Url_Not_Hiding_Top_Level_No_Directory_Urls(int nodeId, string niceUrlMatch)
		//{
		//    var routingContext = GetRoutingContext("/test", 1111);
			
		//    var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

		//    Assert.AreEqual(niceUrlMatch, result);
		//}

		// test hideTopLevelNodeFromPath false
		[TestCase(1046, "/home")]
		[TestCase(1173, "/home/sub1")]
		[TestCase(1174, "/home/sub1/sub2")]
		[TestCase(1176, "/home/sub1/sub-3")]
		[TestCase(1177, "/home/sub1/custom-sub-1")]
		[TestCase(1178, "/home/sub1/custom-sub-2")]
		[TestCase(1175, "/home/sub-2")]
		[TestCase(1172, "/test-page")]

		public void Get_Nice_Url_Not_Hiding_Top_Level(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false");

			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);
			Assert.AreEqual(niceUrlMatch, result);
		}

		// no need for umbracoUseDirectoryUrls test = should be handled by UriUtilityTests

		//[TestCase(1046, "/")]
		//[TestCase(1173, "/sub1.aspx")]
		//[TestCase(1174, "/sub1/sub2.aspx")]
		//[TestCase(1176, "/sub1/sub-3.aspx")]
		//[TestCase(1177, "/sub1/custom-sub-1.aspx")]
		//[TestCase(1178, "/sub1/custom-sub-2.aspx")]
		//[TestCase(1175, "/sub-2.aspx")]
		//[TestCase(1172, "/test-page.aspx")]
		//public void Get_Nice_Url_Hiding_Top_Level_No_Directory_Urls(int nodeId, string niceUrlMatch)
		//{
		//    var routingContext = GetRoutingContext("/test", 1111);

		//    ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "true");

		//     var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);

		//    Assert.AreEqual(niceUrlMatch, result);
		//}

		// test hideTopLevelNodeFromPath true
		[TestCase(1046, "/")]
		[TestCase(1173, "/sub1")]
		[TestCase(1174, "/sub1/sub2")]
		[TestCase(1176, "/sub1/sub-3")]
		[TestCase(1177, "/sub1/custom-sub-1")]
		[TestCase(1178, "/sub1/custom-sub-2")]
		[TestCase(1175, "/sub-2")]
		[TestCase(1172, "/test-page")] // not hidden because not first root

		public void Get_Nice_Url_Hiding_Top_Level(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "true");

			var result = routingContext.NiceUrlProvider.GetNiceUrl(nodeId);
			Assert.AreEqual(niceUrlMatch, result);
		}
	}
}