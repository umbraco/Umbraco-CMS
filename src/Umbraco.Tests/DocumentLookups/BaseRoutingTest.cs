using System.Configuration;
using System.Linq;
using System.Web.Routing;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.DocumentLookups
{
	[TestFixture, RequiresSTA]
	public abstract class BaseRoutingTest : BaseWebTest
	{		
		public override void Initialize()
		{						
		}

		public override void TearDown()
		{			
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