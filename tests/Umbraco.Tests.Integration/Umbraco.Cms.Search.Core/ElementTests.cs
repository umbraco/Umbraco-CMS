using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ElementTests : ContentBaseTestBase
{
    private const string ElementsIndexAlias = global::Umbraco.Cms.Core.Constants.IndexAliases.DraftElements;

    private const string ElementsViaDraftContentStrategyIndexAlias = "Test_ElementsViaDraftContentStrategy";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IDistributedContentIndexRebuilder DistributedContentIndexRebuilder => GetRequiredService<IDistributedContentIndexRebuilder>();

    private Guid RootContainerKey { get; } = Guid.NewGuid();

    private Guid ChildContainerKey { get; } = Guid.NewGuid();

    private Guid OtherRootContainerKey { get; } = Guid.NewGuid();

    private IContentType _elementType = null!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftElementChangeStrategy>(ElementsIndexAlias, UmbracoObjectTypes.Element);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(ElementsViaDraftContentStrategyIndexAlias, UmbracoObjectTypes.Element);
        });
    }

    [SetUp]
    public async Task SetupTest()
    {
        _elementType = await CreateElementType(ContentVariation.Nothing);

        await ElementContainerService.CreateAsync(RootContainerKey, "Root container", null, Constants.Security.SuperUserKey);
        await ElementContainerService.CreateAsync(ChildContainerKey, "Child container", RootContainerKey, Constants.Security.SuperUserKey);
        await ElementContainerService.CreateAsync(OtherRootContainerKey, "Other root container", null, Constants.Security.SuperUserKey);

        IndexerAndSearcher.Reset();
    }

    [Test]
    public async Task Element_AtRoot_IsIndexed_WithRootStructure()
    {
        IElement element = await CreateElement(null, "The title");

        TestIndexDocument document = GetElementDocument(element.Key);
        VerifyDocumentStructureValues(document, element.Key, Guid.Empty, [element.Key]);
        VerifyDocumentSystemValues(document, element, []);
    }

    [Test]
    public async Task Element_InNestedContainer_IsIndexed_WithContainerAncestry()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");

        TestIndexDocument document = GetElementDocument(element.Key);
        VerifyDocumentStructureValues(document, element.Key, ChildContainerKey, [RootContainerKey, ChildContainerKey, element.Key]);
    }

    [Test]
    public async Task Element_IsIndexed_OnlyByTheElementChangeStrategy()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");

        Attempt<EntityContainerOperationStatus> moveResult = await ElementContainerService.MoveAsync(ChildContainerKey, OtherRootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(moveResult.Success, Is.True);

        DistributedContentIndexRebuilder.Rebuild(ElementsViaDraftContentStrategyIndexAlias);

        Assert.Multiple(() =>
        {
            Assert.That(ElementsIndex().Select(document => document.Id), Is.EquivalentTo(new[] { element.Key }));
            Assert.That(IndexerAndSearcher.Dump(ElementsViaDraftContentStrategyIndexAlias), Is.Empty);
        });
    }

    [Test]
    public async Task Element_IsIndexed_OnlyInElementIndexes()
    {
        await CreateElement(ChildContainerKey, "The title");

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(ElementsIndexAlias), Has.Count.EqualTo(1));
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);
        });
    }

    [Test]
    public async Task Element_Update_IndexesDraftValues_WithoutPublishing()
    {
        IElement element = await CreateElement(null, "The original title");

        Attempt<ElementUpdateResult, ContentEditingOperationStatus> result = await ElementEditingService.UpdateAsync(
            element.Key,
            new ElementUpdateModel
            {
                Variants = [new VariantModel { Name = element.Name! }],
                Properties = [new PropertyValueModel { Alias = "title", Value = "The updated title" }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = GetElementDocument(element.Key);
        Assert.That(GetTitle(document), Is.EqualTo("The updated title"));
    }

    [Test]
    public async Task Element_Move_ReindexesStructure()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");

        Attempt<ContentEditingOperationStatus> result = await ElementEditingService.MoveAsync(element.Key, OtherRootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = GetElementDocument(element.Key);
        VerifyDocumentStructureValues(document, element.Key, OtherRootContainerKey, [OtherRootContainerKey, element.Key]);
    }

    [Test]
    public async Task Element_MoveToRecycleBin_ReindexesStructure()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");

        Attempt<ContentEditingOperationStatus> result = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = GetElementDocument(element.Key);
        VerifyDocumentStructureValues(
            document,
            element.Key,
            Constants.System.RecycleBinElementKey,
            [Constants.System.RecycleBinElementKey, element.Key]);
    }

    [Test]
    public async Task Element_Delete_RemovesDocument()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");
        Assert.That(ElementsIndex(), Has.Count.EqualTo(1));

        Attempt<ContentEditingOperationStatus> trashResult = await ElementEditingService.MoveToRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        Attempt<IElement?, ContentEditingOperationStatus> deleteResult = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(deleteResult.Success, Is.True);

        Assert.That(ElementsIndex(), Is.Empty);
    }

    [Test]
    public async Task ContainerMove_ReindexesDescendantElements()
    {
        IElement childContainerElement = await CreateElement(ChildContainerKey, "Child container element");
        IElement rootContainerElement = await CreateElement(RootContainerKey, "Root container element");

        Attempt<EntityContainerOperationStatus> result = await ElementContainerService.MoveAsync(ChildContainerKey, OtherRootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        VerifyDocumentStructureValues(
            GetElementDocument(childContainerElement.Key),
            childContainerElement.Key,
            ChildContainerKey,
            [OtherRootContainerKey, ChildContainerKey, childContainerElement.Key]);
        VerifyDocumentStructureValues(
            GetElementDocument(rootContainerElement.Key),
            rootContainerElement.Key,
            RootContainerKey,
            [RootContainerKey, rootContainerElement.Key]);
    }

    [Test]
    public async Task ContainerMoveToRecycleBin_ReindexesDescendantElements()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");

        Attempt<EntityContainerOperationStatus> result = await ElementContainerService.MoveToRecycleBinAsync(RootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        VerifyDocumentStructureValues(
            GetElementDocument(element.Key),
            element.Key,
            ChildContainerKey,
            [Constants.System.RecycleBinElementKey, RootContainerKey, ChildContainerKey, element.Key]);
    }

    [Test]
    public async Task ElementTypeChange_ReindexesElements()
    {
        IElement element = await CreateElement(ChildContainerKey, "The title");
        IndexerAndSearcher.Reset();

        _elementType.RemovePropertyType("text");
        Attempt<ContentTypeOperationStatus> result = await ContentTypeService.UpdateAsync(_elementType, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = GetElementDocument(element.Key);
        Assert.That(GetTitle(document), Is.EqualTo("The title"));
    }

    [Test]
    public async Task Rebuild_ElementIndex_ContainsAllElements_IncludingTrashed()
    {
        IElement rootElement = await CreateElement(null, "Root element");
        IElement nestedElement = await CreateElement(ChildContainerKey, "Nested element");
        IElement trashedElement = await CreateElement(OtherRootContainerKey, "Trashed element");
        Attempt<ContentEditingOperationStatus> trashResult = await ElementEditingService.MoveToRecycleBinAsync(trashedElement.Key, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        IndexerAndSearcher.Reset();
        DistributedContentIndexRebuilder.Rebuild(ElementsIndexAlias);

        Assert.That(
            ElementsIndex().Select(document => document.Id),
            Is.EquivalentTo(new[] { rootElement.Key, nestedElement.Key, trashedElement.Key }));
        Assert.That(ElementsIndex().All(document => document.ObjectType is UmbracoObjectTypes.Element), Is.True);
        VerifyDocumentStructureValues(
            GetElementDocument(nestedElement.Key),
            nestedElement.Key,
            ChildContainerKey,
            [RootContainerKey, ChildContainerKey, nestedElement.Key]);
    }

    [Test]
    public async Task Rebuild_DraftContentIndex_ContainsNoElements()
    {
        await CreateElement(ChildContainerKey, "The title");

        IndexerAndSearcher.Reset();
        DistributedContentIndexRebuilder.Rebuild(IndexAliases.DraftContent);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);
    }

    [Test]
    public async Task CultureVariantElement_IsIndexed_PerCulture()
    {
        await LanguageService.CreateAsync(new LanguageBuilder().WithCultureInfo("da-DK").Build(), Constants.Security.SuperUserKey);
        IContentType variantElementType = await CreateElementType(ContentVariation.Culture);

        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = variantElementType.Key,
                ParentKey = ChildContainerKey,
                Variants =
                [
                    new VariantModel { Culture = "en-US", Name = "English name" },
                    new VariantModel { Culture = "da-DK", Name = "Danish name" },
                ],
                Properties =
                [
                    new PropertyValueModel { Alias = "title", Value = "English title", Culture = "en-US" },
                    new PropertyValueModel { Alias = "title", Value = "Danish title", Culture = "da-DK" },
                ],
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = GetElementDocument(result.Result.Content!.Key);
        Assert.Multiple(() =>
        {
            Assert.That(document.Variations.Select(variation => variation.Culture), Is.EquivalentTo(new[] { "en-US", "da-DK" }));
            Assert.That(GetTitle(document, "en-US"), Is.EqualTo("English title"));
            Assert.That(GetTitle(document, "da-DK"), Is.EqualTo("Danish title"));

            IndexField[] nameFields = document.Fields.Where(field => field.FieldName == Constants.IndexFieldNames.Name).ToArray();
            Assert.That(nameFields.SingleOrDefault(field => field.Culture == "en-US")?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo("English name"));
            Assert.That(nameFields.SingleOrDefault(field => field.Culture == "da-DK")?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo("Danish name"));
        });
    }

    private IReadOnlyList<TestIndexDocument> ElementsIndex() => IndexerAndSearcher.Dump(ElementsIndexAlias);

    private TestIndexDocument GetElementDocument(Guid key)
    {
        TestIndexDocument? document = ElementsIndex().SingleOrDefault(document => document.Id == key);
        Assert.That(document, Is.Not.Null, $"Element {key} was not found in the elements index.");
        return document!;
    }

    private static string? GetTitle(TestIndexDocument document, string? culture = null)
        => document.Fields
            .SingleOrDefault(field => field.FieldName == "title" && field.Culture == culture)?
            .Value.Texts?.SingleOrDefault();

    private async Task<IContentType> CreateElementType(ContentVariation variation)
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName("Element type")
            .WithAllowAsRoot(true)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(variation)
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithVariations(variation)
                .Done()
            .AddPropertyType()
                .WithAlias("text")
                .WithName("Text")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .WithVariations(variation)
                .Done()
            .Build();
        Attempt<ContentTypeOperationStatus> result = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return elementType;
    }

    private async Task<IElement> CreateElement(Guid? parentKey, string title)
    {
        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                ParentKey = parentKey,
                Variants = [new VariantModel { Name = Guid.NewGuid().ToString("N") }],
                Properties = [new PropertyValueModel { Alias = "title", Value = title }],
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return result.Result.Content!;
    }
}
