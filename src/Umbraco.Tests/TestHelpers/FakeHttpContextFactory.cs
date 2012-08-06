using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Routing;
using Rhino.Mocks;

namespace Umbraco.Tests.TestHelpers
{
	/// <summary>
	/// Creates a mock http context with supporting other contexts to test against
	/// </summary>
	public class FakeHttpContextFactory
	{

		[SecuritySafeCritical]
		public FakeHttpContextFactory(Uri fullUrl)
		{
			CreateContext(fullUrl);
		}

		[SecuritySafeCritical]
		public FakeHttpContextFactory(string path)
		{
			CreateContext(new Uri("http://mysite" + VirtualPathUtility.ToAbsolute(path, "/")));
		}

		[SecuritySafeCritical]
		public FakeHttpContextFactory(string path, RouteData routeData)
		{
			CreateContext(new Uri("http://mysite" + VirtualPathUtility.ToAbsolute(path, "/")), routeData);
		}

		public HttpContextBase HttpContext { get; private set; }
		public RequestContext RequestContext { get; private set; }

		/// <summary>
		/// Mocks the http context to test against
		/// </summary>
		/// <param name="fullUrl"></param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		private void CreateContext(Uri fullUrl, RouteData routeData = null)
		{
			//Request context

			RequestContext = MockRepository.GenerateMock<RequestContext>();

			//Request

			var request = MockRepository.GenerateMock<HttpRequestBase>();
			request.Stub(x => x.AppRelativeCurrentExecutionFilePath).Return("~" + fullUrl.AbsolutePath);
			request.Stub(x => x.PathInfo).Return(string.Empty);
			request.Stub(x => x.RawUrl).Return(VirtualPathUtility.ToAbsolute("~" + fullUrl.AbsolutePath, "/"));
			request.Stub(x => x.RequestContext).Return(RequestContext);
			request.Stub(x => x.Url).Return(fullUrl);
			request.Stub(x => x.ApplicationPath).Return("/");
			request.Stub(x => x.Cookies).Return(new HttpCookieCollection());
			request.Stub(x => x.ServerVariables).Return(new NameValueCollection());
			var queryStrings = HttpUtility.ParseQueryString(fullUrl.Query);
			request.Stub(x => x.QueryString).Return(queryStrings);
			request.Stub(x => x.Form).Return(new NameValueCollection());

			//Cache
			var cache = MockRepository.GenerateMock<HttpCachePolicyBase>();

			//Response 
			//var response = new FakeHttpResponse();
			var response = MockRepository.GenerateMock<HttpResponseBase>();
			response.Stub(x => x.ApplyAppPathModifier(null)).IgnoreArguments().Do(new Func<string, string>(appPath => appPath));
			response.Stub(x => x.Cache).Return(cache);

			//Server

			var server = MockRepository.GenerateStub<HttpServerUtilityBase>();
			server.Stub(x => x.MapPath(Arg<string>.Is.Anything)).Return(Environment.CurrentDirectory);

			//HTTP Context

			HttpContext = MockRepository.GenerateMock<HttpContextBase>();
			HttpContext.Stub(x => x.Cache).Return(HttpRuntime.Cache);
			HttpContext.Stub(x => x.Items).Return(new Dictionary<object, object>());
			HttpContext.Stub(x => x.Request).Return(request);
			HttpContext.Stub(x => x.Server).Return(server);
			HttpContext.Stub(x => x.Response).Return(response);

			RequestContext.Stub(x => x.HttpContext).Return(HttpContext);

			if (routeData != null)
			{
				RequestContext.Stub(x => x.RouteData).Return(routeData);
			}
		}

	}
}
