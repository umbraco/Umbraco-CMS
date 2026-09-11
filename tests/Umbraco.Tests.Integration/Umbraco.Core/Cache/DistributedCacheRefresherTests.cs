using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Cache;

// We need to make sure that it's the distributed cache refreshers that refresh the elements cache
// see: https://github.com/umbraco/Umbraco-CMS/issues/18467
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
internal sealed class DistributedCacheRefresherTests : UmbracoIntegrationTest
{
    private IElementsCache ElementsCache => GetRequiredService<IElementsCache>();

    private ContentCacheRefresher ContentCacheRefresher => GetRequiredService<ContentCacheRefresher>();

    private MediaCacheRefresher MediaCacheRefresher => GetRequiredService<MediaCacheRefresher>();

    private ElementCacheRefresher ElementCacheRefresher => GetRequiredService<ElementCacheRefresher>();

    [Test]
    public void DistributedContentCacheRefresherClearsElementsCache()
    {
        var cacheKey = "test";
        PopulateCache("test");

        ContentCacheRefresher.RefreshInternal([new ContentCacheRefresher.JsonPayload()]);
        ContentCacheRefresher.Refresh([new ContentCacheRefresher.JsonPayload()]);

        Assert.That(ElementsCache.Get(cacheKey), Is.Null);
    }

    [Test]
    public void DistributedMediaCacheRefresherClearsElementsCache()
    {
        var cacheKey = "test";
        PopulateCache("test");

        MediaCacheRefresher.RefreshInternal([new MediaCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll)]);
        MediaCacheRefresher.Refresh([new MediaCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll)]);

        Assert.That(ElementsCache.Get(cacheKey), Is.Null);
    }

    [Test]
    public void DistributedElementCacheRefresherClearsElementsCache()
    {
        var cacheKey = "test";
        PopulateCache("test");

        ElementCacheRefresher.Refresh([new ElementCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll)]);

        Assert.That(ElementsCache.Get(cacheKey), Is.Null);
    }

    private void PopulateCache(string key)
    {
        ElementsCache.Get(key, () => new object());

        // Just making sure something is in the cache now.
        Assert.That(ElementsCache.Get(key), Is.Not.Null);
    }
}
