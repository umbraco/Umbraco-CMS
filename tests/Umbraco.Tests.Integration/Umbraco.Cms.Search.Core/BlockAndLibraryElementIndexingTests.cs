using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Elements used as blocks are part of the document that holds them, and are indexed with it. Library elements are
/// stand-alone entities, and are indexed in the elements index only. These tests assert that the two never mix,
/// even when they share an element type.
/// </summary>
public class BlockAndLibraryElementIndexingTests : ContentTestBase
{
    private const string BlockText = "blockonlytext";

    private const string LibraryText = "libraryonlytext";

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentType _elementType = null!;

    private IContentType _blockEditorContentType = null!;

    [SetUp]
    public async Task SetupTest()
    {
        _elementType = new ContentTypeBuilder()
            .WithAlias("sharedElementType")
            .WithName("Shared element type")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithAllowAsRoot(true)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "blocks", new BlockListConfiguration.BlockConfiguration[] { new() { ContentElementTypeKey = _elementType.Key } } },
            },
            Name = "Shared element block list",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        _blockEditorContentType = new ContentTypeBuilder()
            .WithAlias("blockEditor")
            .WithName("Block editor")
            .AddPropertyType()
                .WithAlias("blocks")
                .WithName("Blocks")
                .WithDataTypeId(blockListDataType.Id)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(_blockEditorContentType, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }

    [Test]
    public void BlockElement_IsIndexedWithItsDocument()
    {
        IContent document = CreateDocumentWithBlock(BlockText);

        TestIndexDocument indexedDocument = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        Assert.Multiple(() =>
        {
            Assert.That(indexedDocument.Id, Is.EqualTo(document.Key));
            Assert.That(indexedDocument.ObjectType, Is.EqualTo(UmbracoObjectTypes.Document));
            Assert.That(AllTexts(indexedDocument), Has.Some.Contains(BlockText));
        });
    }

    [Test]
    public void BlockElement_IsNotIndexedAsLibraryElement()
    {
        CreateDocumentWithBlock(BlockText);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Elements), Is.Empty);
    }

    [Test]
    public async Task LibraryElement_IsNotIndexedAsContent()
    {
        IElement libraryElement = await CreateLibraryElement(LibraryText);

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Elements).Select(document => document.Id), Is.EquivalentTo(new[] { libraryElement.Key }));
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);
        });
    }

    [Test]
    public async Task BlockAndLibraryElements_OfTheSameElementType_AreIndexedSeparately()
    {
        IContent document = CreateDocumentWithBlock(BlockText);
        IElement libraryElement = await CreateLibraryElement(LibraryText);

        TestIndexDocument indexedDocument = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        TestIndexDocument indexedElement = IndexerAndSearcher.Dump(IndexAliases.Elements).Single();
        Assert.Multiple(() =>
        {
            Assert.That(indexedDocument.Id, Is.EqualTo(document.Key));
            Assert.That(AllTexts(indexedDocument), Has.Some.Contains(BlockText));
            Assert.That(AllTexts(indexedDocument), Has.None.Contains(LibraryText));

            Assert.That(indexedElement.Id, Is.EqualTo(libraryElement.Key));
            Assert.That(indexedElement.ObjectType, Is.EqualTo(UmbracoObjectTypes.Element));
            Assert.That(AllTexts(indexedElement), Has.Some.Contains(LibraryText));
            Assert.That(AllTexts(indexedElement), Has.None.Contains(BlockText));
        });
    }

    [Test]
    public async Task LibraryElementChange_DoesNotReindexContent()
    {
        CreateDocumentWithBlock(BlockText);
        IElement libraryElement = await CreateLibraryElement(LibraryText);
        IndexerAndSearcher.Reset();

        Attempt<ElementUpdateResult, ContentEditingOperationStatus> result = await ElementEditingService.UpdateAsync(
            libraryElement.Key,
            new ElementUpdateModel
            {
                Variants = [new VariantModel { Name = libraryElement.Name! }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "updatedlibrarytext" }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);
            Assert.That(AllTexts(IndexerAndSearcher.Dump(IndexAliases.Elements).Single()), Has.Some.Contains("updatedlibrarytext"));
        });
    }

    [Test]
    public async Task DocumentChange_DoesNotReindexLibraryElements()
    {
        IContent document = CreateDocumentWithBlock(BlockText);
        await CreateLibraryElement(LibraryText);
        IndexerAndSearcher.Reset();

        document.SetValue("blocks", BlocksValue("updatedblocktext"));
        ContentService.Save(document);

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Elements), Is.Empty);
            Assert.That(AllTexts(IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single()), Has.Some.Contains("updatedblocktext"));
        });
    }

    [Test]
    public async Task RebuildingEitherIndex_KeepsBlockAndLibraryElementsSeparate()
    {
        IContent document = CreateDocumentWithBlock(BlockText);
        IElement libraryElement = await CreateLibraryElement(LibraryText);
        IndexerAndSearcher.Reset();

        IDistributedContentIndexRebuilder rebuilder = GetRequiredService<IDistributedContentIndexRebuilder>();
        rebuilder.Rebuild(IndexAliases.DraftContent);
        rebuilder.Rebuild(IndexAliases.Elements);

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent).Select(d => d.Id), Is.EquivalentTo(new[] { document.Key }));
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Elements).Select(d => d.Id), Is.EquivalentTo(new[] { libraryElement.Key }));
        });
    }

    private static string[] AllTexts(TestIndexDocument document)
        => document.Fields.SelectMany(field => field.Value.AllTexts()).ToArray();

    private IContent CreateDocumentWithBlock(string text)
    {
        Content document = new ContentBuilder()
            .WithContentType(_blockEditorContentType)
            .WithName("Document with blocks")
            .WithPropertyValues(new { blocks = BlocksValue(text) })
            .Build();
        ContentService.Save(document);
        return document;
    }

    private string BlocksValue(string text)
    {
        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue([new() { ContentKey = contentElementKey }])
        {
            ContentData =
            [
                new(contentElementKey, _elementType.Key, _elementType.Alias)
                {
                    Values = [new() { Alias = "title", Value = text }],
                },
            ],
            Expose = [new() { ContentKey = contentElementKey }],
        };
        return JsonSerializer.Serialize(blockListValue);
    }

    private async Task<IElement> CreateLibraryElement(string text)
    {
        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                Variants = [new VariantModel { Name = "Library element" }],
                Properties = [new PropertyValueModel { Alias = "title", Value = text }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return result.Result.Content!;
    }
}
