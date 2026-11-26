using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Full-stack coverage of the picker property value converters against a cold cache: converts the value of a picker
/// property that points at several documents and media items, through the real converter, cache and database, and
/// asserts that the picked items are read in one batch rather than one row per item.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PickerValueConvertersColdReadTests : UmbracoIntegrationTestWithContent
{
    private const int PickedItemCount = 6;
    private const string DefaultCulture = "en-US";
    private const string PickerAlias = "picker";

    private CountingDatabaseCacheRepository _countingRepository = null!;

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IMediaCacheService MediaCacheService => GetRequiredService<IMediaCacheService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IPublishedValueFallback PublishedValueFallback => GetRequiredService<IPublishedValueFallback>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Deliver cache refreshers synchronously in-process (the harness default is a no-op messenger — see
        // tests/Umbraco.Tests.Integration/CLAUDE.md).
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();

        // A deterministic variation-context accessor whose set culture persists without a request.
        builder.Services.AddUnique<IVariationContextAccessor, TestVariationContextAccessor>();

        // Disable seeding so a cleared cache stays genuinely cold.
        builder.Services.PostConfigure<CacheSettings>(options =>
        {
            options.DocumentBreadthFirstSeedCount = 0;
            options.MediaBreadthFirstSeedCount = 0;
        });

        // Wrap the real database cache repository so single vs batched reads can be counted.
        builder.Services.AddUnique<IDatabaseCacheRepository>(sp =>
            new CountingDatabaseCacheRepository(ActivatorUtilities.CreateInstance<DatabaseCacheRepository>(sp)));
    }

    [TearDown]
    public void ClearHttpContext()
    {
        // Clearing the request avoids leaking a disposed service provider into later fixtures via IHttpContextAccessor.
        GetRequiredService<IHttpContextAccessor>().HttpContext = null;
    }

    [Test]
    public async Task Can_Convert_A_Multi_Node_Tree_Picker_Of_Documents_With_One_Batched_Read()
    {
        SetRequest();
        using UmbracoContextReference umbracoContextReference = GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        Guid[] documentKeys = CreateDocuments();
        IDataType dataType = await CreateDataTypeAsync(
            Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
            new Dictionary<string, object> { { "startNode", new MultiNodePickerConfigurationTreeSource { ObjectType = "content" } } });
        var storedValue = string.Join(",", documentKeys.Select(key => Udi.Create(Constants.UdiEntityType.Document, key)));

        IPublishedContent host = await CreateHostAndGoColdAsync(dataType, storedValue, documentKeys, []);

        IEnumerable<IPublishedContent>? picked = host.Value<IEnumerable<IPublishedContent>>(PublishedValueFallback, PickerAlias);

        Assert.Multiple(() =>
        {
            Assert.That(picked?.Select(x => x.Key), Is.EqualTo(documentKeys), "The picked documents should be returned in the order they were picked.");
            AssertReadInOneBatch(documentKeys, _countingRepository.SingleContentKeys, _countingRepository.BatchContentKeys, "documents");
        });
    }

    [Test]
    public async Task Can_Convert_A_Multiple_Document_Picker_With_One_Batched_Read()
    {
        SetRequest();
        using UmbracoContextReference umbracoContextReference = GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        Guid[] documentKeys = CreateDocuments();
        IDataType dataType = await CreateDataTypeAsync(Constants.PropertyEditors.Aliases.MultipleDocumentPicker, []);
        var storedValue = JsonSerializer.Serialize(documentKeys);

        IPublishedContent host = await CreateHostAndGoColdAsync(dataType, storedValue, documentKeys, []);

        IEnumerable<IPublishedContent>? picked = host.Value<IEnumerable<IPublishedContent>>(PublishedValueFallback, PickerAlias);

        Assert.Multiple(() =>
        {
            Assert.That(picked?.Select(x => x.Key), Is.EqualTo(documentKeys), "The picked documents should be returned in the order they were picked.");
            AssertReadInOneBatch(documentKeys, _countingRepository.SingleContentKeys, _countingRepository.BatchContentKeys, "documents");
        });
    }

    [Test]
    public async Task Can_Convert_A_Media_Picker_With_Crops_With_One_Batched_Read()
    {
        SetRequest();
        using UmbracoContextReference umbracoContextReference = GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        Guid[] mediaKeys = await CreateMediaAsync();
        IDataType dataType = await CreateDataTypeAsync(
            Constants.PropertyEditors.Aliases.MediaPicker3,
            new Dictionary<string, object> { { "multiple", true } });
        var storedValue = JsonSerializer.Serialize(mediaKeys.Select(key =>
            new MediaPickerPropertyEditorBase.MediaPickerPropertyValueEditor.MediaWithCropsDto
            {
                Key = Guid.NewGuid(),
                MediaKey = key,
                Crops = [],
            }).ToArray());

        IPublishedContent host = await CreateHostAndGoColdAsync(dataType, storedValue, [], mediaKeys);

        IEnumerable<MediaWithCrops>? picked = host.Value<IEnumerable<MediaWithCrops>>(PublishedValueFallback, PickerAlias);

        Assert.Multiple(() =>
        {
            Assert.That(picked?.Select(x => x.Content.Key), Is.EqualTo(mediaKeys), "The picked media should be returned in the order they were picked.");
            AssertReadInOneBatch(mediaKeys, _countingRepository.SingleMediaKeys, _countingRepository.BatchMediaKeys, "media");
        });
    }

    [Test]
    public async Task Can_Convert_A_Multi_Url_Picker_Of_Documents_And_Media_With_One_Batched_Read_Each()
    {
        SetRequest();
        using UmbracoContextReference umbracoContextReference = GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        Guid[] documentKeys = CreateDocuments();
        Guid[] mediaKeys = await CreateMediaAsync();
        IDataType dataType = await CreateDataTypeAsync(
            Constants.PropertyEditors.Aliases.MultiUrlPicker,
            new Dictionary<string, object> { { "maxNumber", 0 } });

        // Alternate documents and media, so the batches are not simply the first half and the second half of the value.
        var udis = new List<GuidUdi>();
        for (var i = 0; i < PickedItemCount; i++)
        {
            udis.Add(new GuidUdi(Constants.UdiEntityType.Document, documentKeys[i]));
            udis.Add(new GuidUdi(Constants.UdiEntityType.Media, mediaKeys[i]));
        }

        var storedValue = JsonSerializer.Serialize(udis.Select(udi => new MultiUrlPickerValueEditor.LinkDto { Udi = udi }).ToArray());

        IPublishedContent host = await CreateHostAndGoColdAsync(dataType, storedValue, documentKeys, mediaKeys);

        IEnumerable<Link>? links = host.Value<IEnumerable<Link>>(PublishedValueFallback, PickerAlias);

        Assert.Multiple(() =>
        {
            Assert.That(links?.Select(x => ((GuidUdi)x.Udi!).Guid), Is.EqualTo(udis.Select(x => x.Guid)), "The links should be returned in the order they were picked.");
            AssertReadInOneBatch(documentKeys, _countingRepository.SingleContentKeys, _countingRepository.BatchContentKeys, "documents");
            AssertReadInOneBatch(mediaKeys, _countingRepository.SingleMediaKeys, _countingRepository.BatchMediaKeys, "media");
        });
    }

    // The picked items must never be read one row at a time, and must all arrive in exactly one batched read that asks for
    // nothing else. Reads of other items (a page's ancestors, for example) are not what the converters batch and are ignored.
    private static void AssertReadInOneBatch(
        Guid[] pickedKeys,
        IReadOnlyCollection<Guid> singleReadKeys,
        IReadOnlyCollection<Guid[]> batchReadKeys,
        string description)
    {
        Guid[][] batchesOfPickedItems = batchReadKeys.Where(batch => batch.Intersect(pickedKeys).Any()).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(singleReadKeys.Intersect(pickedKeys), Is.Empty, $"The picked {description} must not be read one row at a time.");
            Assert.That(batchesOfPickedItems, Has.Length.EqualTo(1), $"The picked {description} should be read in one batch.");
            Assert.That(batchesOfPickedItems.SelectMany(batch => batch), Is.EquivalentTo(pickedKeys), $"The batch should ask for the picked {description} and nothing else.");
        });
    }

    private void SetRequest()
        => GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
            },
            RequestServices = Services,
        };

    private Guid[] CreateDocuments()
    {
        var keys = new List<Guid>();
        for (var i = 0; i < PickedItemCount; i++)
        {
            Content document = ContentBuilder.CreateSimpleContent(ContentType, $"Picked document {i}", Textpage.Id);
            ContentService.Save(document, -1);
            keys.Add(document.Key);
        }

        return keys.ToArray();
    }

    private async Task<Guid[]> CreateMediaAsync()
    {
        MediaType mediaType = MediaTypeBuilder.CreateSimpleMediaType("pickedMedia", "Picked media");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var keys = new List<Guid>();
        for (var i = 0; i < PickedItemCount; i++)
        {
            Media media = MediaBuilder.CreateSimpleMedia(mediaType, $"Picked media {i}", -1);
            MediaService.Save(media, -1);
            keys.Add(media.Key);
        }

        return keys.ToArray();
    }

    private async Task<IDataType> CreateDataTypeAsync(string editorAlias, Dictionary<string, object> configurationData)
    {
        IDataEditor editor = GetRequiredService<PropertyEditorCollection>()[editorAlias]
                             ?? throw new InvalidOperationException($"The property editor {editorAlias} was not registered.");

        var dataType = new DataType(editor, GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = configurationData,
            Name = $"Test {editorAlias}",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    // Publishes a host page whose picker property holds the given value together with the picked documents, empties both
    // cache tiers, and resolves the host, so only reading the picker property is left to measure.
    private async Task<IPublishedContent> CreateHostAndGoColdAsync(
        IDataType dataType,
        string storedValue,
        Guid[] documentKeys,
        Guid[] mediaKeys)
    {
        _countingRepository = (CountingDatabaseCacheRepository)GetRequiredService<IDatabaseCacheRepository>();

        ContentType hostContentType = ContentTypeBuilder.CreateSimpleContentType("pickerHost", "Picker host", defaultTemplateId: ContentType.DefaultTemplate!.Id);
        hostContentType.AddPropertyType(new PropertyType(GetRequiredService<IShortStringHelper>(), dataType, PickerAlias) { Name = "Picker" }, "content");
        await ContentTypeService.CreateAsync(hostContentType, Constants.Security.SuperUserKey);

        Content host = ContentBuilder.CreateSimpleContent(hostContentType, "Host", -1);
        host.SetValue(PickerAlias, storedValue);
        ContentService.Save(host, -1);

        // Publishes the picked documents along with the rest of the Textpage branch, then the host.
        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.Publish(host, ["*"]);

        var documentCacheService = (DocumentCacheService)DocumentCacheService;
        documentCacheService.ResetSeedKeys();
        await DocumentCacheService.ClearMemoryCacheAsync(CancellationToken.None);
        var mediaCacheService = (MediaCacheService)MediaCacheService;
        mediaCacheService.ResetSeedKeys();
        await MediaCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        GetRequiredService<IVariationContextAccessor>().VariationContext = new VariationContext(DefaultCulture);

        IPublishedContent? publishedHost = await PublishedContentCache.GetByIdAsync(host.Key);
        Assert.IsNotNull(publishedHost);

        // Sanity-check the precondition: the picked items are genuinely cold, so a passing test cannot be a false green
        // from a warm cache.
        foreach (Guid key in documentKeys)
        {
            Assert.IsFalse(DocumentCacheService.TryGetCached(key, false, out _), "Picked documents must not be cached before the conversion.");
        }

        foreach (Guid key in mediaKeys)
        {
            Assert.IsFalse(MediaCacheService.TryGetCached(key, out _), "Picked media must not be cached before the conversion.");
        }

        _countingRepository.Reset();
        return publishedHost!;
    }
}
