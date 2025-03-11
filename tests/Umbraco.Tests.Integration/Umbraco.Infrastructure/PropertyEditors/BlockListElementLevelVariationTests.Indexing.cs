using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal partial class BlockListElementLevelVariationTests
{
    [Test]
    public async Task Can_Index_Cultures_Independently_Invariant_Blocks()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The content value (en-US)", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The content value (da-DK)", Culture = "da-DK" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The settings value (en-US)", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The settings value (da-DK)", Culture = "da-DK" },
            },
            true);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: true,
            availableCultures: ["en-US", "da-DK"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            });

        Assert.AreEqual(2, indexValues.Count());

        AssertIndexValues("en-US");
        AssertIndexValues("da-DK");

        void AssertIndexValues(string culture)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture == culture);
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, values.Length);
            Assert.Contains($"The content value ({culture})", values);
            Assert.Contains("The invariant content value", values);
        }
    }

    [Test]
    public async Task Can_Index_Cultures_Independently_Variant_Blocks()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

        var content = CreateContent(
            contentType,
            elementType,
            new []
            {
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "en-US invariantText content value" },
                        new() { Alias = "variantText", Value = "en-US variantText content value" }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "en-US invariantText settings value" },
                        new() { Alias = "variantText", Value = "en-US variantText settings value" }
                    },
                    "en-US",
                    null),
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "da-DK invariantText content value" },
                        new() { Alias = "variantText", Value = "da-DK variantText content value" }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "da-DK invariantText settings value" },
                        new() { Alias = "variantText", Value = "da-DK variantText settings value" }
                    },
                    "da-DK",
                    null)
            },
            true);

        AssertIndexValues("en-US");
        AssertIndexValues("da-DK");

        void AssertIndexValues(string culture)
        {
            var editor = blockListDataType.Editor!;
            var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
                content.Properties["blocks"]!,
                culture: culture,
                segment: null,
                published: true,
                availableCultures: ["en-US", "da-DK"],
                contentTypeDictionary: new Dictionary<Guid, IContentType>
                {
                    { elementType.Key, elementType }, { contentType.Key, contentType }
                });

            Assert.AreEqual(1, indexValues.Count());

            var indexValue = indexValues.FirstOrDefault(v => v.Culture == culture);
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            Assert.AreEqual($"{culture} invariantText content value {culture} variantText content value", TrimAndStripNewlines(indexedValue));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_Unexposed_Blocks(bool published)
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(contentType, elementType, [], false);
        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null
                    )
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null
                    )
                )
            ]
        );

        // only expose the first block in English and the second block in Danish (to make a difference between published and unpublished index values)
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, contentType, ["en-US", "da-DK"]);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: published,
            availableCultures: ["en-US", "da-DK"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            });

        Assert.AreEqual(2, indexValues.Count());
        var indexValue = indexValues.FirstOrDefault(v => v.Culture == "da-DK");
        Assert.IsNotNull(indexValue);
        Assert.AreEqual(1, indexValue.Values.Count());
        var indexedValue = indexValue.Values.First() as string;
        Assert.IsNotNull(indexedValue);
        if (published)
        {
            Assert.AreEqual("#2: The invariant content value #2: The content value in Danish", TrimAndStripNewlines(indexedValue));
        }
        else
        {
            Assert.AreEqual("#1: The invariant content value #1: The content value in Danish #2: The invariant content value #2: The content value in Danish", TrimAndStripNewlines(indexedValue));
        }

        indexValue = indexValues.FirstOrDefault(v => v.Culture == "en-US");
        Assert.IsNotNull(indexValue);
        Assert.AreEqual(1, indexValue.Values.Count());
        indexedValue = indexValue.Values.First() as string;
        Assert.IsNotNull(indexedValue);
        if (published)
        {
            Assert.AreEqual("#1: The content value in English #1: The invariant content value", TrimAndStripNewlines(indexedValue));
        }
        else
        {
            Assert.AreEqual("#1: The invariant content value #1: The content value in English #2: The invariant content value #2: The content value in English", TrimAndStripNewlines(indexedValue));
        }
    }

    [TestCase(ContentVariation.Nothing)]
    [TestCase(ContentVariation.Culture)]
    public async Task Can_Index_Invariant(ContentVariation elementTypeVariation)
    {
        var elementType = CreateElementType(elementTypeVariation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "Another invariant content value" }
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "Another invariant settings value" }
            },
            true);

        var editor = blockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: true,
            availableCultures: ["en-US"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            });

        Assert.AreEqual(1, indexValues.Count());
        var indexValue = indexValues.FirstOrDefault(v => v.Culture is null);
        Assert.IsNotNull(indexValue);
        Assert.AreEqual(1, indexValue.Values.Count());
        var indexedValue = indexValue.Values.First() as string;
        Assert.IsNotNull(indexedValue);
        var values = indexedValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(2, values.Length);
        Assert.Contains("The invariant content value", values);
        Assert.Contains("Another invariant content value", values);
    }

    [Test]
    public async Task Can_Index_Cultures_Independently_Nested_Invariant_Blocks()
    {
        var nestedElementType = CreateElementType(ContentVariation.Culture);
        var nestedBlockListDataType = await CreateBlockListDataType(nestedElementType);

        var rootElementType = new ContentTypeBuilder()
            .WithAlias("myRootElementType")
            .WithName("My Root Element Type")
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantText")
            .WithName("Variant text")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .AddPropertyType()
            .WithAlias("nestedBlocks")
            .WithName("Nested blocks")
            .WithDataTypeId(nestedBlockListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .Build();
        ContentTypeService.Save(rootElementType);
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = CreateContentType(ContentVariation.Culture, rootBlockListDataType);

        var nestedElementContentKey = Guid.NewGuid();
        var nestedElementSettingsKey = Guid.NewGuid();
        var content = CreateContent(
            contentType,
            rootElementType,
            new List<BlockPropertyValue>
            {
                new()
                {
                    Alias = "nestedBlocks",
                    Value = BlockListPropertyValue(
                        nestedElementType,
                        nestedElementContentKey,
                        nestedElementSettingsKey,
                        new BlockProperty(
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant content value" },
                                new() { Alias = "variantText", Value = "The first nested content value in English", Culture = "en-US" },
                                new() { Alias = "variantText", Value = "The first nested content value in Danish", Culture = "da-DK" },
                            },
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant settings value" },
                                new() { Alias = "variantText", Value = "The first nested settings value in English", Culture = "en-US" },
                                new() { Alias = "variantText", Value = "The first nested settings value in Danish", Culture = "da-DK" },
                            },
                            null,
                            null))
                },
                new() { Alias = "invariantText", Value = "The first root invariant content value" },
                new() { Alias = "variantText", Value = "The first root content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The first root content value in Danish", Culture = "da-DK" },
            },
            [],
            true);

        var editor = rootBlockListDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: true,
            availableCultures: ["en-US"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { nestedElementType.Key, nestedElementType }, { rootElementType.Key, rootElementType }, { contentType.Key, contentType }
            });
        Assert.AreEqual(2, indexValues.Count());

        AssertIndexedValues(
            "en-US",
            "The first root invariant content value",
            "The first root content value in English",
            "The first nested invariant content value",
            "The first nested content value in English");

        AssertIndexedValues(
            "da-DK",
            "The first root invariant content value",
            "The first root content value in Danish",
            "The first nested invariant content value",
            "The first nested content value in Danish");

        void AssertIndexedValues(string culture, params string[] expectedIndexedValues)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture == culture);
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(expectedIndexedValues.Length, values.Length);
            Assert.IsTrue(values.ContainsAll(expectedIndexedValues));
        }
    }

    private string TrimAndStripNewlines(string value)
        => value.Replace(Environment.NewLine, " ").Trim();
}
