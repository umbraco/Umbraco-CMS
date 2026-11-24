// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Documents and media are cached under their keys in one HybridCache. These tests assert that each cache
/// resolves only its own entity type, and that looking a key up in the wrong cache doesn't hide the item from its own.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class HybridCacheEntityTypeIsolationTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private Microsoft.Extensions.Caching.Hybrid.HybridCache HybridCache => GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

    [Test]
    public async Task Document_Cache_Does_Not_Resolve_A_Cached_Media_Item()
    {
        Guid mediaKey = await CreateMedia();
        Assert.That(await MediaCacheService.GetByKeyAsync(mediaKey), Is.Not.Null);

        IPublishedContent? result = await DocumentCacheService.GetByKeyAsync(mediaKey, false);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Media_Cache_Does_Not_Resolve_A_Cached_Document()
    {
        Guid documentKey = await CreateAndPublishDocument();
        Assert.That(await DocumentCacheService.GetByKeyAsync(documentKey, false), Is.Not.Null);

        IPublishedContent? result = await MediaCacheService.GetByKeyAsync(documentKey);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Document_Cache_Batch_Resolves_Only_Documents_Among_Cached_Entries()
    {
        Guid documentKey = await CreateAndPublishDocument();
        Guid mediaKey = await CreateMedia();
        Assert.That(await DocumentCacheService.GetByKeyAsync(documentKey, false), Is.Not.Null);
        Assert.That(await MediaCacheService.GetByKeyAsync(mediaKey), Is.Not.Null);

        IReadOnlyList<IPublishedContent> result = await DocumentCacheService.GetByKeysAsync([mediaKey, documentKey], false);

        Assert.That(result.Select(x => x.Key), Is.EqualTo(new[] { documentKey }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Media_Remains_Resolvable_After_The_Document_Cache_Is_Asked_For_Its_Key(bool batched)
    {
        Guid mediaKey = await CreateMedia();
        await MediaCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        await LookUpInDocumentCache(mediaKey, false, batched);
        IPublishedContent? result = await MediaCacheService.GetByKeyAsync(mediaKey);

        Assert.That(result?.Key, Is.EqualTo(mediaKey));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Document_Cache_Caches_A_Miss_For_A_Key_No_Other_Cache_Reads(bool batched)
    {
        Guid contentTypeKey = await CreateContentType();

        await LookUpInDocumentCache(contentTypeKey, false, batched);

        Assert.That(await IsCached(contentTypeKey, false), Is.True);
    }

    [Test]
    public async Task Document_Cache_Caches_A_Preview_Miss_For_The_Key_Of_A_Media_Item()
    {
        Guid mediaKey = await CreateMedia();
        await MediaCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        await LookUpInDocumentCache(mediaKey, true, false);

        Assert.Multiple(async () =>
        {
            Assert.That(await IsCached(mediaKey, true), Is.True);
            Assert.That((await MediaCacheService.GetByKeyAsync(mediaKey))?.Key, Is.EqualTo(mediaKey));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Media_Cache_Caches_A_Miss_For_A_Key_No_Other_Cache_Reads(bool batched)
    {
        Guid contentTypeKey = await CreateContentType();

        await LookUpInMediaCache(contentTypeKey, batched);

        Assert.That(await IsCached(contentTypeKey, false), Is.True);
    }

    [Test]
    public async Task Clearing_The_Media_Cache_Removes_A_Cached_Miss()
    {
        Guid contentTypeKey = await CreateContentType();
        await LookUpInMediaCache(contentTypeKey, false);
        Assert.That(await IsCached(contentTypeKey, false), Is.True);

        await MediaCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        Assert.That(await IsCached(contentTypeKey, false), Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Document_Remains_Resolvable_After_The_Media_Cache_Is_Asked_For_Its_Key(bool batched)
    {
        Guid documentKey = await CreateAndPublishDocument();
        await DocumentCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        await LookUpInMediaCache(documentKey, batched);
        IPublishedContent? result = await DocumentCacheService.GetByKeyAsync(documentKey, false);

        Assert.That(result?.Key, Is.EqualTo(documentKey));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Missing_Key_Stays_Unresolved_In_Every_Cache(bool preview)
    {
        var missingKey = Guid.NewGuid();

        Assert.Multiple(async () =>
        {
            Assert.That(await DocumentCacheService.GetByKeyAsync(missingKey, preview), Is.Null);
            Assert.That(await MediaCacheService.GetByKeyAsync(missingKey), Is.Null);
        });
    }

    private async Task LookUpInDocumentCache(Guid key, bool preview, bool batched)
    {
        if (batched)
        {
            Assert.That(await DocumentCacheService.GetByKeysAsync([key], preview), Is.Empty);
        }
        else
        {
            Assert.That(await DocumentCacheService.GetByKeyAsync(key, preview), Is.Null);
        }
    }

    private async Task LookUpInMediaCache(Guid key, bool batched)
    {
        if (batched)
        {
            Assert.That(await MediaCacheService.GetByKeysAsync([key]), Is.Empty);
        }
        else
        {
            Assert.That(await MediaCacheService.GetByKeyAsync(key), Is.Null);
        }
    }

    private async Task<bool> IsCached(Guid key, bool preview)
        => await HybridCache.ExistsAsync<ContentCacheNode?>(preview ? $"{key}+draft" : $"{key}", CancellationToken.None);

    private async Task<Guid> CreateContentType()
    {
        ContentType contentType = ContentTypeBuilder.CreateBasicContentType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType.Key;
    }

    private async Task<Guid> CreateAndPublishDocument()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content content = ContentBuilder.CreateBasicContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);
        return content.Key;
    }

    private async Task<Guid> CreateMedia()
    {
        IMediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        Media media = MediaBuilder.CreateSimpleMedia(mediaType, "Test Media", Constants.System.Root);
        MediaService.Save(media);
        return media.Key;
    }
}
