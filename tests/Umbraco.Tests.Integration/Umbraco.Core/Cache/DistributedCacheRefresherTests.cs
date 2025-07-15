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

    [Test]
    public void DistributedContentCacheRefresherClearsElementsCache()
    {
        var cacheKey = "test";
        PopulateCache("test");

        ContentCacheRefresher.Refresh([new ContentCacheRefresher.JsonPayload()]);

        Assert.IsNull(ElementsCache.Get(cacheKey));
    }

    [Test]
    public void DistributedMediaCacheRefresherClearsElementsCache()
    {
        var cacheKey = "test";
        PopulateCache("test");

        MediaCacheRefresher.Refresh([new MediaCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll)]);

        Assert.IsNull(ElementsCache.Get(cacheKey));
    }

    private void PopulateCache(string key)
    {
        ElementsCache.Get(key, () => new object());

        // Just making sure something is in the cache now.
        Assert.IsNotNull(ElementsCache.Get(key));
    }
}
