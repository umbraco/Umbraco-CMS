using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class SerializerTypeCacheRefresherTests : UmbracoIntegrationTestWithContentEditing
{
    private const int MediaTypeId = 1234;

    private CountingSerializerFactory SerializerFactory
        => (CountingSerializerFactory)GetRequiredService<IContentCacheDataSerializerFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.Services.AddUnique<IContentCacheDataSerializerFactory>(provider =>
            new CountingSerializerFactory(
                ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(provider)));

    [Test]
    public void Refreshing_A_Content_Type_Clears_The_Serializer_Type_Cache_Without_Local_Type_Notifications()
    {
        // A server that did not save the type only runs the cache refresher; none of the notifications published
        // by the content type service reach it.
        ContentTypeCacheRefresher refresher = GetRequiredService<ContentTypeCacheRefresher>();
        ContentTypeCacheRefresher.JsonPayload[] payloads =
        [
            new(nameof(IContentType), ContentType.Id, ContentTypeChangeTypes.RefreshOther),
        ];
        var clearsBefore = SerializerFactory.ClearCount;

        refresher.RefreshInternal(payloads);
        refresher.Refresh(payloads);

        Assert.Greater(SerializerFactory.ClearCount, clearsBefore);
    }

    [Test]
    public void Refreshing_A_Media_Type_Clears_The_Serializer_Type_Cache_Without_Local_Type_Notifications()
    {
        ContentTypeCacheRefresher refresher = GetRequiredService<ContentTypeCacheRefresher>();
        ContentTypeCacheRefresher.JsonPayload[] payloads =
        [
            new(nameof(IMediaType), MediaTypeId, ContentTypeChangeTypes.RefreshOther),
        ];
        var clearsBefore = SerializerFactory.ClearCount;

        refresher.RefreshInternal(payloads);
        refresher.Refresh(payloads);

        Assert.Greater(SerializerFactory.ClearCount, clearsBefore);
    }

    [Test]
    public async Task Rebuilding_The_Document_Memory_Cache_By_Content_Type_Clears_The_Serializer_Type_Cache()
    {
        var clearsBefore = SerializerFactory.ClearCount;

        await GetRequiredService<IDocumentCacheService>().RebuildMemoryCacheByContentTypeAsync([ContentType.Id]);

        Assert.Greater(SerializerFactory.ClearCount, clearsBefore);
    }

    [Test]
    public async Task Rebuilding_The_Media_Memory_Cache_By_Content_Type_Clears_The_Serializer_Type_Cache()
    {
        var clearsBefore = SerializerFactory.ClearCount;

        await GetRequiredService<IMediaCacheService>().RebuildMemoryCacheByContentTypeAsync([MediaTypeId]);

        Assert.Greater(SerializerFactory.ClearCount, clearsBefore);
    }

    private sealed class CountingSerializerFactory : IContentCacheDataSerializerFactory
    {
        private readonly IContentCacheDataSerializerFactory _inner;
        private int _clearCount;

        public CountingSerializerFactory(IContentCacheDataSerializerFactory inner) => _inner = inner;

        public int ClearCount => _clearCount;

        public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types) => _inner.Create(types);

        public void ClearTypeCache()
        {
            Interlocked.Increment(ref _clearCount);
            _inner.ClearTypeCache();
        }
    }
}
