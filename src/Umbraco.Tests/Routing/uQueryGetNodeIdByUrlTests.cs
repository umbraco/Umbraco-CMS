using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using System.Configuration;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class uQueryGetNodeIdByUrlTests : BaseRoutingTest
	{
		protected RoutingContext GetRoutingContext()
		{
			var url = "/test";
			var templateId = 1111;

			var lookup = new Umbraco.Web.Routing.LookupByNiceUrl();
			var lookups = new Umbraco.Web.Routing.IDocumentLookup[] { lookup };

			var umbracoContext = GetUmbracoContext(url, templateId, null);
			var contentStore = new Umbraco.Web.DefaultPublishedContentStore();
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			var routingContext = new RoutingContext(
				umbracoContext,
				lookups,
				new Umbraco.Tests.Stubs.FakeLastChanceLookup(),
				contentStore,
				niceUrls);

			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

			return routingContext;
		}

		[TestCase(1046, "/home")]
		[TestCase(1173, "/home/sub1")]
		[TestCase(1174, "/home/sub1/sub2")]
		[TestCase(1176, "/home/sub1/sub-3")]
		[TestCase(1177, "/home/sub1/custom-sub-1")]
		[TestCase(1178, "/home/sub1/custom-sub-2")]
		[TestCase(1175, "/home/sub-2")]
		[TestCase(1172, "/test-page")]

		public void GetNodeIdByUrl_Not_Hiding_Top_Level(int nodeId, string url)
		{
			var routingContext = GetRoutingContext();
			Umbraco.Web.UmbracoContext.Current = routingContext.UmbracoContext;

			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", "true");
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "false");
			Umbraco.Core.Configuration.UmbracoSettings.UseDomainPrefixes = false;

			Assert.AreEqual(nodeId, global::umbraco.uQuery.GetNodeIdByUrl("http://example.com" + url));
		}
	}
}
