using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheBoundedTests : UmbracoIntegrationTestWithMediaEditing
{
    private const int MaximumItems = 5;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.Configure<CacheSettings>(options => options.Entry.Media.MaximumLocalCacheItems = MaximumItems);
    }

    private IPublishedMediaCache PublishedMediaHybridCache => GetRequiredService<IPublishedMediaCache>();

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    [Test]
    public async Task Bounded_L0_Cache_Evicts_Yet_Still_Serves_All_Media()
    {
        const int mediaCount = 30;
        var keys = await CreateMediaAsync(mediaCount);

        // Populate the L0 cache by requesting every media item through the published cache.
        foreach (Guid key in keys)
        {
            Assert.That(await PublishedMediaHybridCache.GetByIdAsync(key), Is.Not.Null);
        }

        var reporter = (IMemoryCacheSizeReporter)MediaCacheService;

        // The bounded L0 cache evicted down to its configured maximum (eviction is applied asynchronously by
        // the eviction policy, so poll for the count to settle).
        Assert.That(
            () => reporter.GetApproximateCount(),
            Is.LessThanOrEqualTo(MaximumItems).After(5000, 200),
            "the bounded L0 cache should evict down to its configured maximum rather than retain every requested item");

        // ...and every media item still resolves correctly after eviction (cache miss -> re-fetch -> re-convert).
        foreach (Guid key in keys)
        {
            IPublishedContent? media = await PublishedMediaHybridCache.GetByIdAsync(key);
            Assert.That(media, Is.Not.Null, $"media '{key}' should still resolve after eviction");
        }
    }

    private async Task<List<Guid>> CreateMediaAsync(int count)
    {
        var keys = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            MediaCreateModel media = MediaEditingBuilder.CreateSimpleMedia(CustomMediaType.Key, $"Bounded Media {i}", null);
            var created = await MediaEditingService.CreateAsync(media, Constants.Security.SuperUserKey);
            Assert.IsTrue(created.Success);
            keys.Add(media.Key!.Value);
        }

        return keys;
    }
}
