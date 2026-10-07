using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ReloadPublishedCacheTests : UmbracoIntegrationTestWithContentEditing
{
    private IPublishedContentTypeCache PublishedContentTypeCache => GetRequiredService<IPublishedContentTypeCache>();

    private DistributedCache DistributedCache => GetRequiredService<DistributedCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();

        // The harness registers a no-op server messenger, which would swallow the refresh and let this test
        // pass without the cache refresher ever running.
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public void Reloading_The_Published_Cache_Clears_The_Published_Content_Type_Cache()
    {
        IPublishedContentType primed = PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key);
        Assert.IsNotNull(primed);

        // This is what the "Reload Memory Cache" backoffice action triggers. Reloading the published caches from
        // the database cache is only meaningful if the content types the content is projected through are
        // reloaded too, so the cached published content types have to go as well.
        DistributedCache.RefreshAllPublishedSnapshot();

        IPublishedContentType after = PublishedContentTypeCache.Get(PublishedItemType.Content, ContentType.Key);
        Assert.IsFalse(ReferenceEquals(primed, after), "the published content type should have been rebuilt, not re-served from the cache");
    }
}
