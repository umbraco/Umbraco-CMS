using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
public abstract class PublishedUrlInfoProviderTestsBase : UmbracoIntegrationTestWithContent
{
    protected IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    protected IPublishedUrlInfoProvider PublishedUrlInfoProvider => GetRequiredService<IPublishedUrlInfoProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();
        builder.Services.AddUnique<IUmbracoContextAccessor>(serviceProvider => new TestUmbracoContextAccessor(GetUmbracoContext(serviceProvider)));
        builder.Services.AddUnique(CreateHttpContextAccessor());
    }

    public override void Setup()
    {
        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        base.Setup();
    }

    private IUmbracoContext GetUmbracoContext(IServiceProvider serviceProvider)
    {
        var mock = new Mock<IUmbracoContext>();

        mock.Setup(x => x.Content).Returns(serviceProvider.GetRequiredService<IPublishedContentCache>());
        mock.Setup(x => x.CleanedUmbracoUrl).Returns(new Uri("https://localhost:44339"));

        return mock.Object;
    }

    private IHttpContextAccessor CreateHttpContextAccessor()
    {
        var mock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");

        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock.Object;
    }
}
