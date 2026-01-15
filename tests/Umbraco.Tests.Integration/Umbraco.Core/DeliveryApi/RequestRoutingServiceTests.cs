using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerFixture,
    WithApplication = true)]
public class RequestRoutingServiceTests : UmbracoIntegrationTest
{
    private IRequestRoutingService RequestRoutingService => GetRequiredService<IRequestRoutingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IRequestRoutingService, RequestRoutingService>();

        var elementCache = new FastDictionaryAppCache();
        var snapshotCache = new FastDictionaryAppCache();

        var domainCacheMock = new Mock<IDomainCache>();
        domainCacheMock.Setup(x => x.GetAll(It.IsAny<bool>()))
            .Returns(
            [
                new Domain(1, "localhost/en", 1000, "en-us", false, 0),
                new Domain(2, "localhost/jp", 1000, "ja-jp", false, 1),
            ]);
        builder.Services.AddSingleton(provider => domainCacheMock.Object);
    }

    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("/", "/")]
    [TestCase("/en/test/", "1000/test/")]       // Verifies matching a domain.
    [TestCase("/da/test/", "/da/test/")]        // Verifies that with no matching domain, so route will be returned as is.
    [TestCase("/jp/オフィス/", "1000/オフィス/")] // Verifies that with a URL segment containing special characters, the route remains decoded.
    public void GetContentRoute_ReturnsExpectedRoute(string? requestedRoute, string expectedResult)
    {
        if (!string.IsNullOrEmpty(requestedRoute))
        {
            var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

            httpContextAccessor.HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost"),
                    Path = requestedRoute,
                },
            };
        }

        var result = RequestRoutingService.GetContentRoute(requestedRoute);
        Assert.AreEqual(expectedResult, result);
    }
}
