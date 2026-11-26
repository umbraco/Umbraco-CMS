using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementPropertyCompressionTests : UmbracoIntegrationTest
{
    private const string PropertyAlias = "title";

    private const string PropertyValue = "A value long enough to be worth compressing";

    private readonly SwitchablePropertyCacheCompressionOptions _compressionOptions = new();

    private IContentType _elementType = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique<IPropertyCacheCompressionOptions>(_compressionOptions);
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IDatabaseCacheRepository DatabaseCacheRepository => GetRequiredService<IDatabaseCacheRepository>();

    private IContentCacheDataSerializerFactory SerializerFactory => GetRequiredService<IContentCacheDataSerializerFactory>();

    private IElementCacheService ElementCacheService => GetRequiredService<IElementCacheService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    [SetUp]
    public async Task SetUpTest()
    {
        // The fixture instance is shared by the tests, so a test that enabled compression must not leak into the next.
        _compressionOptions.Enabled = false;

        _elementType = ContentTypeBuilder.CreateSimpleElementType();
        _elementType.AllowedInLibrary = true;
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);
    }

    [Test]
    public async Task Element_Properties_Are_Compressed_When_The_Compression_Options_Ask_For_It()
    {
        EnableCompression();

        Guid key = await CreateAndPublishElement();

        Assert.That(await ReadStoredValue(key), Is.InstanceOf<LazyCompressedString>());
        Assert.That(await ReadPublishedValue(key), Is.EqualTo(PropertyValue));
    }

    [Test]
    public async Task Elements_Stored_Without_Compression_Stay_Readable_When_Compression_Is_Enabled_Afterwards()
    {
        Guid key = await CreateAndPublishElement();
        Assert.That(await ReadStoredValue(key), Is.EqualTo(PropertyValue), "Precondition: stored without compression");

        EnableCompression();

        Assert.That(await ReadStoredValue(key), Is.EqualTo(PropertyValue));
        Assert.That(await ReadPublishedValue(key), Is.EqualTo(PropertyValue));
    }

    [Test]
    public async Task Elements_Stored_Without_Compression_Are_Compressed_When_They_Are_Saved_Again_After_Compression_Is_Enabled()
    {
        Guid untouchedKey = await CreateAndPublishElement();
        Guid resavedKey = await CreateAndPublishElement();

        EnableCompression();
        await UpdateAndPublishElement(resavedKey);

        Assert.Multiple(async () =>
        {
            Assert.That(await ReadStoredValue(resavedKey), Is.InstanceOf<LazyCompressedString>(), "Saved again");
            Assert.That(await ReadStoredValue(untouchedKey), Is.EqualTo(PropertyValue), "Not saved again");
            Assert.That(await ReadPublishedValue(resavedKey), Is.EqualTo(PropertyValue));
            Assert.That(await ReadPublishedValue(untouchedKey), Is.EqualTo(PropertyValue));
        });
    }

    [Test]
    public async Task A_Full_Rebuild_Compresses_Elements_Stored_Without_Compression_After_Compression_Is_Enabled()
    {
        Guid key = await CreateAndPublishElement();

        EnableCompression();
        Attempt<DatabaseCacheRebuildResult> rebuild = await GetRequiredService<IDatabaseCacheRebuilder>().RebuildAsync(useBackgroundThread: false);
        Assert.That(rebuild.Success, Is.True);

        Assert.That(await ReadStoredValue(key), Is.InstanceOf<LazyCompressedString>());
        Assert.That(await ReadPublishedValue(key), Is.EqualTo(PropertyValue));
    }

    // The compression decision is memoised in the serializer factory's type snapshot, so enabling compression at
    // runtime is only picked up once the snapshot is replaced, which a content type change does.
    private void EnableCompression()
    {
        _compressionOptions.Enabled = true;
        SerializerFactory.ClearTypeCache();
    }

    private async Task<Guid> CreateAndPublishElement()
    {
        var createResult = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                Variants = [new() { Name = Guid.NewGuid().ToString() }],
                Properties = [new PropertyValueModel { Alias = PropertyAlias, Value = PropertyValue }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        Guid key = createResult.Result.Content!.Key;
        await Publish(key);
        return key;
    }

    private async Task UpdateAndPublishElement(Guid key)
    {
        var updateResult = await ElementEditingService.UpdateAsync(
            key,
            new ElementUpdateModel
            {
                Variants = [new() { Name = Guid.NewGuid().ToString() }],
                Properties = [new PropertyValueModel { Alias = PropertyAlias, Value = PropertyValue }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);

        await Publish(key);
    }

    private async Task Publish(Guid key)
    {
        var publishResult = await ElementPublishingService.PublishAsync(
            key,
            [new CulturePublishScheduleModel { Culture = "*", Schedule = null }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
    }

    private async Task<object?> ReadStoredValue(Guid key)
    {
        ContentCacheNode? node = await DatabaseCacheRepository.GetElementSourceAsync(key);
        Assert.That(node?.Data, Is.Not.Null);
        return node!.Data!.Properties[PropertyAlias].Single().Value;
    }

    private async Task<object?> ReadPublishedValue(Guid key)
    {
        await ElementCacheService.RebuildMemoryCacheByContentTypeAsync([_elementType.Id]);

        IPublishedElement? element = await PublishedElementCache.GetByIdAsync(key);
        Assert.That(element, Is.Not.Null);
        return element!.Value(PropertyAlias);
    }

    private sealed class SwitchablePropertyCacheCompressionOptions : IPropertyCacheCompressionOptions
    {
        public bool Enabled { get; set; }

        public bool IsCompressed(IReadOnlyContentBase content, IPropertyType propertyType, IDataEditor dataEditor, bool published)
            => Enabled;
    }
}
