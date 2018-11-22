using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Configuration;

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
			if (path.StartsWith("http://") || path.StartsWith("https://"))
				CreateContext(new Uri(path));
			else
				CreateContext(new Uri("http://mysite" + VirtualPathUtility.ToAbsolute(path, "/")));
		}

		[SecuritySafeCritical]
		public FakeHttpContextFactory(string path, RouteData routeData)
		{
			if (path.StartsWith("http://") || path.StartsWith("https://"))
				CreateContext(new Uri(path), routeData);
			else
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

            var requestContextMock = new Mock<RequestContext>();

            RequestContext = requestContextMock.Object;

            //Cookie collection
		    var cookieCollection = new HttpCookieCollection();
            cookieCollection.Add(new HttpCookie("UMB_UCONTEXT", "FBA996E7-D6BE-489B-B199-2B0F3D2DD826"));

		    //Request
            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(x => x.AppRelativeCurrentExecutionFilePath).Returns("~" + fullUrl.AbsolutePath);
			requestMock.Setup(x => x.PathInfo).Returns(string.Empty);
            requestMock.Setup(x => x.RawUrl).Returns(VirtualPathUtility.ToAbsolute("~" + fullUrl.AbsolutePath, "/"));
            requestMock.Setup(x => x.RequestContext).Returns(RequestContext);
            requestMock.Setup(x => x.Url).Returns(fullUrl);
            requestMock.Setup(x => x.ApplicationPath).Returns("/");
            requestMock.Setup(x => x.Cookies).Returns(cookieCollection);
            requestMock.Setup(x => x.ServerVariables).Returns(new NameValueCollection());
			var queryStrings = HttpUtility.ParseQueryString(fullUrl.Query);
            requestMock.Setup(x => x.QueryString).Returns(queryStrings);
            requestMock.Setup(x => x.Form).Returns(new NameValueCollection());

			//Cache
            var cacheMock = new Mock<HttpCachePolicyBase>();

			//Response 
			//var response = new FakeHttpResponse();
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string s) => s);
            responseMock.Setup(x => x.Cache).Returns(cacheMock.Object);

			//Server

			var serverMock = new Mock<HttpServerUtilityBase>();
			serverMock.Setup(x => x.MapPath(It.IsAny<string>())).Returns(Environment.CurrentDirectory);

            //User
		    var user = new Mock<IPrincipal>().Object;

			//HTTP Context

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(x => x.Cache).Returns(HttpRuntime.Cache);
            //note: foreach on Items should return DictionaryEntries!
            //httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());
		    httpContextMock.Setup(x => x.Items).Returns(new Hashtable());
            httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);
            httpContextMock.Setup(x => x.Server).Returns(serverMock.Object);
            httpContextMock.Setup(x => x.Response).Returns(responseMock.Object);
            httpContextMock.Setup(x => x.User).Returns(user);

            HttpContext = httpContextMock.Object;

            requestContextMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

			if (routeData != null)
			{
                requestContextMock.Setup(x => x.RouteData).Returns(routeData);
			}
		    
            
		}

	}
}
