using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

[LongRunning]
public class LibraryElementSearchTests : SearcherTestBase
{
    private const string ElementsIndexAlias = Constants.Search.IndexAliases.DraftElements;

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IIndexedEntitySearchService IndexedEntitySearchService => GetRequiredService<IIndexedEntitySearchService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IContentType _elementType = null!;

    [SetUp]
    public async Task SetupTest()
    {
        _elementType = new ContentTypeBuilder()
            .WithAlias("libraryElementType")
            .WithName("Library element type")
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
    }

    [Test]
    public async Task Can_Search_LibraryElement_By_PropertyValue()
    {
        IElement element = await CreateElementAsync(null, "Some element", "findable library text");

        SearchResult results = await Searcher.SearchAsync(ElementsIndexAlias, "findable");

        Assert.Multiple(() =>
        {
            Assert.That(results.Total, Is.EqualTo(1));
            Assert.That(results.Documents.Single().Id, Is.EqualTo(element.Key));
            Assert.That(results.Documents.Single().ObjectType, Is.EqualTo(UmbracoObjectTypes.Element));
        });
    }

    [Test]
    public async Task Can_Search_LibraryElement_By_Name()
    {
        IElement element = await CreateElementAsync(null, "Distinctive element name", "some text");

        SearchResult results = await Searcher.SearchAsync(ElementsIndexAlias, "distinctive");

        Assert.That(results.Documents.Select(document => document.Id), Is.EquivalentTo(new[] { element.Key }));
    }

    [Test]
    public async Task Can_Filter_LibraryElements_By_Container()
    {
        var containerKey = Guid.NewGuid();
        var childContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.CreateAsync(childContainerKey, "Child container", containerKey, Constants.Security.SuperUserKey);

        IElement nestedElement = await CreateElementAsync(childContainerKey, "Nested", "shared text");
        await CreateElementAsync(null, "At root", "shared text");

        SearchResult results = await Searcher.SearchAsync(
            ElementsIndexAlias,
            "shared",
            filters: [new KeywordFilter(Constants.Search.FieldNames.PathIds, [containerKey.AsKeyword()], false)]);

        Assert.That(results.Documents.Select(document => document.Id), Is.EquivalentTo(new[] { nestedElement.Key }));
    }

    [Test]
    public async Task Search_Reflects_LibraryElement_Updates()
    {
        IElement element = await CreateElementAsync(null, "Some element", "original text");

        await WaitForIndexing(ElementsIndexAlias, async () =>
        {
            Attempt<ElementUpdateResult, ContentEditingOperationStatus> result = await ElementEditingService.UpdateAsync(
                element.Key,
                new ElementUpdateModel
                {
                    Variants = [new VariantModel { Name = "Some element" }],
                    Properties = [new PropertyValueModel { Alias = "title", Value = "replacement text" }],
                },
                Constants.Security.SuperUserKey);
            Assert.That(result.Success, Is.True);
        });

        SearchResult originalResults = await Searcher.SearchAsync(ElementsIndexAlias, "original");
        SearchResult replacementResults = await Searcher.SearchAsync(ElementsIndexAlias, "replacement");

        Assert.Multiple(() =>
        {
            Assert.That(originalResults.Total, Is.Zero);
            Assert.That(replacementResults.Total, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Search_Excludes_Deleted_LibraryElements()
    {
        IElement element = await CreateElementAsync(null, "Some element", "doomed text");

        await WaitForIndexing(ElementsIndexAlias, async () =>
        {
            Assert.That((await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey)).Success, Is.True);
            Assert.That((await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey)).Success, Is.True);
        });

        Assert.That((await Searcher.SearchAsync(ElementsIndexAlias, "doomed")).Total, Is.Zero);
    }

    [Test]
    public async Task LibraryElement_Is_Not_Found_In_Content_Indexes()
    {
        await CreateElementAsync(null, "Some element", "librarytext");

        SearchResult draftResults = await Searcher.SearchAsync(Constants.Search.IndexAliases.DraftContent, "librarytext");
        SearchResult publishedResults = await Searcher.SearchAsync(Constants.Search.IndexAliases.PublishedContent, "librarytext");

        Assert.Multiple(() =>
        {
            Assert.That(draftResults.Total, Is.Zero);
            Assert.That(publishedResults.Total, Is.Zero);
        });
    }

    [Test]
    public async Task BlockElement_Is_Found_With_Its_Document_And_Not_In_Elements_Index()
    {
        IContent document = await CreateDocumentWithBlockAsync("blocktext");
        await CreateElementAsync(null, "Some element", "librarytext");

        SearchResult contentResults = await Searcher.SearchAsync(Constants.Search.IndexAliases.DraftContent, "blocktext");
        SearchResult elementResults = await Searcher.SearchAsync(ElementsIndexAlias, "blocktext");

        Assert.Multiple(() =>
        {
            Assert.That(contentResults.Documents.Select(result => result.Id), Is.EquivalentTo(new[] { document.Key }));
            Assert.That(elementResults.Total, Is.Zero);
        });
    }

    [Test]
    public async Task Can_Search_LibraryElements_Through_The_Backoffice_Search_Service()
    {
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Container", null, Constants.Security.SuperUserKey);
        IElement element = await CreateElementAsync(containerKey, "Backoffice element", "backoffice searchable text");
        await CreateDocumentWithBlockAsync("backoffice searchable text");

        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Element,
            "searchable",
            parentId: null,
            contentTypeIds: null,
            trashed: null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            IEntitySlim item = result.Items.Single();
            Assert.That(item, Is.InstanceOf<IElementEntitySlim>());
            Assert.That(item.Key, Is.EqualTo(element.Key));
            Assert.That(item.Name, Is.EqualTo("Backoffice element"));
        });
    }

