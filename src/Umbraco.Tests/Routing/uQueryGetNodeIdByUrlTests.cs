using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using System.Configuration;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class uQueryGetNodeIdByUrlTests : BaseRoutingTest
	{
		internal override IRoutesCache GetRoutesCache()
		{
			return new DefaultRoutesCache(false);
		}

		public override void Initialize()
		{
			base.Initialize();

			var url = "/test";
			
			var lookup = new Umbraco.Web.Routing.ContentFinderByNiceUrl();
			var lookups = new Umbraco.Web.Routing.IContentFinder[] { lookup };

			var t = Template.MakeNew("test", new User(0));

			var umbracoContext = GetUmbracoContext(url, t.Id);
			var contentStore = new DefaultPublishedContentStore();
            var urlProvider = new UrlProvider(umbracoContext, contentStore, new IUrlProvider[] { new DefaultUrlProvider() });
			var routingContext = new RoutingContext(
				umbracoContext,
				lookups,
				new FakeLastChanceFinder(),
				contentStore,
                urlProvider,
                GetRoutesCache());

			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

			////assign the routing context back to the umbraco context
			//umbracoContext.RoutingContext = routingContext;
			Umbraco.Web.UmbracoContext.Current = routingContext.UmbracoContext;
		}


		[TestCase(1046, "/home")]
		[TestCase(1173, "/home/sub1")]
		[TestCase(1174, "/home/sub1/sub2")]
		[TestCase(1176, "/home/sub1/sub-3")]
		[TestCase(1177, "/home/sub1/custom-sub-1")]
		[TestCase(1178, "/home/sub1/custom-sub-2")]
		[TestCase(1175, "/home/sub-2")]
		[TestCase(1172, "/test-page")]

		public void GetNodeIdByUrl_Not_Hiding_Top_Level_Absolute(int nodeId, string url)
		{
		    SettingsForTests.UseDirectoryUrls = true;
		    SettingsForTests.HideTopLevelNodeFromPath = false;
		    SettingsForTests.UseDomainPrefixes = false;

			Assert.AreEqual(nodeId, global::umbraco.uQuery.GetNodeIdByUrl("http://example.com" + url));
		}

		[TestCase(1046, "/home")]
		[TestCase(1173, "/home/sub1")]
		[TestCase(1174, "/home/sub1/sub2")]
		[TestCase(1176, "/home/sub1/sub-3")]
		[TestCase(1177, "/home/sub1/custom-sub-1")]
		[TestCase(1178, "/home/sub1/custom-sub-2")]
		[TestCase(1175, "/home/sub-2")]
		[TestCase(1172, "/test-page")]

		public void GetNodeIdByUrl_Not_Hiding_Top_Level_Relative(int nodeId, string url)
		{
            SettingsForTests.UseDirectoryUrls = true;
            SettingsForTests.HideTopLevelNodeFromPath = false;
            SettingsForTests.UseDomainPrefixes = false;

			Assert.AreEqual(nodeId, global::umbraco.uQuery.GetNodeIdByUrl(url));
		}
	}
}
