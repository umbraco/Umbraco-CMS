using System.Configuration;
using System.Linq;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture, RequiresSTA]
	public abstract class BaseRoutingTest : BaseWebTest
	{		
		public override void Initialize()
		{					
			base.Initialize();
		}

		public override void TearDown()
		{			
			base.TearDown();

			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "");					
		}	

		protected RoutingContext GetRoutingContext(string url, Template template, RouteData routeData = null)
		{
			var umbracoContext = GetUmbracoContext(url, template, routeData);
			var contentStore = new XmlPublishedContentStore();
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			var routingRequest = new RoutingContext(
				umbracoContext,
				Enumerable.Empty<IDocumentLookup>(),
				new FakeLastChanceLookup(),
				contentStore,
				niceUrls);
			return routingRequest;
		}

		
	}
}