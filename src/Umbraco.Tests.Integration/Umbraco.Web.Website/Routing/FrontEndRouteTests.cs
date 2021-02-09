using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.TestServerTest;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Tests.Integration.Umbraco.Web.Website.Routing
{
    [TestFixture]
    public class SurfaceControllerTests : UmbracoTestServerTestBase
    {
        [Test]
        public async Task Auto_Routes_For_Default_Action()
        {
            string url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.Index());

            // Act
            HttpResponseMessage response = await Client.GetAsync(url);

            string body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Auto_Routes_For_Custom_Action()
        {
            string url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.News());

            // Act
            HttpResponseMessage response = await Client.GetAsync(url);

            string body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    // Test controllers must be non-nested, else we need to jump through some hoops with custom
    // IApplicationFeatureProvider<ControllerFeature>
    // For future notes if we want this, some example code of this is here
    // https://tpodolak.com/blog/2020/06/22/asp-net-core-adding-controllers-directly-integration-tests/
    public class TestSurfaceController : SurfaceController
    {
        public TestSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
        }

        public IActionResult Index() => Ok();

        public IActionResult News() => Forbid();
    }
}
