using System;
using System.Configuration;
using NUnit.Framework;
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
			var ids = new[]
				{
					new Tuple<int, string>(1046, "/home"),
					new Tuple<int, string>(1173, "/home/sub1"),
					new Tuple<int, string>(1174, "/home/sub1/sub2"),
					new Tuple<int, string>(1176, "/home/sub1/sub-3"),
					new Tuple<int, string>(1177, "/home/sub1/custom-sub-1"),
					new Tuple<int, string>(1178, "/home/sub1/custom-sub-2"),
					new Tuple<int, string>(1175, "/home/sub-2"),
					new Tuple<int, string>(1172, "/test-page")
				};
			foreach(var i in ids)
			{
				var result = routingContext.NiceUrlProvider.GetNiceUrl(i.Item1);
				Assert.AreEqual(i.Item2, result);
			}
			Assert.AreEqual(8, ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedRoutes().Count);
			Assert.AreEqual(8, ((DefaultRoutesCache)routingContext.UmbracoContext.RoutesCache).GetCachedIds().Count);
		}

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