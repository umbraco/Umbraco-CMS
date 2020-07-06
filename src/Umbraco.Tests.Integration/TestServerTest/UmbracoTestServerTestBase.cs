
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Composing;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Editors;


namespace Umbraco.Tests.Integration.TestServerTest
{
    [TestFixture]
    public abstract class UmbracoTestServerTestBase : UmbracoIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            Factory = new UmbracoWebApplicationFactory(TestDBConnectionString);
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions(){
                AllowAutoRedirect = false
            });
            LinkGenerator = Factory.Services.GetRequiredService<LinkGenerator>();

        }

        /// <summary>
        /// Get the service from the underlying container that is also used by the <see cref="Client"/>.
        /// </summary>
        protected T GetRequiredService<T>() => Factory.Services.GetRequiredService<T>();

        /// <summary>
        /// Prepare a url before using <see cref="Client"/>.
        /// This returns the url but also sets the HttpContext.request into to use this url.
        /// </summary>
        /// <returns>The string URL of the controller action.</returns>
        protected string PrepareUrl<T>(Expression<Func<T, object>> methodSelector)
            where T : UmbracoApiController
        {
            var url = LinkGenerator.GetUmbracoApiService<T>(methodSelector);

            var umbracoContextFactory = GetRequiredService<IUmbracoContextFactory>();
            var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

            httpContextAccessor.HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost", 80),
                    Path = url,
                    QueryString = new QueryString(string.Empty)
                }
            };

            umbracoContextFactory.EnsureUmbracoContext();

            return url;
        }

        protected HttpClient Client { get; set; }
        protected LinkGenerator LinkGenerator { get; set; }
        protected UmbracoWebApplicationFactory Factory { get; set; }

        [TearDown]
        public void TearDown()
        {

            Factory.Dispose();

            if (Current.IsInitialized)
            {
                Current.IsInitialized = false;
            }

        }
    }
}
