using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that a document referencing an external (reusable) block element is re-indexed when that element
/// itself changes, so its flattened content stays in sync.
/// </summary>
public class ElementIndexingNotificationHandlerTests : PropertyValueHandlerTestsBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);

        // wraps the real IContentIndexingService so tests can assert whether a reindex was actually triggered,
        // without relying on index *content* differences that a correct draft-vs-published distinction wouldn't
        // produce anyway (external element content is always flattened from its published values).
        services.AddSingleton<ContentIndexingService>();
        services.AddSingleton<CountingContentIndexingService>();
        services.AddSingleton<IContentIndexingService>(sp => sp.GetRequiredService<CountingContentIndexingService>());
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // creates the umbExternalBlockElement relation when the referencing document or element is saved/published
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ElementPublishedNotification, ContentRelationsUpdate>();

        // the per-element reindex trigger (ElementPublishStatusNotificationHandler -> ElementChangeCacheRefresher
        // -> ElementIndexingNotificationHandler) is wired up automatically by AddSearchCore() - no test-specific
        // registration needed.
    }

    [Test]
    public async Task Can_Reindex_Referencing_Document_When_Referenced_Element_Is_Republished()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertPublishedBlocksTextsContain("Original text");

        // change the element's content and republish - the referencing document should be transparently reindexed
        IElement updatedElement = ElementService.GetById(element.Key)!;
        updatedElement.SetValue("textValue", "Updated text");
        ElementService.Save(updatedElement);
        ElementService.Publish(updatedElement, ["*"]);

        AssertPublishedBlocksTextsContain("Updated text");
    }

    [Test]
    public async Task Saving_External_Element_Without_Publishing_Does_Not_Trigger_Reindex()
    {
        var countingService = (CountingContentIndexingService)ContentIndexingService;

        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertPublishedBlocksTextsContain("Original text");

        countingService.Reset();

        // a plain draft save of the already-published, already-referenced element must not trigger a reindex of
        // documents referencing it - only a publish makes a difference to the published index.
        IElement draftElement = ElementService.GetById(element.Key)!;
        draftElement.SetValue("textValue", "Draft-only text");
        ElementService.Save(draftElement);

        Assert.That(countingService.HandleCallCount, Is.Zero, "A draft-only element save must not trigger a reindex of documents referencing it.");
        AssertPublishedBlocksTextsContain("Original text");

        // publishing the same change must trigger the reindex
        ElementService.Publish(ElementService.GetById(element.Key)!, ["*"]);

        Assert.That(countingService.HandleCallCount, Is.GreaterThan(0), "Publishing the element must trigger a reindex of documents referencing it.");
        AssertPublishedBlocksTextsContain("Draft-only text");
    }

    [Test]
    public async Task Can_Remove_Element_Content_From_Referencing_Document_When_Element_Is_Unpublished()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "Original text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocumentBefore = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValueBefore = publishedDocumentBefore.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValueBefore, Is.Not.Null);

        ElementService.Unpublish(ElementService.GetById(element.Key)!);

        TestIndexDocument publishedDocumentAfter = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValueAfter = publishedDocumentAfter.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValueAfter, Is.Null);
    }

    [Test]
    public async Task Can_Reindex_Referencing_Document_When_Transitively_Referenced_Element_Is_Republished()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();
        await AddBlocksPropertyToElementType(elementType);

        // the leaf element - the one that will change - is referenced not by the document directly, but by
        // another (published) reusable element, which is the one the document actually references.
        Element leafElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Leaf element")
            .Build();
        leafElement.SetValue("textValue", "Original leaf text");
        ElementService.Save(leafElement);
        ElementService.Publish(leafElement, ["*"]);

        Element intermediateElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Intermediate element")
            .Build();
        intermediateElement.SetValue("textValue", "Intermediate text");
        intermediateElement.SetValue("blocks", JsonSerializer.Serialize(ExternalBlockListValue(leafElement.Key)));
        ElementService.Save(intermediateElement);
        ElementService.Publish(intermediateElement, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, intermediateElement.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertPublishedBlocksTextsContain("Intermediate text", "Original leaf text");

        // change only the leaf element and republish - the document should be reindexed transitively, through the
        // intermediate element, even though the document has no direct relation to the leaf element.
        IElement updatedLeafElement = ElementService.GetById(leafElement.Key)!;
        updatedLeafElement.SetValue("textValue", "Updated leaf text");
        ElementService.Save(updatedLeafElement);
        ElementService.Publish(updatedLeafElement, ["*"]);

        AssertPublishedBlocksTextsContain("Intermediate text", "Updated leaf text");
    }

    [Test]
    public async Task Cannot_Find_Referencing_Document_Through_Unpublished_Intermediate_Element()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();
        await AddBlocksPropertyToElementType(elementType);

        Element leafElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Leaf element")
            .Build();
        leafElement.SetValue("textValue", "Leaf text");
        ElementService.Save(leafElement);
        ElementService.Publish(leafElement, ["*"]);

        // the intermediate element references the leaf element externally, but is only ever saved, never published
        Element intermediateElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Intermediate element")
            .Build();
        intermediateElement.SetValue("textValue", "Intermediate text");
        intermediateElement.SetValue("blocks", JsonSerializer.Serialize(ExternalBlockListValue(leafElement.Key)));
        ElementService.Save(intermediateElement);

        Content content = CreatePageWithExternalBlockReference(contentType, intermediateElement.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        // call the traversal directly: the index-time flattening already excludes the unpublished intermediate
        // element's content independently, so there is no way to observe the traversal pruning through index
        // content alone - it must be verified directly.
        var handler = new ElementIndexingNotificationHandler(
            GetRequiredService<ICoreScopeProvider>(),
            ContentIndexingService,
            GetRequiredService<IRelationService>(),
            GetRequiredService<IOptions<IndexingSettings>>(),
            GetRequiredService<IOriginProvider>(),
            GetRequiredService<IIndexDocumentService>());

        Guid[] referencingDocumentKeys = handler.FindDocumentKeysReferencingElements([leafElement.Id]);

        Assert.That(referencingDocumentKeys, Is.Empty);
    }

    private static BlockListValue ExternalBlockListValue(Guid externalElementKey)
        => new()
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = externalElementKey, IsExternalContent = true }]
                }
            },
            ContentData = [],
            Expose = [],
        };

    private void AssertPublishedBlocksTextsContain(params string[] expectedTexts)
    {
        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        foreach (var expectedText in expectedTexts)
        {
            CollectionAssert.Contains(publishedValue.Texts, expectedText);
        }
    }

    // adds a second, self-referencing "blocks" property to the given element type, so an element of this type can
    // itself externally reference another element of the same type - used to build a transitive reference chain.
    private async Task AddBlocksPropertyToElementType(IContentType elementType)
    {
        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementType.Key }
                    }
                }
            },
            Name = "My Nested Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        elementType.AddPropertyType(new PropertyType(ShortStringHelper, blockListDataType, "blocks"));
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
    }

    // wraps the real ContentIndexingService to count Handle invocations, so tests can assert whether a reindex
    // was actually triggered.
    private sealed class CountingContentIndexingService : IContentIndexingService
    {
        private readonly ContentIndexingService _inner;

        public CountingContentIndexingService(ContentIndexingService inner) => _inner = inner;

        public int HandleCallCount { get; private set; }

        public void Reset() => HandleCallCount = 0;

        public void Handle(IEnumerable<ContentChange> changes, string origin)
        {
            HandleCallCount++;
            _inner.Handle(changes, origin);
        }

        public void Rebuild(string indexAlias, string origin) => _inner.Rebuild(indexAlias, origin);
    }
}
