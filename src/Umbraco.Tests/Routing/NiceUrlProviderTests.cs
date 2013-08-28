using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class NiceUrlProviderTests : BaseRoutingTest
	{
        protected override void FreezeResolution()
        {
            SiteDomainHelperResolver.Current = new SiteDomainHelperResolver(new SiteDomainHelper());

            base.FreezeResolution();
        }

		/// <summary>
		/// This checks that when we retreive a NiceUrl for multiple items that there are no issues with cache overlap 
		/// and that they are all cached correctly.
        /// </summary>
		[Test]
		public void Ensure_Cache_Is_Correct()
		{
			var routingContext = GetRoutingContext("/test", 1111);
		    SettingsForTests.UseDirectoryUrls = true;
		    SettingsForTests.HideTopLevelNodeFromPath = false;
            SettingsForTests.AddTrailingSlash = false; // (cached routes have none)

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
				var result = routingContext.UrlProvider.GetUrl(sample.Key);
				Assert.AreEqual(sample.Value, result);
			}

			var randomSample = new KeyValuePair<int, string>(1177, "/home/sub1/custom-sub-1");
			for (int i = 0; i < 5; i++)
			{
				var result = routingContext.UrlProvider.GetUrl(randomSample.Key);
				Assert.AreEqual(randomSample.Value, result);
			}

            var cache = routingContext.UmbracoContext.ContentCache.InnerCache as PublishedContentCache;
            if (cache == null) throw new Exception("Unsupported IPublishedContentCache, only the Xml one is supported.");
            var cachedRoutes = cache.RoutesCache.GetCachedRoutes();
			Assert.AreEqual(8, cachedRoutes.Count);

			foreach (var sample in samples)
			{
				Assert.IsTrue(cachedRoutes.ContainsKey(sample.Key));
				Assert.AreEqual(sample.Value, cachedRoutes[sample.Key]);
			}

			var cachedIds = cache.RoutesCache.GetCachedIds();
			Assert.AreEqual(8, cachedIds.Count);

			foreach (var sample in samples)
			{
				var key = sample.Value;
				Assert.IsTrue(cachedIds.ContainsKey(key));
				Assert.AreEqual(sample.Key, cachedIds[key]);
			}
		}

		// test hideTopLevelNodeFromPath false
		[TestCase(1046, "/home/")]
		[TestCase(1173, "/home/sub1/")]
		[TestCase(1174, "/home/sub1/sub2/")]
		[TestCase(1176, "/home/sub1/sub-3/")]
		[TestCase(1177, "/home/sub1/custom-sub-1/")]
		[TestCase(1178, "/home/sub1/custom-sub-2/")]
		[TestCase(1175, "/home/sub-2/")]
		[TestCase(1172, "/test-page/")]
		public void Get_Nice_Url_Not_Hiding_Top_Level(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

		    SettingsForTests.UseDirectoryUrls = true;
		    SettingsForTests.HideTopLevelNodeFromPath = false;
		    SettingsForTests.UseDomainPrefixes = false;

			var result = routingContext.UrlProvider.GetUrl(nodeId);
			Assert.AreEqual(niceUrlMatch, result);
		}

		// no need for umbracoUseDirectoryUrls test = should be handled by UriUtilityTests

		// test hideTopLevelNodeFromPath true
		[TestCase(1046, "/")]
		[TestCase(1173, "/sub1/")]
		[TestCase(1174, "/sub1/sub2/")]
		[TestCase(1176, "/sub1/sub-3/")]
		[TestCase(1177, "/sub1/custom-sub-1/")]
		[TestCase(1178, "/sub1/custom-sub-2/")]
		[TestCase(1175, "/sub-2/")]
		[TestCase(1172, "/test-page/")] // not hidden because not first root
		public void Get_Nice_Url_Hiding_Top_Level(int nodeId, string niceUrlMatch)
		{
			var routingContext = GetRoutingContext("/test", 1111);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = true;
            SettingsForTests.UseDomainPrefixes = false;

			var result = routingContext.UrlProvider.GetUrl(nodeId);
			Assert.AreEqual(niceUrlMatch, result);
		}

		[Test]
		public void Get_Nice_Url_Relative_Or_Absolute()
		{
			var routingContext = GetRoutingContext("http://example.com/test", 1111);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false;

            SettingsForTests.UseDomainPrefixes = false;
			Assert.AreEqual("/home/sub1/custom-sub-1/", routingContext.UrlProvider.GetUrl(1177));

            SettingsForTests.UseDomainPrefixes = true;
			Assert.AreEqual("http://example.com/home/sub1/custom-sub-1/", routingContext.UrlProvider.GetUrl(1177));

            SettingsForTests.UseDomainPrefixes = false;
			routingContext.UrlProvider.Mode = UrlProviderMode.Absolute;
			Assert.AreEqual("http://example.com/home/sub1/custom-sub-1/", routingContext.UrlProvider.GetUrl(1177));
		}

		[Test]
		public void Get_Nice_Url_Unpublished()
		{
			var routingContext = GetRoutingContext("http://example.com/test", 1111);

            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false;

            SettingsForTests.UseDomainPrefixes = false;
			Assert.AreEqual("#", routingContext.UrlProvider.GetUrl(999999));
            SettingsForTests.UseDomainPrefixes = true;
			Assert.AreEqual("#", routingContext.UrlProvider.GetUrl(999999));
            SettingsForTests.UseDomainPrefixes = false;
			routingContext.UrlProvider.Mode = UrlProviderMode.Absolute;
			Assert.AreEqual("#", routingContext.UrlProvider.GetUrl(999999));
		}
	}
}