    [TestCase("en-US", "englishname", true)]
    [TestCase("en-US", "danishname", false)]
    [TestCase("da-DK", "danishname", true)]
    [TestCase("da-DK", "englishname", false)]
    [TestCase(null, "englishname", false)]
    public async Task Culture_Variant_LibraryElement_Is_Searched_In_The_Requested_Culture(string? culture, string query, bool expectFound)
    {
        await LanguageService.CreateAsync(new LanguageBuilder().WithCultureInfo("da-DK").Build(), Constants.Security.SuperUserKey);
        IContentType variantElementType = new ContentTypeBuilder()
            .WithAlias("variantLibraryElementType")
            .WithName("Variant library element type")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithAllowAsRoot(true)
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        await ContentTypeService.CreateAsync(variantElementType, Constants.Security.SuperUserKey);

        IElement? element = null;
        await WaitForIndexing(ElementsIndexAlias, async () =>
        {
            Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
                new ElementCreateModel
                {
                    ContentTypeKey = variantElementType.Key,
                    Variants =
                    [
                        new VariantModel { Culture = "en-US", Name = "Englishname element" },
                        new VariantModel { Culture = "da-DK", Name = "Danishname element" },
                    ],
                },
                Constants.Security.SuperUserKey);
            Assert.That(result.Success, Is.True);
            element = result.Result.Content;
        });

        PagedModel<IEntitySlim> searchResult = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Element,
            query,
            parentId: null,
            contentTypeIds: null,
            trashed: null,
            culture: culture);

        Assert.That(
            searchResult.Items.Select(item => item.Key),
            expectFound ? Is.EquivalentTo(new[] { element!.Key }) : Is.Empty);
    }

    [TestCase(null, true, true)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    public async Task Search_Filters_LibraryElements_By_Trashed_State(bool? trashed, bool expectTrashed, bool expectActive)
    {
        IElement trashedElement = await CreateElementAsync(null, "Trashed element", "trashfilter text");
        IElement activeElement = await CreateElementAsync(null, "Active element", "trashfilter text");
        await WaitForIndexing(ElementsIndexAlias, async () =>
            Assert.That((await ElementEditingService.MoveToRecycleBinAsync(trashedElement.Key, Constants.Security.SuperUserKey)).Success, Is.True));

        PagedModel<IEntitySlim> searchResult = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Element,
            "trashfilter",
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        var expected = new List<Guid>();
        if (expectTrashed)
        {
            expected.Add(trashedElement.Key);
        }

        if (expectActive)
        {
            expected.Add(activeElement.Key);
        }

        Assert.That(searchResult.Items.Select(item => item.Key), Is.EquivalentTo(expected));
    }

    private async Task<IElement> CreateElementAsync(Guid? containerKey, string name, string title)
    {
        IElement? element = null;
        await WaitForIndexing(ElementsIndexAlias, async () =>
        {
            Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
                new ElementCreateModel
                {
                    ContentTypeKey = _elementType.Key,
                    ParentKey = containerKey,
                    Variants = [new VariantModel { Name = name }],
                    Properties = [new PropertyValueModel { Alias = "title", Value = title }],
                },
                Constants.Security.SuperUserKey);
            Assert.That(result.Success, Is.True);
            element = result.Result.Content;
        });

        return element!;
    }

    private async Task<IContent> CreateDocumentWithBlockAsync(string blockText)
    {
        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "blocks", new BlockListConfiguration.BlockConfiguration[] { new() { ContentElementTypeKey = _elementType.Key } } },
            },
            Name = "Library element block list",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType documentType = new ContentTypeBuilder()
            .WithAlias("blockPage")
            .WithName("Block page")
            .AddPropertyType()
                .WithAlias("blocks")
                .WithName("Blocks")
                .WithDataTypeId(blockListDataType.Id)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(documentType, Constants.Security.SuperUserKey);
        documentType = await ContentTypeService.GetAsync(documentType.Key) ?? throw new InvalidOperationException("Document type not found");

        var contentElementKey = Guid.NewGuid();
        var blockListValue = new BlockListValue([new BlockListLayoutItem { ContentKey = contentElementKey }])
        {
            ContentData =
            [
                new BlockItemData(contentElementKey, _elementType.Key, _elementType.Alias)
                {
                    Values = [new BlockPropertyValue { Alias = "title", Value = blockText }],
                },
            ],
            Expose = [new BlockItemVariation { ContentKey = contentElementKey }],
        };

        Content document = new ContentBuilder()
            .WithContentType(documentType)
            .WithName("Page with blocks")
            .WithPropertyValues(new { blocks = JsonSerializer.Serialize(blockListValue) })
            .Build();

        await WaitForIndexing(Constants.Search.IndexAliases.DraftContent, () =>
        {
            ContentService.Save(document);
            return Task.CompletedTask;
        });

        return document;
    }
}
