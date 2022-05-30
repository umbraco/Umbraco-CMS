using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Website.Routing;

[TestFixture]
public class SurfaceControllerTests : UmbracoTestServerTestBase
{
    [Test]
    public async Task Auto_Routes_For_Default_Action()
    {
        var url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.Index());

        // Act
        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task Auto_Routes_For_Custom_Action()
    {
        var url = PrepareSurfaceControllerUrl<TestSurfaceController>(x => x.News());

        // Act
        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Test]
    public async Task Plugin_Controller_Routes_By_Area()
    {
        // Create URL manually, because PrepareSurfaceController URl will prepare whatever the controller is routed as
        var controllerType = typeof(TestPluginController);
        var pluginAttribute =
            CustomAttributeExtensions.GetCustomAttribute<PluginControllerAttribute>(controllerType, false);
        var controllerName = ControllerExtensions.GetControllerName(controllerType);
        var url = $"/umbraco/{pluginAttribute?.AreaName}/{controllerName}";
        PrepareUrl(url);

        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}

// Test controllers must be non-nested, else we need to jump through some hoops with custom
// IApplicationFeatureProvider<ControllerFeature>
// For future notes if we want this, some example code of this is here
// https://tpodolak.com/blog/2020/06/22/asp-net-core-adding-controllers-directly-integration-tests/
public class TestSurfaceController : SurfaceController
{
    public TestSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider)
        : base(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider)
    {
    }

    public IActionResult Index() => Ok();

    public IActionResult News() => NoContent();
}

[PluginController("TestArea")]
public class TestPluginController : SurfaceController
{
    public TestPluginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider)
        : base(
            umbracoContextAccessor,
            databaseFactory,
            services,
            appCaches,
            profilingLogger,
            publishedUrlProvider)
    {
    }

    public IActionResult Index() => Ok();
}
