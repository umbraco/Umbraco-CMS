using System.Web;
using System.Web.Routing;
using Moq;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Routing
{
	public static class RouteTestExtensions
	{

		/// <summary>
		/// Return the route data for the url based on a mocked context
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="requestUrl"></param>
		/// <returns></returns>
		public static RouteData GetDataForRoute(this RouteCollection routes, string requestUrl)
		{
			var context = new FakeHttpContextFactory(requestUrl);
			return routes.GetDataForRoute(context.HttpContext);
		}

		/// <summary>
		/// Get the route data based on the url and http context
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		public static RouteData GetDataForRoute(this RouteCollection routes, HttpContextBase httpContext)
		{
			var data = routes.GetRouteData(httpContext);

			//set the route data on the request context
		    var requestMock = Mock.Get(httpContext.Request.RequestContext);
            requestMock.Setup(x => x.RouteData).Returns(data);

			return data;
		}

		/// <summary>
		/// Checks if the URL will be ignored in the RouteTable
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		/// <remarks>
		/// MVCContrib has a similar one but is faulty:
		///  http://mvccontrib.codeplex.com/workitem/7173
		/// </remarks>
		public static bool ShouldIgnoreRoute(this string url)
		{
			var http = new FakeHttpContextFactory(url);
			var r = RouteTable.Routes.GetRouteData(http.HttpContext);
			if (r == null) return false;
			return (r.RouteHandler is StopRoutingHandler);
		}

	}
}