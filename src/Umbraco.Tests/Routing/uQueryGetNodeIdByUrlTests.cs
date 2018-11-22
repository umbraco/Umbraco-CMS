using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
	public class uQueryGetNodeIdByUrlTests : BaseRoutingTest
	{

        private IUmbracoSettingsSection _umbracoSettings;

		public override void Initialize()
		{
			base.Initialize();

            //generate new mock settings and assign so we can configure in individual tests
            _umbracoSettings = SettingsForTests.GenerateMockSettings();
            SettingsForTests.ConfigureSettings(_umbracoSettings);

			var url = "/test";
			
			var lookup = new Umbraco.Web.Routing.ContentFinderByNiceUrl();
			var lookups = new Umbraco.Web.Routing.IContentFinder[] { lookup };

			var t = Template.MakeNew("test", new User(0));

			var umbracoContext = GetUmbracoContext(url, t.Id);
            var urlProvider = new UrlProvider(umbracoContext, _umbracoSettings.WebRouting, new IUrlProvider[] { new DefaultUrlProvider() });
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

            var requestMock = Mock.Get(_umbracoSettings.RequestHandler);
            requestMock.Setup(x => x.UseDomainPrefixes).Returns(false);
		    
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

            var requestMock = Mock.Get(_umbracoSettings.RequestHandler);
            requestMock.Setup(x => x.UseDomainPrefixes).Returns(false);
            
			Assert.AreEqual(nodeId, global::umbraco.uQuery.GetNodeIdByUrl(url));
		}
	}
}
