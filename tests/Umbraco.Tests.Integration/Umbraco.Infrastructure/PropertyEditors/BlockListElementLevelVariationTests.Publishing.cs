using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

/*
If the content type varies, then:

1. Variant element types ALWAYS adds variation to the block property Expose, as well as to any variant element properties.
2. Invariant element types NEVER adds variation to the block property Expose, nor to any variant element properties (because there are none).

If the content type does NOT vary, then variation is NEVER added to Expose, nor to any variant properties - regardless of the element type variation.

This means that an invariant element cannot be "turned off" for a single variation - it's all or nothing.

It also means that in a variant setting, the parent property variance has no effect for the variance notation for any nested blocks.
*/
internal partial class BlockListElementLevelVariationTests
{
    [Test]
    public async Task Can_Publish_Cultures_Independently_Invariant_Blocks()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The first invariant content value" },
                new() { Alias = "variantText", Value = "The first content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The first content value in Danish", Culture = "da-DK" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The first invariant settings value" },
                new() { Alias = "variantText", Value = "The first settings value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The first settings value in Danish", Culture = "da-DK" },
            },
            true);

        AssertPropertyValues("en-US",
            "The first invariant content value", "The first content value in English",
            "The first invariant settings value", "The first settings value in English");

        AssertPropertyValues("da-DK",
            "The first invariant content value", "The first content value in Danish",
            "The first invariant settings value", "The first settings value in Danish");

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);
        blockListValue.ContentData[0].Values[0].Value = "The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "The second content value in Danish";
        blockListValue.SettingsData[0].Values[0].Value = "The second invariant settings value";
        blockListValue.SettingsData[0].Values[1].Value = "The second settings value in English";
        blockListValue.SettingsData[0].Values[2].Value = "The second settings value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues("en-US",
            "The second invariant content value", "The second content value in English",
            "The second invariant settings value", "The second settings value in English");

        AssertPropertyValues("da-DK",
            "The second invariant content value", "The first content value in Danish",
            "The second invariant settings value", "The first settings value in Danish");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK",
            "The second invariant content value", "The second content value in Danish",
            "The second invariant settings value", "The second settings value in Danish");

        void AssertPropertyValues(string culture,
            string expectedInvariantContentValue, string expectedVariantContentValue,
            string expectedInvariantSettingsValue, string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantContentValue, blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantContentValue, blockListItem.Content.Value<string>("variantText"));
            });

            Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantSettingsValue, blockListItem.Settings.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantSettingsValue, blockListItem.Settings.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Variant_Blocks()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
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
                        new() { Alias = "invariantText", Value = "English invariantText content value" },
                        new() { Alias = "variantText", Value = "English variantText content value" }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "English invariantText settings value" },
                        new() { Alias = "variantText", Value = "English variantText settings value" }
                    },
                    "en-US",
                    null),
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "Danish invariantText content value" },
                        new() { Alias = "variantText", Value = "Danish variantText content value" }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "Danish invariantText settings value" },
                        new() { Alias = "variantText", Value = "Danish variantText settings value" }
                    },
                    "da-DK",
                    null)
            },
            true);

        AssertPropertyValues("en-US",
            "English invariantText content value", "English variantText content value",
            "English invariantText settings value", "English variantText settings value");

        AssertPropertyValues("da-DK",
            "Danish invariantText content value", "Danish variantText content value",
            "Danish invariantText settings value", "Danish variantText settings value");

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue("en-US")!);
        blockListValue.ContentData[0].Values[0].Value = "English invariantText content value (updated)";
        blockListValue.ContentData[0].Values[1].Value = "English variantText content value (updated)";
        blockListValue.SettingsData[0].Values[0].Value = "English invariantText settings value (updated)";
        blockListValue.SettingsData[0].Values[1].Value = "English variantText settings value (updated)";
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), "en-US");

        blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue("da-DK")!);
        blockListValue.ContentData[0].Values[0].Value = "Danish invariantText content value (updated)";
        blockListValue.ContentData[0].Values[1].Value = "Danish variantText content value (updated)";
        blockListValue.SettingsData[0].Values[0].Value = "Danish invariantText settings value (updated)";
        blockListValue.SettingsData[0].Values[1].Value = "Danish variantText settings value (updated)";
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), "da-DK");

        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues("en-US",
            "English invariantText content value (updated)", "English variantText content value (updated)",
            "English invariantText settings value (updated)", "English variantText settings value (updated)");

        AssertPropertyValues("da-DK",
            "Danish invariantText content value", "Danish variantText content value",
            "Danish invariantText settings value", "Danish variantText settings value");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK",
            "Danish invariantText content value (updated)", "Danish variantText content value (updated)",
            "Danish invariantText settings value (updated)", "Danish variantText settings value (updated)");

        void AssertPropertyValues(string culture,
            string expectedInvariantContentValue, string expectedVariantContentValue,
            string expectedInvariantSettingsValue, string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantContentValue, blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantContentValue, blockListItem.Content.Value<string>("variantText"));
            });

            Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantSettingsValue, blockListItem.Settings.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantSettingsValue, blockListItem.Settings.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Nested_Invariant_Blocks()
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

        AssertPropertyValues(
            "en-US",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The first root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in English", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in English", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in English", nestedBlockSetting.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The first root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);
        blockListValue.ContentData[0].Values[0].Value = BlockListPropertyValue(
            nestedElementType,
            nestedElementContentKey,
            nestedElementSettingsKey,
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant content value" },
                    new() { Alias = "variantText", Value = "The second nested content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "The second nested content value in Danish", Culture = "da-DK" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant settings value" },
                    new() { Alias = "variantText", Value = "The second nested settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "The second nested settings value in Danish", Culture = "da-DK" },
                },
                null,
                null));
        blockListValue.ContentData[0].Values[1].Value = "The second root invariant content value";
        blockListValue.ContentData[0].Values[2].Value = "The second root content value in English";
        blockListValue.ContentData[0].Values[3].Value = "The second root content value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues(
            "en-US",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second root content value in English", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second nested content value in English", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The second nested settings value in English", nestedBlockSetting.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The second nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        void AssertPropertyValues(string culture, Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var rootBlock = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(rootBlock);
            Assert.AreEqual(1, rootBlock.Count);
            Assert.Multiple(() =>
            {
                var rootBlockContent = rootBlock.First().Content;

                var nestedBlock = rootBlockContent.Value<BlockListModel>("nestedBlocks");
                Assert.IsNotNull(nestedBlock);
                Assert.AreEqual(1, nestedBlock.Count);

                var nestedBlockContent = nestedBlock.First().Content;
                var nestedBlockSettings = nestedBlock.First().Settings;

                validateBlockValues(rootBlockContent, nestedBlockContent, nestedBlockSettings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Nested_Variant_Blocks()
    {
        var nestedElementType = CreateElementType(ContentVariation.Nothing);
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
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        ContentTypeService.Save(rootElementType);
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = CreateContentType(ContentVariation.Culture, rootBlockListDataType);

        var nestedElementContentKeyEnUs = Guid.NewGuid();
        var nestedElementSettingsKeyEnUs = Guid.NewGuid();
        var nestedElementContentKeyDaDk = Guid.NewGuid();
        var nestedElementSettingsKeyDaDk = Guid.NewGuid();
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
                        nestedElementContentKeyEnUs,
                        nestedElementSettingsKeyEnUs,
                        new BlockProperty(
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant content value" },
                                new() { Alias = "variantText", Value = "The first nested content value in English" }
                            },
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant settings value" },
                                new() { Alias = "variantText", Value = "The first nested settings value in English" }
                            },
                            null,
                            null)),
                    Culture = "en-US"
                },
                new()
                {
                    Alias = "nestedBlocks",
                    Value = BlockListPropertyValue(
                        nestedElementType,
                        nestedElementContentKeyDaDk,
                        nestedElementSettingsKeyDaDk,
                        new BlockProperty(
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant content value" },
                                new() { Alias = "variantText", Value = "The first nested content value in Danish" }
                            },
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant settings value" },
                                new() { Alias = "variantText", Value = "The first nested settings value in Danish" }
                            },
                            null,
                            null)),
                    Culture = "da-DK"
                },
                new() { Alias = "invariantText", Value = "The first root invariant content value" },
                new() { Alias = "variantText", Value = "The first root content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The first root content value in Danish", Culture = "da-DK" },
            },
            [],
            true);

        AssertPropertyValues(
            "en-US",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The first root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in English", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in English", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in English", nestedBlockSetting.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The first root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);
        blockListValue.ContentData[0].Values[0].Value = BlockListPropertyValue(
            nestedElementType,
            nestedElementContentKeyEnUs,
            nestedElementSettingsKeyEnUs,
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant content value" },
                    new() { Alias = "variantText", Value = "The second nested content value in English" }
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant settings value" },
                    new() { Alias = "variantText", Value = "The second nested settings value in English" }
                },
                null,
                null));
        blockListValue.ContentData[0].Values[1].Value = BlockListPropertyValue(
            nestedElementType,
            nestedElementContentKeyDaDk,
            nestedElementSettingsKeyDaDk,
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant content value" },
                    new() { Alias = "variantText", Value = "The second nested content value in Danish" }
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant settings value" },
                    new() { Alias = "variantText", Value = "The second nested settings value in Danish" }
                },
                null,
                null));
        blockListValue.ContentData[0].Values[2].Value = "The second root invariant content value";
        blockListValue.ContentData[0].Values[3].Value = "The second root content value in English";
        blockListValue.ContentData[0].Values[4].Value = "The second root content value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues(
            "en-US",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second root content value in English", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second nested content value in English", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The second nested settings value in English", nestedBlockSetting.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The first nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The first nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The first nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.AreEqual("The second root invariant content value", rootBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second root content value in Danish", rootBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant content value", nestedBlockContent.Value<string>("invariantText"));
                Assert.AreEqual("The second nested content value in Danish", nestedBlockContent.Value<string>("variantText"));

                Assert.AreEqual("The second nested invariant settings value", nestedBlockSetting.Value<string>("invariantText"));
                Assert.AreEqual("The second nested settings value in Danish", nestedBlockSetting.Value<string>("variantText"));
            });

        void AssertPropertyValues(string culture, Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var rootBlock = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(rootBlock);
            Assert.AreEqual(1, rootBlock.Count);
            Assert.Multiple(() =>
            {
                var rootBlockContent = rootBlock.First().Content;

                var nestedBlock = rootBlockContent.Value<BlockListModel>("nestedBlocks");
                Assert.IsNotNull(nestedBlock);
                Assert.AreEqual(1, nestedBlock.Count);

                var nestedBlockContent = nestedBlock.First().Content;
                var nestedBlockSettings = nestedBlock.First().Settings;

                validateBlockValues(rootBlockContent, nestedBlockContent, nestedBlockSettings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_With_Segments()
    {
        var elementType = CreateElementType(ContentVariation.CultureAndSegment);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.CultureAndSegment, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The first invariant content value" },
                new() { Alias = "variantText", Value = "The first content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The first content value in English (Segment 1)", Culture = "en-US", Segment = "s1" },
                new() { Alias = "variantText", Value = "The first content value in English (Segment 2)", Culture = "en-US", Segment = "s2" },
                new() { Alias = "variantText", Value = "The first content value in Danish", Culture = "da-DK" },
                new() { Alias = "variantText", Value = "The first content value in Danish (Segment 1)", Culture = "da-DK", Segment = "s1" },
                new() { Alias = "variantText", Value = "The first content value in Danish (Segment 2)", Culture = "da-DK", Segment = "s2" },
            },
            [],
            true);

        AssertPropertyValues("en-US", null,
            "The first invariant content value", "The first content value in English");

        AssertPropertyValues("en-US", "s1",
            "The first invariant content value", "The first content value in English (Segment 1)");

        AssertPropertyValues("en-US", "s2",
            "The first invariant content value", "The first content value in English (Segment 2)");

        AssertPropertyValues("da-DK", null,
            "The first invariant content value", "The first content value in Danish");

        AssertPropertyValues("da-DK", "s1",
            "The first invariant content value", "The first content value in Danish (Segment 1)");

        AssertPropertyValues("da-DK", "s2",
            "The first invariant content value", "The first content value in Danish (Segment 2)");

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);
        blockListValue.ContentData[0].Values[0].Value = "The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "The second content value in English (Segment 1)";
        blockListValue.ContentData[0].Values[3].Value = "The second content value in English (Segment 2)";
        blockListValue.ContentData[0].Values[4].Value = "The second content value in Danish";
        blockListValue.ContentData[0].Values[5].Value = "The second content value in Danish (Segment 1)";
        blockListValue.ContentData[0].Values[6].Value = "The second content value in Danish (Segment 2)";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues("en-US", null,
            "The second invariant content value", "The second content value in English");

        AssertPropertyValues("en-US", "s1",
            "The second invariant content value", "The second content value in English (Segment 1)");

        AssertPropertyValues("en-US", "s2",
            "The second invariant content value", "The second content value in English (Segment 2)");

        AssertPropertyValues("da-DK", null,
            "The second invariant content value", "The first content value in Danish");

        AssertPropertyValues("da-DK", "s1",
            "The second invariant content value", "The first content value in Danish (Segment 1)");

        AssertPropertyValues("da-DK", "s2",
            "The second invariant content value", "The first content value in Danish (Segment 2)");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", null,
            "The second invariant content value", "The second content value in Danish");

        AssertPropertyValues("da-DK", "s1",
            "The second invariant content value", "The second content value in Danish (Segment 1)");

        AssertPropertyValues("da-DK", "s2",
            "The second invariant content value", "The second content value in Danish (Segment 2)");

        void AssertPropertyValues(string culture, string? segment, string expectedInvariantContentValue, string expectedVariantContentValue)
        {
            SetVariationContext(culture, segment);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantContentValue, blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantContentValue, blockListItem.Content.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Segments()
    {
        var elementType = CreateElementType(ContentVariation.Segment);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Segment, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The first invariant content value" },
                new() { Alias = "variantText", Value = "The first content value" },
                new() { Alias = "variantText", Value = "The first content value (Segment 1)", Segment = "s1" },
                new() { Alias = "variantText", Value = "The first content value (Segment 2)", Segment = "s2" }
            },
            [],
            true);

        AssertPropertyValues(null, "The first invariant content value", "The first content value");

        AssertPropertyValues("s1", "The first invariant content value", "The first content value (Segment 1)");

        AssertPropertyValues("s2", "The first invariant content value", "The first content value (Segment 2)");

        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);
        blockListValue.ContentData[0].Values[0].Value = "The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "The second content value";
        blockListValue.ContentData[0].Values[2].Value = "The second content value (Segment 1)";
        blockListValue.ContentData[0].Values[3].Value = "The second content value (Segment 2)";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType);

        AssertPropertyValues(null, "The second invariant content value", "The second content value");

        AssertPropertyValues("s1", "The second invariant content value", "The second content value (Segment 1)");

        AssertPropertyValues("s2", "The second invariant content value", "The second content value (Segment 2)");

        void AssertPropertyValues(string? segment, string expectedInvariantContentValue, string expectedVariantContentValue)
        {
            SetVariationContext(null, segment);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);

            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedInvariantContentValue, blockListItem.Content.Value<string>("invariantText"));
                Assert.AreEqual(expectedVariantContentValue, blockListItem.Content.Value<string>("variantText"));
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Removed()
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#3: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#3: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#3: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                ),
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 3, blocks =>
        {
            Assert.AreEqual("#1: The first invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in English", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The first invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The first content value in English", blocks[1].Content.Value<string>("variantText"));

            Assert.AreEqual("#3: The first invariant content value", blocks[2].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The first content value in English", blocks[2].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 3, blocks =>
        {
            Assert.AreEqual("#1: The first invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The first invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The first content value in Danish", blocks[1].Content.Value<string>("variantText"));

            Assert.AreEqual("#3: The first invariant content value", blocks[2].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The first content value in Danish", blocks[2].Content.Value<string>("variantText"));
        });

        // remove block #2
        blockListValue.Layout[blockListValue.Layout.First().Key] =
        [
            blockListValue.Layout.First().Value.First(),
            blockListValue.Layout.First().Value.Last()
        ];
        blockListValue.ContentData.RemoveAt(1);
        blockListValue.SettingsData.RemoveAt(1);

        blockListValue.ContentData[0].Values[0].Value = "#1: The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "#1: The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "#1: The second content value in Danish";
        blockListValue.ContentData[1].Values[0].Value = "#3: The second invariant content value";
        blockListValue.ContentData[1].Values[1].Value = "#3: The second content value in English";
        blockListValue.ContentData[1].Values[2].Value = "#3: The second content value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));

        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in English", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second content value in English", blocks[1].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The first content value in Danish", blocks[1].Content.Value<string>("variantText"));
        });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second content value in Danish", blocks[1].Content.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_In_One_Language()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(contentType, elementType, [], false);
        var firstBlockContentElementKey = Guid.NewGuid();
        var firstBlockSettingsElementKey = Guid.NewGuid();
        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    firstBlockContentElementKey,
                    firstBlockSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                )
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 1, blocks =>
        {
            Assert.AreEqual("#1: The first invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in English", blocks[0].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("#1: The first invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
        });

        // Add one more block
        blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    firstBlockContentElementKey,
                    firstBlockSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#2: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The second content value in English", Culture = "en-US" }
                        },
                        [],
                        null,
                        null)
                )
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));

        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in English", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The second content value in English", blocks[1].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
        });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Exposed()
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#2: The invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null)
                )
            ]);

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The content value in English", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The content value in English", blocks[1].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("#2: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The content value in Danish", blocks[0].Content.Value<string>("variantText"));
        });

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The content value in Danish", blocks[1].Content.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Expose_Invariant_Blocks_Across_Cultures()
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The other invariant content value" }
                        },
                        [],
                        null,
                        null)
                ),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#2: The invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The other invariant content value" }
                        },
                        [],
                        null,
                        null)
                )
            ]);

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key },
            new() { ContentKey = blockListValue.ContentData[1].Key },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        foreach (var culture in new[] { "en-US", "da-DK" })
        {
            AssertPropertyValues(culture, 2, blocks =>
            {
                Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
                Assert.AreEqual("#1: The other invariant content value", blocks[0].Content.Value<string>("variantText"));

                Assert.AreEqual("#2: The invariant content value", blocks[1].Content.Value<string>("invariantText"));
                Assert.AreEqual("#2: The other invariant content value", blocks[1].Content.Value<string>("variantText"));
            });
        }

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[1].Key },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        // note how publishing in one language affects both due to the invariance of the block element type
        PublishContent(content, contentType, ["en-US"]);

        foreach (var culture in new[] { "en-US", "da-DK" })
        {
            AssertPropertyValues(culture, 1, blocks =>
            {
                Assert.AreEqual("#2: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
                Assert.AreEqual("#2: The other invariant content value", blocks[0].Content.Value<string>("variantText"));
            });
        }

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Expose_Both_Variant_And_Invariant_Blocks()
    {
        var invariantElementType = CreateElementType(ContentVariation.Nothing);
        var variantElementType = CreateElementType(ContentVariation.Culture, "myVariantElementType");
        var blockListDataType = await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = invariantElementType.Key },
                new() { ContentElementTypeKey = variantElementType.Key }
            });
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(contentType, invariantElementType, [], false);
        var blockListValue = BlockListPropertyValue(
            invariantElementType,
            [
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "#1: The invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The other invariant content value" }
                        },
                        [],
                        null,
                        null)
                )
            ]);

        var variantElementKey = Guid.NewGuid();
        blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList] = blockListValue
            .Layout[Constants.PropertyEditors.Aliases.BlockList]
            .Union(new[] { new BlockListLayoutItem(variantElementKey) });
        blockListValue.ContentData.Add(
            new BlockItemData(variantElementKey, variantElementType.Key, variantElementType.Alias)
            {
                Values = [
                    new() { Alias = "invariantText", Value = "#2: The invariant content value" },
                    new() { Alias = "variantText", Value = "#2: The variant content value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "#2: The variant content value in Danish", Culture = "da-DK" },
                ]
            });

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "en-US" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The other invariant content value", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The variant content value in English", blocks[1].Content.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The other invariant content value", blocks[0].Content.Value<string>("variantText"));
        });

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The other invariant content value", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The variant content value in Danish", blocks[1].Content.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Publish_Invariant_Properties_Without_Default_Culture_With_AllowEditInvariantFromNonDefault()
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "The first invariant content value" },
                            new() { Alias = "variantText", Value = "The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The first content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "The first invariant settings value" },
                            new() { Alias = "variantText", Value = "The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The first settings value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null)
                )
            ]);

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("en-US", 0);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("The first invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.IsNotNull(blocks[0].Settings);
            Assert.AreEqual("The first invariant settings value", blocks[0].Settings.Value<string>("invariantText"));
            Assert.AreEqual("The first settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
        });

        blockListValue.ContentData[0].Values[0].Value = "The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "The second content value in Danish";
        blockListValue.SettingsData[0].Values[0].Value = "The second invariant settings value";
        blockListValue.SettingsData[0].Values[1].Value = "The second settings value in English";
        blockListValue.SettingsData[0].Values[2].Value = "The second settings value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("en-US", 0);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual("The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("The second content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.IsNotNull(blocks[0].Settings);
            Assert.AreEqual("The second invariant settings value", blocks[0].Settings.Value<string>("invariantText"));
            Assert.AreEqual("The second settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel>? validateBlocks = null)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks?.Invoke(value);
        }
    }

    [Test]
    public async Task Cannot_Publish_Invariant_Properties_Without_Default_Culture_Without_AllowEditInvariantFromNonDefault()
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "The first invariant content value" },
                            new() { Alias = "variantText", Value = "The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The first content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "The first invariant settings value" },
                            new() { Alias = "variantText", Value = "The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The first settings value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null)
                )
            ]);

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("en-US", 0);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.AreEqual(string.Empty, blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.IsNotNull(blocks[0].Settings);
            Assert.AreEqual(string.Empty, blocks[0].Settings.Value<string>("invariantText"));
            Assert.AreEqual("The first settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel>? validateBlocks = null)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks?.Invoke(value);
        }
    }

    [Test]
    public async Task Can_Publish_Valid_Properties()
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.IsTrue(publishResult.Success);
        Assert.IsTrue(publishResult.Content.PublishedCultures.Contains("en-US"));
        Assert.IsTrue(publishResult.Content.PublishedCultures.Contains("da-DK"));
    }

    [Test]
    public async Task Can_Publish_Valid_Properties_Specific_Culture_Only()
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.IsTrue(publishResult.Success);
        Assert.IsTrue(publishResult.Content.PublishedCultures.Contains("en-US"));
        Assert.IsFalse(publishResult.Content.PublishedCultures.Contains("da-DK"));
    }

    [Test]
    public async Task Can_Publish_Valid_Properties_With_Wildcard_Culture()
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["*"]);
        Assert.IsTrue(publishResult.Success);
        Assert.IsTrue(publishResult.Content.PublishedCultures.Contains("en-US"));
        Assert.IsTrue(publishResult.Content.PublishedCultures.Contains("da-DK"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Publish_Invalid_Invariant_Properties(bool invalidSettingsValue)
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = $"{(invalidSettingsValue ? "Valid" : "Invalid")} invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = $"{(invalidSettingsValue ? "Invalid" : "Valid")} invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(publishResult.Success);
            Assert.IsNotNull(publishResult.InvalidProperties);
            Assert.AreEqual(1, publishResult.InvalidProperties.Count());
            Assert.AreEqual("blocks", publishResult.InvalidProperties.First().Alias);
        });
    }

    [Test]
    public async Task Cannot_Publish_Missing_Invariant_Properties()
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "variantText", Value = "Valid value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "Valid value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(publishResult.Success);
            Assert.IsNotNull(publishResult.InvalidProperties);
            Assert.AreEqual(1, publishResult.InvalidProperties.Count());
            Assert.AreEqual("blocks", publishResult.InvalidProperties.First().Alias);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Publish_Invalid_Variant_Properties(bool invalidSettingsValue)
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = $"{(invalidSettingsValue ? "Valid" : "Invalid")} content value in Danish", Culture = "da-DK" }
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = $"{(invalidSettingsValue ? "Invalid" : "Valid")} settings value in Danish", Culture = "da-DK" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.IsTrue(publishResult.Success);

        publishResult = ContentService.Publish(content, ["da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(publishResult.Success);
            Assert.IsNotNull(publishResult.InvalidProperties);
            Assert.AreEqual(1, publishResult.InvalidProperties.Count());
            Assert.AreEqual("blocks", publishResult.InvalidProperties.First().Alias);
        });
    }

    [Test]
    public async Task Cannot_Publish_Missing_Variant_Properties()
    {
        var elementType = CreateElementTypeWithValidation();
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
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant content value" },
                            new() { Alias = "variantText", Value = "Valid content value in English", Culture = "en-US" },
                        },
                        new List<BlockPropertyValue>
                        {
                            new() { Alias = "invariantText", Value = "Valid invariant settings value" },
                            new() { Alias = "variantText", Value = "Valid settings value in English", Culture = "en-US" },
                        },
                        null,
                        null
                    )
                )
            ]
        );

        // make sure all blocks are exposed
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.IsTrue(publishResult.Success);

        publishResult = ContentService.Publish(content, ["da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(publishResult.Success);
            Assert.IsNotNull(publishResult.InvalidProperties);
            Assert.AreEqual(1, publishResult.InvalidProperties.Count());
            Assert.AreEqual("blocks", publishResult.InvalidProperties.First().Alias);
        });
    }
}
