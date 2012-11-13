using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
	[TestFixture]
	public class RouteParserTests
	{
		/// <summary>
		/// used for testing
		/// </summary>
		private class MyRoute : RouteBase
		{
			private readonly string _url;

			public MyRoute(string url)
			{
				_url = url;
			}

			public override RouteData GetRouteData(HttpContextBase httpContext)
			{
				throw new NotImplementedException();
			}

			public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void Get_Successfully_Parsed_Urls()
		{
			var urls = new RouteBase[]
				{
					new Route("Umbraco/RenderMvc/{action}/{id}", null),
					new Route("Another/Test/{controller}/{action}/{id}", null),
					new MyRoute("Umbraco/RenderMvc/{action}/{id}"), //wont match because its not a route object
					new Route("Another/Test/{id}/{controller}/{action}", null) //invalid
				};
			var valid = new string[]
				{
					"~/Umbraco/RenderMvc",
					"~/Another/Test"
				};

			var collection = new RouteCollection();
			foreach (var u in urls)
			{
				collection.Add(u);
			}
			var parser = new RouteParser(collection);

			var result = parser.ParsedVirtualUrlsFromRouteTable();

			Assert.AreEqual(2, result.Count());
			Assert.IsTrue(result.ContainsAll(valid));
		}

		[TestCase("Umbraco/RenderMvc/{action}/{id}", "~/Umbraco/RenderMvc")]
		[TestCase("Install/PackageInstaller/{action}/{id}", "~/Install/PackageInstaller")]
		[TestCase("Test/{controller}/{action}/{id}", "~/Test")]
		[TestCase("Another/Test/{controller}/{action}/{id}", "~/Another/Test")]
		public void Successful_Virtual_Path(string url, string result)
		{
			var route = new Route(url, null);
			var parser = new RouteParser(new RouteCollection());
			var attempt = parser.TryGetBaseVirtualPath(route);
			if (!attempt.Success && attempt.Error != null)
				throw attempt.Error; //throw this so we can see the error in the test output

			Assert.IsTrue(attempt.Success);
			Assert.AreEqual(result, attempt.Result);
		}

		[TestCase("Umbraco/RenderMvc/{action}/{controller}/{id}")]
		[TestCase("Install/PackageInstaller/{id}/{action}")]
		[TestCase("Test/{controller}/{id}/{action}")]
		[TestCase("Another/Test/{id}/{controller}/{action}")]
		public void Non_Parsable_Virtual_Path(string url)
		{
			var route = new Route(url, null);
			var parser = new RouteParser(new RouteCollection());
			var attempt = parser.TryGetBaseVirtualPath(route);
			Assert.IsFalse(attempt.Success);
		}

	}
}
