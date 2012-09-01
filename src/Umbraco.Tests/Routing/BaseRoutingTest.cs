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

		/// <summary>
		/// Return a new RoutingContext
		/// </summary>
		/// <param name="url"></param>
		/// <param name="templateId">
		/// The template Id to insert into the Xml cache file for each node, this is helpful for unit testing with templates but you		 
		/// should normally create the template in the database with this id
		///</param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, int templateId, RouteData routeData = null)
		{
			var umbracoContext = GetUmbracoContext(url, templateId, routeData);
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

		/// <summary>
		/// Return a new RoutingContext
		/// </summary>
		/// <param name="url"></param>
		/// <param name="template"></param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, Template template, RouteData routeData = null)
		{
			return GetRoutingContext(url, template.Id, routeData);
		}

		/// <summary>
		/// Return a new RoutingContext that doesn't require testing based on template
		/// </summary>
		/// <param name="url"></param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, RouteData routeData = null)
		{
			return GetRoutingContext(url, 1234, routeData);
		}

		
	}
}