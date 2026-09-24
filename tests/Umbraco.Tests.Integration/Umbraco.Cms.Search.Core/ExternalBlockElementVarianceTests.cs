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
using IndexField = Umbraco.Cms.Core.Search.Indexing.IndexField;
using IndexValue = Umbraco.Cms.Core.Search.Indexing.IndexValue;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that the content of an external (reusable) element is flattened into a referencing document's published
/// index regardless of how the document and the element vary relative to each other.
/// </summary>
/// <remarks>
/// An element varies independently of the documents that embed it, so the two variations do not have to agree: an
/// invariant element can be embedded in a culture- or segment-variant document, and vice versa. The rule the
/// rendering pipeline applies is that a variation only carries over when both sides vary that way (see
/// <c>BlockPropertyValueCreatorBase</c>); indexing must not lose content when they disagree.
/// </remarks>
public class ExternalBlockElementVarianceTests : ContentTestBase
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    [SetUp]
    public void SetUp() => IndexerAndSearcher.Reset();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.Configure<IndexingSettings>(options => options.IndexExternalBlockElements = true);
    }

    [Test]
    public async Task Can_Index_Invariant_And_Variant_Block_Content_On_Culture_Variant_Block_List_Property()
    {
        // a second language, so the invariant content can be shown to be indexed once under the invariant
        // variation rather than repeated under every culture
        await LanguageService.CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        // each element type is used twice in the same block list - once as a local block, once as an externally
        // referenced library element - so the only variable between the two is where the content comes from.
        IContentType mixedElementType = new ContentTypeBuilder()
            .WithAlias("mixedElement")
            .WithName("Mixed Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
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

        IContentType invariantOnlyElementType = new ContentTypeBuilder()
            .WithAlias("invariantOnlyElement")
            .WithName("Invariant Only Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant Text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(invariantOnlyElementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                ["blocks"] = new BlockListConfiguration.BlockConfiguration[]
                {
                    new() { ContentElementTypeKey = mixedElementType.Key },
                    new() { ContentElementTypeKey = invariantOnlyElementType.Key },
                },
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

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

        // the externally referenced counterparts of the two local blocks below, as published library elements
        Element externalInvariantElement = new ElementBuilder()
            .WithContentType(invariantOnlyElementType)
            .WithName("External Invariant Element")
            .Build();
        externalInvariantElement.SetValue("invariantText", "Invariant text in external invariant element");
        ElementService.Save(externalInvariantElement);
        ElementService.Publish(externalInvariantElement, ["*"]);

        Element externalMixedElement = new ElementBuilder()
            .WithContentType(mixedElementType)
            .WithCultureName("en-US", "External Mixed Element")
            .WithCultureName("da-DK", "External Mixed Element DA")
            .Build();
        externalMixedElement.SetValue("invariantText", "Invariant text in external mixed element");
        externalMixedElement.SetValue("variantText", "Variant text EN in external mixed element", "en-US");
        externalMixedElement.SetValue("variantText", "Variant text DA in external mixed element", "da-DK");
        ElementService.Save(externalMixedElement);
        ElementService.Publish(externalMixedElement, ["en-US", "da-DK"]);

        var localMixedElementKey = Guid.NewGuid();
        var localInvariantElementKey = Guid.NewGuid();

        var blockListValue = new BlockListValue([
            new BlockListLayoutItem { ContentKey = localMixedElementKey },
            new BlockListLayoutItem { ContentKey = localInvariantElementKey },
            new BlockListLayoutItem { ContentKey = externalInvariantElement.Key, IsExternalContent = true },
            new BlockListLayoutItem { ContentKey = externalMixedElement.Key, IsExternalContent = true },
        ])
        {
            ContentData =
            [
                new BlockItemData(localMixedElementKey, mixedElementType.Key, mixedElementType.Alias)
                {
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "Invariant text in local mixed element" },
                        new BlockPropertyValue { Alias = "variantText", Value = "Variant text EN in local mixed element", Culture = "en-US" },
                        new BlockPropertyValue { Alias = "variantText", Value = "Variant text DA in local mixed element", Culture = "da-DK" },
                    ],
                },
                new BlockItemData(localInvariantElementKey, invariantOnlyElementType.Key, invariantOnlyElementType.Alias)
                {
                    Values =
                    [
                        new BlockPropertyValue { Alias = "invariantText", Value = "Invariant text in local invariant element" },
                    ],
                }
            ],
            Expose =
            [
                new BlockItemVariation(localMixedElementKey, "en-US", null),
                new BlockItemVariation(localMixedElementKey, "da-DK", null),
                new BlockItemVariation(localInvariantElementKey, null, null),
            ],
        };

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "My Page")
            .WithCultureName("da-DK", "My Page DA")
            .Build();
        var blockListJson = JsonSerializer.Serialize(blockListValue);
        content.Properties["blocks"]!.SetValue(blockListJson, "en-US");
        content.Properties["blocks"]!.SetValue(blockListJson, "da-DK");
        ContentService.Save(content);
        ContentService.Publish(content, ["en-US", "da-DK"]);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();

        // invariant content is indexed once, under the invariant variation - not repeated under each culture
        IndexValue? invariantValue = document.Fields.SingleOrDefault(f => f is { FieldName: "blocks", Culture: null })?.Value;
        Assert.That(invariantValue, Is.Not.Null, "Invariant blocks/properties should still be indexed even though the containing block-list property varies by culture.");
        CollectionAssert.AreEquivalent(
            new[]
            {
                "Invariant text in local mixed element", "Invariant text in local invariant element",
                "Invariant text in external invariant element", "Invariant text in external mixed element",
            },
            invariantValue.Texts);

        // each culture carries only its own content, local and external alike
        IndexValue? englishValue = document.Fields.SingleOrDefault(f => f is { FieldName: "blocks", Culture: "en-US" })?.Value;
        Assert.That(englishValue, Is.Not.Null);
        CollectionAssert.AreEquivalent(
            new[] { "Variant text EN in local mixed element", "Variant text EN in external mixed element" },
            englishValue.Texts);

        IndexValue? danishValue = document.Fields.SingleOrDefault(f => f is { FieldName: "blocks", Culture: "da-DK" })?.Value;
        Assert.That(danishValue, Is.Not.Null);
        CollectionAssert.AreEquivalent(
            new[] { "Variant text DA in local mixed element", "Variant text DA in external mixed element" },
            danishValue.Texts);
    }

    // An invariant block list on a variant document still expands into the document's published cultures (see
    // GetPropertyCultures), so the element's per-culture publish state has to be honoured either way.
    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Nothing)]
    public async Task Does_Not_Index_Unpublished_Culture_Of_Culture_Variant_External_Element(ContentVariation blocksVariation)
    {
        await LanguageService.CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        IContentType elementType = await CreateElementType(ContentVariation.Culture);
        IContentType contentType = await CreateDocumentType(ContentVariation.Culture, elementType, blocksVariation);

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithCultureName("en-US", "Reusable element EN")
            .WithCultureName("da-DK", "Reusable element DA")
            .Build();
        element.SetValue("textValue", "The element text in English", "en-US");
        element.SetValue("textValue", "The element text in Danish", "da-DK");
        ElementService.Save(element);

        // only the English culture of the element is published
        ElementService.Publish(element, ["en-US"]);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Variant page EN")
            .WithCultureName("da-DK", "Variant page DA")
            .Build();
        if (blocksVariation is ContentVariation.Culture)
        {
            content.SetValue("blocks", ExternalBlockListJson(element.Key), "en-US");
            content.SetValue("blocks", ExternalBlockListJson(element.Key), "da-DK");
        }
        else
        {
            content.SetValue("blocks", ExternalBlockListJson(element.Key));
        }

        ContentService.Save(content);
        ContentService.Publish(content, ["en-US", "da-DK"]);

        IndexField[] blocksFields = BlocksFields();
        Assert.Multiple(() =>
        {
            AssertBlocksFieldContains(blocksFields, f => f.Culture == "en-US", "en-US", "The element text in English");
            Assert.That(
                blocksFields.Where(f => f.Culture == "da-DK").SelectMany(f => f.Value.Texts ?? []),
                Is.Empty,
                "the element's unpublished da-DK culture must not contribute any content");
        });
    }

    [Test]
    public async Task Does_Not_Index_Culture_Variant_External_Element_Content_On_Invariant_Document()
    {
        await LanguageService.CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        IContentType elementType = await CreateElementType(ContentVariation.Culture);
        IContentType contentType = await CreateDocumentType(ContentVariation.Nothing, elementType);

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithCultureName("en-US", "Reusable element EN")
            .WithCultureName("da-DK", "Reusable element DA")
            .Build();
        element.SetValue("textValue", "The element text in English", "en-US");
        element.SetValue("textValue", "The element text in Danish", "da-DK");
        ElementService.Save(element);
        ElementService.Publish(element, ["en-US", "da-DK"]);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Invariant page")
            .Build();
        content.SetValue("blocks", ExternalBlockListJson(element.Key));
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        // The block editor rejects this configuration outright ("blockVariantConfigurationNotSupported"), so it is
        // only reachable programmatically. An invariant owner expands to the invariant culture only, and a
        // culture-varying property is skipped on that pass, so nothing is indexed - locally contained blocks
        // behave identically. Pinned so the unsupported combination degrades quietly instead of leaking the
        // content of an arbitrary culture.
        Assert.That(BlocksFields().SelectMany(f => f.Value.Texts ?? []), Is.Empty);
    }

    [Test]
    public async Task Can_Index_Invariant_External_Element_Content_On_Invariant_Block_List_Of_Segment_Variant_Document()
    {
        IContentType elementType = await CreateElementType(ContentVariation.Nothing);
        IContentType contentType = await CreateDocumentType(ContentVariation.Segment, elementType, ContentVariation.Nothing);

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Reusable element")
            .Build();
        element.SetValue("textValue", "The external element text");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Segment variant page")
            .Build();
        content.SetValue("blocks", ExternalBlockListJson(element.Key));
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        AssertBlocksFieldContains(BlocksFields(), f => f.Segment is null, "the default segment", "The external element text");
    }

    [Test]
    public async Task Can_Index_External_Element_Content_Under_The_Referencing_Segment()
    {
        IContentType elementType = await CreateElementType(ContentVariation.Nothing);
        IContentType contentType = await CreateDocumentType(ContentVariation.Segment, elementType);

        Element defaultElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Default segment element")
            .Build();
        defaultElement.SetValue("textValue", "Default segment element text");
        ElementService.Save(defaultElement);
        ElementService.Publish(defaultElement, ["*"]);

        Element segmentElement = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Segment one element")
            .Build();
        segmentElement.SetValue("textValue", "Segment one element text");
        ElementService.Save(segmentElement);
        ElementService.Publish(segmentElement, ["*"]);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Segment variant page")
            .Build();
        content.SetValue("blocks", ExternalBlockListJson(defaultElement.Key));
        content.SetValue("blocks", ExternalBlockListJson(segmentElement.Key), segment: "segment-1");
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        // each segment references a different external element, so each segment's content belongs under that
        // segment - the same re-homing a locally contained block gets from Property.SetValue(value, culture,
        // segment). Anything else leaves the two elements indistinguishable when a segment is searched for.
        IndexField[] blocksFields = BlocksFields();
        Assert.Multiple(() =>
        {
            AssertBlocksFieldContains(blocksFields, f => f.Segment is null, "the default segment", "Default segment element text");
            AssertBlocksFieldContains(blocksFields, f => f.Segment == "segment-1", "segment-1", "Segment one element text");
        });
    }

    [Test]
    public async Task Can_Index_All_Segments_Of_Segment_Variant_External_Element()
    {
        // the document varies by segment too - a segment-varying element inside a document that does not vary by
        // segment is a configuration the block editor rejects outright.
        IContentType elementType = await CreateElementType(ContentVariation.Segment);
        IContentType contentType = await CreateDocumentType(ContentVariation.Segment, elementType);

        Element element = new ElementBuilder()
            .WithContentType(elementType)
            .WithName("Segment variant element")
            .Build();
        element.SetValue("textValue", "Element text default segment");
        element.SetValue("textValue", "Element text segment one", segment: "segment-1");
        ElementService.Save(element);
        ElementService.Publish(element, ["*"]);

        Content content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Segment variant page")
            .Build();
        content.SetValue("blocks", ExternalBlockListJson(element.Key));
        content.SetValue("blocks", ExternalBlockListJson(element.Key), segment: "segment-1");
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        // a document's own segment-varying property has every stored segment indexed (the indexer enumerates them
        // from the property's values) - an element's segments must not be dropped just because it is embedded.
        var allTexts = BlocksFields().SelectMany(f => f.Value.Texts ?? []).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(allTexts, Does.Contain("Element text default segment"));
            Assert.That(allTexts, Does.Contain("Element text segment one"));
        });
    }

    private async Task<IContentType> CreateElementType(ContentVariation variation)
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("reusableElement")
            .WithName("Reusable Element")
            .WithIsElement(true)
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("textValue")
            .WithName("Text")
            .WithVariations(variation)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    // the "blocks" property defaults to the document type's own variation, but can be given a narrower one, so a
    // variant document can hold an invariant block list
    private async Task<IContentType> CreateDocumentType(ContentVariation variation, IContentType elementType, ContentVariation? blocksVariation = null)
    {
        var blockListDataType = new DataType(
            PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList],
            ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                ["blocks"] = new BlockListConfiguration.BlockConfiguration[]
                {
                    new() { ContentElementTypeKey = elementType.Key },
                },
            },
            Name = "My Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("pageWithBlocks")
            .WithName("Page With Blocks")
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("blocks")
            .WithName("blocks")
            .WithVariations(blocksVariation ?? variation)
            .WithDataTypeId(blockListDataType.Id)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    private string ExternalBlockListJson(Guid externalElementKey)
        => JsonSerializer.Serialize(new BlockListValue
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
        });

    private IndexField[] BlocksFields()
    {
        TestIndexDocument publishedDocument = IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Single();
        return publishedDocument.Fields.Where(f => f.FieldName == "blocks").ToArray();
    }

    private static void AssertBlocksFieldContains(IndexField[] blocksFields, Func<IndexField, bool> predicate, string variationDescription, string expectedText)
    {
        IndexField? blocksField = blocksFields.SingleOrDefault(predicate);
        if (blocksField is null)
        {
            Assert.Fail($"no \"blocks\" field was indexed for {variationDescription}");
            return;
        }

        Assert.That(blocksField.Value.Texts ?? [], Does.Contain(expectedText), $"the \"blocks\" field for {variationDescription} does not contain the external element content");
    }
}
