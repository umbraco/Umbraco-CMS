using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using System.Configuration;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class uQueryGetNodeIdByUrlTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();

			var url = "/test";
			
			var lookup = new Umbraco.Web.Routing.ContentFinderByNiceUrl();
			var lookups = new Umbraco.Web.Routing.IContentFinder[] { lookup };

			var t = Template.MakeNew("test", new User(0));

			var umbracoContext = GetUmbracoContext(url, t.Id);
            var urlProvider = new UrlProvider(umbracoContext, new IUrlProvider[] { new DefaultUrlProvider() });
			var routingContext = new RoutingContext(
				umbracoContext,
				lookups,
				new FakeLastChanceFinder(),
                urlProvider);

			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

			////assign the routing context back to the umbraco context
			//umbracoContext.RoutingContext = routingContext;
			Umbraco.Web.UmbracoContext.Current = routingContext.UmbracoContext;
		}

        public override void TearDown()
        {
            Umbraco.Web.UmbracoContext.Current = null;
            base.TearDown();
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
            //mock the Umbraco settings that we need
            var settings = MockRepository.GenerateStub<IUmbracoSettings>();
            settings.Stub(x => x.RequestHandler.UseDomainPrefixes).Return(false);
            SettingsForTests.ConfigureSettings(settings);

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
            //mock the Umbraco settings that we need
            var settings = MockRepository.GenerateStub<IUmbracoSettings>();
            settings.Stub(x => x.RequestHandler.UseDomainPrefixes).Return(false);
            SettingsForTests.ConfigureSettings(settings);

			Assert.AreEqual(nodeId, global::umbraco.uQuery.GetNodeIdByUrl(url));
		}
	}
}
