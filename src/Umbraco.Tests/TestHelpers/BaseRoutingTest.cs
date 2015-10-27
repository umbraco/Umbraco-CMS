using System.Configuration;
using System.Linq;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.TestHelpers
{
	[TestFixture, RequiresSTA]
	public abstract class BaseRoutingTest : BaseWebTest
	{
	    ///  <summary>
	    ///  Return a new RoutingContext
	    ///  </summary>
	    ///  <param name="url"></param>
	    ///  <param name="templateId">
	    ///  The template Id to insert into the Xml cache file for each node, this is helpful for unit testing with templates but you		 
	    ///  should normally create the template in the database with this id
	    /// </param>
	    ///  <param name="routeData"></param>
	    /// <param name="setUmbracoContextCurrent">set to true to also set the singleton UmbracoContext.Current to the context created with this method</param>
	    /// <param name="umbracoSettings"></param>
	    /// <returns></returns>
	    protected RoutingContext GetRoutingContext(string url, int templateId, RouteData routeData = null, bool setUmbracoContextCurrent = false, IUmbracoSettingsSection umbracoSettings = null)
	    {
	        if (umbracoSettings == null) umbracoSettings = SettingsForTests.GetDefault();

			var umbracoContext = GetUmbracoContext(url, templateId, routeData);
            var urlProvider = new UrlProvider(umbracoContext, umbracoSettings.WebRouting, new IUrlProvider[]
            {
                new DefaultUrlProvider(umbracoSettings.RequestHandler)
            });
			var routingContext = new RoutingContext(
				umbracoContext,
				Enumerable.Empty<IContentFinder>(),
				new FakeLastChanceFinder(),
                urlProvider);

			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

	        if (setUmbracoContextCurrent)
	            UmbracoContext.Current = umbracoContext;

			return routingContext;
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