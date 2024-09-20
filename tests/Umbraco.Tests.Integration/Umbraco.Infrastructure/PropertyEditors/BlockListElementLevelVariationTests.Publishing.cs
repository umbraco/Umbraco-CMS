using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

public partial class BlockListElementLevelVariationTests
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
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
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
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" }
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
                            new() { Alias = "invariantText", Value = "#3: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#3: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#3: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null
                    )
                ),
            ]
        );

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
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
                        },
                        [],
                        null,
                        null
                    )
                )
            ]
        );

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
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second content value in Danish", Culture = "da-DK" }
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
                            new() { Alias = "invariantText", Value = "#2: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The second content value in English", Culture = "en-US" }
                        },
                        [],
                        null,
                        null
                    )
                )
            ]
        );

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

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual(string.Empty, blocks[1].Content.Value<string>("variantText"));
        });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Value<string>("variantText"));

            Assert.AreEqual("#2: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual(string.Empty, blocks[1].Content.Value<string>("variantText"));
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
}
