using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that the content of an externally referenced (reusable) block element is flattened into the referencing
/// document's index entry when <see cref="IndexingSettings.IndexExternalBlockElements"/> is enabled.
/// </summary>
public class ExternalBlockElementIndexingTests : PropertyValueHandlerTestsBase
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);
    }

    [Test]
    public async Task Can_Flatten_Published_External_Element_Content_Into_Published_Index_Only()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        Guid elementKey = CreateAndPublishElement(elementType, "The external element text");

        Content content = CreatePageWithExternalBlockReference(contentType, elementKey);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, "The external element text");

        // external element content is only ever flattened into the published index, never the draft one
        TestIndexDocument draftDocument = IndexerAndSearcher.Dump(IndexAliases.DraftContent).Single();
        IndexValue? draftValue = draftDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(draftValue, Is.Null);
    }

    [Test]
    public async Task Cannot_Flatten_Unpublished_External_Element_Content()
    {
        var (contentType, elementType) = await SetupBlockListWithElementType();

        // create, but do not publish, the referenced element
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Unpublished reusable element")
            .Build();
        element.SetValue("textValue", "Should not be indexed");
        ElementService.Save(element);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Null);
    }

    [Test]
    public async Task Circular_External_Element_Reference_Does_Not_Cause_Stack_Overflow()
    {
        // an element type whose own "nestedBlocks" block list property can reference other elements of the same
        // type - this allows an element to (directly or transitively) reference itself.
        Guid elementTypeKey = Guid.NewGuid();

        var nestedBlockListDataType = new DataType(GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.BlockList], GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementTypeKey }
                    }
                }
            },
            Name = "Nested Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(nestedBlockListDataType, Constants.Security.SuperUserKey);

        IContentType elementType = new ContentTypeBuilder()
            .WithKey(elementTypeKey)
            .WithAlias("selfReferencingElement")
            .WithName("Self Referencing Element")
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("textValue")
            .WithName("Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("nestedBlocks")
            .WithName("Nested Blocks")
            .WithDataTypeId(nestedBlockListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var pageBlockListDataType = new DataType(GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.BlockList], GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = elementTypeKey }
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(pageBlockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithSelfReferencingBlocks")
            .WithName("Page With Self Referencing Blocks")
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(pageBlockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // the element references itself via its own "nestedBlocks" property
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Self Referencing Element")
            .Build();
        element.SetValue("textValue", "Some element text");

        var selfReferencingBlockListValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    [new BlockListLayoutItem { ContentKey = element.Key, IsExternalContent = true }]
                }
            },
            ContentData = [],
            Expose = [],
        };
        element.SetValue("nestedBlocks", GetRequiredService<IJsonSerializer>().Serialize(selfReferencingBlockListValue));

        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = CreatePageWithExternalBlockReference(contentType, element.Key);
        ContentService.Save(content);

        // this must not recurse indefinitely (and crash the process with a stack overflow) when flattening the
        // circular external element reference into the index
        Assert.DoesNotThrow(() => ContentService.Publish(content, ["*"]));

        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        IndexValue? publishedValue = publishedDocument.Fields.FirstOrDefault(f => f.FieldName == "blocks")?.Value;
        Assert.That(publishedValue, Is.Not.Null);
        CollectionAssert.Contains(publishedValue.Texts, "Some element text");
    }

    [Test]
    public async Task PropertyLevelVariantBlockList_IncludesInvariantElementsAndProperties()
    {
        // an element type with both an invariant and a culture-variant text property (block-level variance)
        IContentType mixedElementType = new ContentTypeBuilder()
            .WithAlias("mixedElement")
            .WithName("Mixed Element")
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantText")
            .WithName("Variant Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(mixedElementType, Constants.Security.SuperUserKey);

        // a fully invariant, reusable ("library") element type
        IContentType invariantOnlyElementType = new ContentTypeBuilder()
            .WithAlias("invariantOnlyElement")
            .WithName("Invariant Only Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .AddPropertyType()
            .WithAlias("onlyText")
            .WithName("Only Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(invariantOnlyElementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.BlockList], GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[]
                    {
                        new() { ContentElementTypeKey = mixedElementType.Key },
                        new() { ContentElementTypeKey = invariantOnlyElementType.Key },
                    }
                }
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow
        };
        await GetRequiredService<IDataTypeService>().CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        // the "blocks" property itself varies by culture, unlike the invariant block-list property tested elsewhere
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithVariantBlocks")
            .WithName("Page With Variant Blocks")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithDataTypeId(blockListDataType.Id)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // a published, reusable ("library") element with only an invariant property
        Element libraryElement = new ElementBuilder()
            .WithContentType(invariantOnlyElementType)
            .WithName("Library Element")
            .Build();
        libraryElement.SetValue("onlyText", "Invariant text in library element");
        ElementService.Save(libraryElement);
        ElementService.Publish(libraryElement, ["*"]);

        var mixedElementKey = Guid.NewGuid();
        var invariantElementKey = Guid.NewGuid();

        var blockListValue = new BlockListValue([
            new () { ContentKey = mixedElementKey },
            new () { ContentKey = invariantElementKey },
            new () { ContentKey = libraryElement.Key, IsExternalContent = true },
        ])
        {
            ContentData =
            [
                new (mixedElementKey, mixedElementType.Key, mixedElementType.Alias)
                {
                    Values =
                    [
                        new () { Alias = "invariantText", Value = "Invariant text in mixed element" },
                        new () { Alias = "variantText", Value = "Variant text EN", Culture = "en-US" },
                    ]
                },
                new (invariantElementKey, invariantOnlyElementType.Key, invariantOnlyElementType.Alias)
                {
                    Values =
                    [
                        new () { Alias = "onlyText", Value = "Invariant text in invariant element" },
                    ]
                }
            ],
            Expose =
            [
                new BlockItemVariation(mixedElementKey, "en-US", null),
                new BlockItemVariation(invariantElementKey, null, null),
            ]
        };

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "My Page")
            .Build();
        content.Properties["blocks"]!.SetValue(GetRequiredService<IJsonSerializer>().Serialize(blockListValue), "en-US");
        ContentService.Save(content);
        ContentService.Publish(content, ["en-US"]);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();

        IndexValue? invariantValue = document.Fields.SingleOrDefault(f => f is { FieldName: "blocks", Culture: null })?.Value;
        Assert.That(invariantValue, Is.Not.Null, "Invariant blocks/properties should still be indexed even though the containing block-list property varies by culture.");
        CollectionAssert.AreEquivalent(
            new[] { "Invariant text in mixed element", "Invariant text in invariant element", "Invariant text in library element" },
            invariantValue.Texts);

        IndexValue? variantValue = document.Fields.SingleOrDefault(f => f is { FieldName: "blocks", Culture: "en-US" })?.Value;
        Assert.That(variantValue, Is.Not.Null);
        CollectionAssert.AreEqual(new[] { "Variant text EN" }, variantValue.Texts);
    }

    private Guid CreateAndPublishElement(IContentType elementType, string textValue)
    {
        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", textValue);
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);
        return element.Key;
    }
}
