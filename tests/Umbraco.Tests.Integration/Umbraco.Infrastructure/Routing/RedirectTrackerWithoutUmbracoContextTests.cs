using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Routing;

/// <summary>
/// Verifies that <see cref="IRedirectTracker"/> resolves routes when called outside of a request (e.g. from a background
/// job such as an Umbraco Deploy restore), where no ambient Umbraco context exists.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RedirectTrackerWithoutUmbracoContextTests : UmbracoIntegrationTestWithContent
{
    private IDocumentUrlService DocumentUrlService => GetRequiredService<IDocumentUrlService>();

    private IRedirectTracker RedirectTracker => GetRequiredService<IRedirectTracker>();

    private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Run the cache refreshers in-process, so published routes are resolvable (the default Umbraco context accessor is kept, so no context exists).
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();

        // The test host's HTTP context has no host, which makes request URLs invalid.
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        builder.Services.AddUnique(Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext));
    }

    public override void Setup()
    {
        DocumentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
        base.Setup();

        ContentService.Publish(Textpage, ["*"]);
        ContentService.Publish(Subpage, ["*"]);
    }

    [Test]
    public void Can_Store_Old_Route()
    {
        Assert.IsFalse(GetRequiredService<IUmbracoContextAccessor>().TryGetUmbracoContext(out _), "Precondition: no ambient Umbraco context.");
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes = [];

        RedirectTracker.StoreOldRoute(Subpage, oldRoutes, isMove: true);

        Assert.AreEqual(1, oldRoutes.Count);
        Assert.AreEqual((Subpage.Key, "/text-page-1"), oldRoutes.Values.Single());
    }

    [Test]
    public void Can_Create_Redirects()
    {
        Assert.IsFalse(GetRequiredService<IUmbracoContextAccessor>().TryGetUmbracoContext(out _), "Precondition: no ambient Umbraco context.");
        var oldRoutes = new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
        {
            [(Subpage.Id, string.Empty)] = (Subpage.Key, "/old-route"),
        };

        RedirectTracker.CreateRedirects(oldRoutes);

        var redirect = RedirectUrlService.GetContentRedirectUrls(Subpage.Key).Single();
        Assert.AreEqual("/old-route", redirect.Url);
    }
}
