using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
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
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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

        AssertPropertyValues(
            "en-US",
            "The first invariant content value",
            "The first content value in English",
            "The first invariant settings value",
            "The first settings value in English");

        AssertPropertyValues(
            "da-DK",
            "The first invariant content value",
            "The first content value in Danish",
            "The first invariant settings value",
            "The first settings value in Danish");

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

        AssertPropertyValues(
            "en-US",
            "The second invariant content value",
            "The second content value in English",
            "The second invariant settings value",
            "The second settings value in English");

        AssertPropertyValues(
            "da-DK",
            "The second invariant content value",
            "The first content value in Danish",
            "The second invariant settings value",
            "The first settings value in Danish");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            "The second invariant content value",
            "The second content value in Danish",
            "The second invariant settings value",
            "The second settings value in Danish");

        void AssertPropertyValues(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });

            Assert.That(blockListItem.Settings.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Settings.Value<string>("invariantText"), Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(blockListItem.Settings.Value<string>("variantText"), Is.EqualTo(expectedVariantSettingsValue));
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Variant_Blocks()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

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

        AssertPropertyValues(
            "en-US",
            "English invariantText content value",
            "English variantText content value",
            "English invariantText settings value",
            "English variantText settings value");

        AssertPropertyValues(
            "da-DK",
            "Danish invariantText content value",
            "Danish variantText content value",
            "Danish invariantText settings value",
            "Danish variantText settings value");

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

        AssertPropertyValues(
            "en-US",
            "English invariantText content value (updated)",
            "English variantText content value (updated)",
            "English invariantText settings value (updated)",
            "English variantText settings value (updated)");

        AssertPropertyValues(
            "da-DK",
            "Danish invariantText content value",
            "Danish variantText content value",
            "Danish invariantText settings value",
            "Danish variantText settings value");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            "Danish invariantText content value (updated)",
            "Danish variantText content value (updated)",
            "Danish invariantText settings value (updated)",
            "Danish variantText settings value (updated)");

        void AssertPropertyValues(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });

            Assert.That(blockListItem.Settings.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Settings.Value<string>("invariantText"), Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(blockListItem.Settings.Value<string>("variantText"), Is.EqualTo(expectedVariantSettingsValue));
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Nested_Invariant_Blocks()
    {
        var nestedElementType = await CreateElementType(ContentVariation.Culture);
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
        await ContentTypeService.CreateAsync(rootElementType, Constants.Security.SuperUserKey);
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = await CreateContentType(ContentVariation.Culture, rootBlockListDataType);

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
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The first root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in English"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The first nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in English"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The first nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The first root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The first nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The first nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in Danish"));
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
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The second root content value in English"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The second nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The second nested content value in English"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The second nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The second nested settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The second nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The second nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in Danish"));
            });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The second root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The second nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The second nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The second nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The second nested settings value in Danish"));
            });

        void AssertPropertyValues(string culture, Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var rootBlock = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(rootBlock, Is.Not.Null);
            Assert.That(rootBlock, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                var rootBlockContent = rootBlock.First().Content;

                var nestedBlock = rootBlockContent.Value<BlockListModel>("nestedBlocks");
                Assert.That(nestedBlock, Is.Not.Null);
                Assert.That(nestedBlock, Has.Count.EqualTo(1));

                var nestedBlockContent = nestedBlock.First().Content;
                var nestedBlockSettings = nestedBlock.First().Settings;

                validateBlockValues(rootBlockContent, nestedBlockContent, nestedBlockSettings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_Nested_Variant_Blocks()
    {
        var nestedElementType = await CreateElementType(ContentVariation.Nothing);
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
        await ContentTypeService.CreateAsync(rootElementType, Constants.Security.SuperUserKey);
        var rootBlockListDataType = await CreateBlockListDataType(rootElementType);
        var contentType = await CreateContentType(ContentVariation.Culture, rootBlockListDataType);

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
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The first root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in English"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The first nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in English"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The first nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The first root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The first nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The first nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in Danish"));
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
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The second root content value in English"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The second nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The second nested content value in English"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The second nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The second nested settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The first root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The first nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The first nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The first nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The first nested settings value in Danish"));
            });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (rootBlockContent, nestedBlockContent, nestedBlockSetting) =>
            {
                Assert.That(rootBlockContent.Value<string>("invariantText"), Is.EqualTo("The second root invariant content value"));
                Assert.That(rootBlockContent.Value<string>("variantText"), Is.EqualTo("The second root content value in Danish"));

                Assert.That(nestedBlockContent.Value<string>("invariantText"), Is.EqualTo("The second nested invariant content value"));
                Assert.That(nestedBlockContent.Value<string>("variantText"), Is.EqualTo("The second nested content value in Danish"));

                Assert.That(nestedBlockSetting.Value<string>("invariantText"), Is.EqualTo("The second nested invariant settings value"));
                Assert.That(nestedBlockSetting.Value<string>("variantText"), Is.EqualTo("The second nested settings value in Danish"));
            });

        void AssertPropertyValues(string culture, Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var rootBlock = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(rootBlock, Is.Not.Null);
            Assert.That(rootBlock, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                var rootBlockContent = rootBlock.First().Content;

                var nestedBlock = rootBlockContent.Value<BlockListModel>("nestedBlocks");
                Assert.That(nestedBlock, Is.Not.Null);
                Assert.That(nestedBlock, Has.Count.EqualTo(1));

                var nestedBlockContent = nestedBlock.First().Content;
                var nestedBlockSettings = nestedBlock.First().Settings;

                validateBlockValues(rootBlockContent, nestedBlockContent, nestedBlockSettings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_Cultures_Independently_With_Segments()
    {
        var elementType = await CreateElementType(ContentVariation.CultureAndSegment);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.CultureAndSegment, blockListDataType);

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

        AssertPropertyValues(
            "en-US",
            null,
            "The first invariant content value",
            "The first content value in English");

        AssertPropertyValues(
            "en-US",
            "s1",
            "The first invariant content value",
            "The first content value in English (Segment 1)");

        AssertPropertyValues(
            "en-US",
            "s2",
            "The first invariant content value",
            "The first content value in English (Segment 2)");

        AssertPropertyValues(
            "da-DK",
            null,
            "The first invariant content value",
            "The first content value in Danish");

        AssertPropertyValues(
            "da-DK",
            "s1",
            "The first invariant content value",
            "The first content value in Danish (Segment 1)");

        AssertPropertyValues(
            "da-DK",
            "s2",
            "The first invariant content value",
            "The first content value in Danish (Segment 2)");

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

        AssertPropertyValues(
            "en-US",
            null,
            "The second invariant content value",
            "The second content value in English");

        AssertPropertyValues(
            "en-US",
            "s1",
            "The second invariant content value",
            "The second content value in English (Segment 1)");

        AssertPropertyValues(
            "en-US",
            "s2",
            "The second invariant content value",
            "The second content value in English (Segment 2)");

        AssertPropertyValues(
            "da-DK",
            null,
            "The second invariant content value",
            "The first content value in Danish");

        AssertPropertyValues(
            "da-DK",
            "s1",
            "The second invariant content value",
            "The first content value in Danish (Segment 1)");

        AssertPropertyValues(
            "da-DK",
            "s2",
            "The second invariant content value",
            "The first content value in Danish (Segment 2)");

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            null,
            "The second invariant content value",
            "The second content value in Danish");

        AssertPropertyValues(
            "da-DK",
            "s1",
            "The second invariant content value",
            "The second content value in Danish (Segment 1)");

        AssertPropertyValues(
            "da-DK",
            "s2",
            "The second invariant content value",
            "The second content value in Danish (Segment 2)");

        void AssertPropertyValues(
            string culture,
            string? segment,
            string expectedInvariantContentValue,
            string expectedVariantContentValue)
        {
            SetVariationContext(culture, segment);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Segments()
    {
        var elementType = await CreateElementType(ContentVariation.Segment);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Segment, blockListDataType);

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
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Removed()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The first invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in English"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The first invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The first content value in English"));

            Assert.That(blocks[2].Content.Value<string>("invariantText"), Is.EqualTo("#3: The first invariant content value"));
            Assert.That(blocks[2].Content.Value<string>("variantText"), Is.EqualTo("#3: The first content value in English"));
        });

        AssertPropertyValues("da-DK", 3, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The first invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in Danish"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The first invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The first content value in Danish"));

            Assert.That(blocks[2].Content.Value<string>("invariantText"), Is.EqualTo("#3: The first invariant content value"));
            Assert.That(blocks[2].Content.Value<string>("variantText"), Is.EqualTo("#3: The first content value in Danish"));
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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The second content value in English"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#3: The second content value in English"));
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in Danish"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#3: The first content value in Danish"));
        });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The second content value in Danish"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#3: The second content value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_In_One_Language()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The first invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in English"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The first invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in Danish"));
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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The second content value in English"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The second invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The second content value in English"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The first content value in Danish"));
        });

        PublishContent(content, contentType, ["da-DK"]);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The second content value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Exposed()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The content value in English"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The content value in English"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#2: The content value in Danish"));
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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The content value in Danish"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The content value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Expose_Invariant_Blocks_Across_Cultures()
    {
        var elementType = await CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
                Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The other invariant content value"));

                Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
                Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The other invariant content value"));
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
                Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
                Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#2: The other invariant content value"));
            });
        }

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Expose_Both_Variant_And_Invariant_Blocks()
    {
        var invariantElementType = await CreateElementType(ContentVariation.Nothing);
        var variantElementType = await CreateElementType(ContentVariation.Culture, "myVariantElementType");
        var blockListDataType = await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = invariantElementType.Key },
                new() { ContentElementTypeKey = variantElementType.Key }
            });
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The other invariant content value"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The variant content value in English"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The other invariant content value"));
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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The other invariant content value"));

            Assert.That(blocks[1].Content.Value<string>("invariantText"), Is.EqualTo("#2: The invariant content value"));
            Assert.That(blocks[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The variant content value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks(value);
        }
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Publish_Invariant_Properties_Without_Default_Culture_With_AllowEditInvariantFromNonDefault()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("The first invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("The first content value in Danish"));
            Assert.That(blocks[0].Settings, Is.Not.Null);
            Assert.That(blocks[0].Settings.Value<string>("invariantText"), Is.EqualTo("The first invariant settings value"));
            Assert.That(blocks[0].Settings.Value<string>("variantText"), Is.EqualTo("The first settings value in Danish"));
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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo("The second invariant content value"));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("The second content value in Danish"));
            Assert.That(blocks[0].Settings, Is.Not.Null);
            Assert.That(blocks[0].Settings.Value<string>("invariantText"), Is.EqualTo("The second invariant settings value"));
            Assert.That(blocks[0].Settings.Value<string>("variantText"), Is.EqualTo("The second settings value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel>? validateBlocks = null)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks?.Invoke(value);
        }
    }

    [Test]
    public async Task Cannot_Publish_Invariant_Properties_Without_Default_Culture_Without_AllowEditInvariantFromNonDefault()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
            Assert.That(blocks[0].Content.Value<string>("invariantText"), Is.EqualTo(string.Empty));
            Assert.That(blocks[0].Content.Value<string>("variantText"), Is.EqualTo("The first content value in Danish"));
            Assert.That(blocks[0].Settings, Is.Not.Null);
            Assert.That(blocks[0].Settings.Value<string>("invariantText"), Is.EqualTo(string.Empty));
            Assert.That(blocks[0].Settings.Value<string>("variantText"), Is.EqualTo("The first settings value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockListModel>? validateBlocks = null)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(numberOfExpectedBlocks));

            validateBlocks?.Invoke(value);
        }
    }

    [Test]
    public async Task Can_Publish_Valid_Properties()
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.Content.PublishedCultures, Does.Contain("en-US"));
        Assert.That(publishResult.Content.PublishedCultures, Does.Contain("da-DK"));
    }

    [Test]
    public async Task Can_Publish_Valid_Properties_Specific_Culture_Only()
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.Content.PublishedCultures, Does.Contain("en-US"));
        Assert.That(publishResult.Content.PublishedCultures, Does.Not.Contain("da-DK"));
    }

    [Test]
    public async Task Can_Publish_Valid_Properties_With_Wildcard_Culture()
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["*"]);
        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.Content.PublishedCultures, Does.Contain("en-US"));
        Assert.That(publishResult.Content.PublishedCultures, Does.Contain("da-DK"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Publish_Invalid_Invariant_Properties(bool invalidSettingsValue)
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.That(publishResult.Success, Is.False);
            Assert.That(publishResult.InvalidProperties, Is.Not.Null);
            Assert.That(publishResult.InvalidProperties.Count(), Is.EqualTo(1));
            Assert.That(publishResult.InvalidProperties.First().Alias, Is.EqualTo("blocks"));
        });
    }

    [Test]
    public async Task Cannot_Publish_Missing_Invariant_Properties()
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US", "da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.That(publishResult.Success, Is.False);
            Assert.That(publishResult.InvalidProperties, Is.Not.Null);
            Assert.That(publishResult.InvalidProperties.Count(), Is.EqualTo(1));
            Assert.That(publishResult.InvalidProperties.First().Alias, Is.EqualTo("blocks"));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Publish_Invalid_Variant_Properties(bool invalidSettingsValue)
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.That(publishResult.Success, Is.True);

        publishResult = ContentService.Publish(content, ["da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.That(publishResult.Success, Is.False);
            Assert.That(publishResult.InvalidProperties, Is.Not.Null);
            Assert.That(publishResult.InvalidProperties.Count(), Is.EqualTo(1));
            Assert.That(publishResult.InvalidProperties.First().Alias, Is.EqualTo("blocks"));
        });
    }

    [Test]
    public async Task Cannot_Publish_Missing_Variant_Properties()
    {
        var elementType = await CreateElementTypeWithValidation();
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                        null))
            ]);

        // make sure all blocks are exposed
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, ["en-US"]);
        Assert.That(publishResult.Success, Is.True);

        publishResult = ContentService.Publish(content, ["da-DK"]);
        Assert.Multiple(() =>
        {
            Assert.That(publishResult.Success, Is.False);
            Assert.That(publishResult.InvalidProperties, Is.Not.Null);
            Assert.That(publishResult.InvalidProperties.Count(), Is.EqualTo(1));
            Assert.That(publishResult.InvalidProperties.First().Alias, Is.EqualTo("blocks"));
        });
    }

    [Test]
    public async Task Can_Handle_Elements_Level_Property_Cache()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myElementType")
            .WithName("My Element Type")
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("contentPicker")
            .WithName("Content Picker")
            .WithDataTypeId(1046)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Culture)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var pickedContent1 = CreateContent(contentType, elementType, [], true);
        var pickedContent2 = CreateContent(contentType, elementType, [], true);

        var content = CreateContent(
            contentType,
            elementType,
            [
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "contentPicker", Value = pickedContent1.GetUdi().ToString(), Culture = "en-US" },
                        new() { Alias = "contentPicker", Value = pickedContent2.GetUdi().ToString(), Culture = "da-DK" },
                    },
                    [],
                    null,
                    null)
            ],
            true);

        AssertPropertyValues("en-US", pickedContent1);

        AssertPropertyValues("da-DK", pickedContent2);

        void AssertPropertyValues(string culture, IContent expectedPickedContent)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(1));
            // need to ensure the property type cache level, otherwise this test has little value
            Assert.That(blockListItem.Content.Properties.First().PropertyType.CacheLevel, Is.EqualTo(PropertyCacheLevel.Elements));
            var actualPickedPublishedContent = blockListItem.Content.Value<IPublishedContent>("contentPicker");
            Assert.That(actualPickedPublishedContent, Is.Not.Null);
            Assert.That(actualPickedPublishedContent.Key, Is.EqualTo(expectedPickedContent.Key));
        }
    }

    [TestCase(ContentVariation.Culture, false)]
    [TestCase(ContentVariation.Culture, true)]
    [TestCase(ContentVariation.Nothing, false)]
    [TestCase(ContentVariation.Nothing, true)]
    public async Task Can_Perform_Language_Fallback(ContentVariation elementTypeVariation, bool performFallbackToDefaultLanguage)
    {
        var daDkLanguage = await LanguageService.GetAsync("da-DK");
        Assert.That(daDkLanguage, Is.Not.Null);
        daDkLanguage.FallbackIsoCode = "en-US";
        var saveLanguageResult = await LanguageService.UpdateAsync(daDkLanguage, Constants.Security.SuperUserKey);
        Assert.That(saveLanguageResult.Success, Is.True);

        daDkLanguage = await LanguageService.GetAsync("da-DK");
        Assert.That(daDkLanguage?.FallbackIsoCode, Is.EqualTo("en-US"));

        var elementType = await CreateElementType(elementTypeVariation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

        var content = CreateContent(
            contentType,
            elementType,
            new []
            {
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "English invariantText content value" },
                        new() { Alias = "variantText", Value = "English variantText content value", Culture = elementTypeVariation.VariesByCulture() ? "en-US" : null }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "English invariantText settings value" },
                        new() { Alias = "variantText", Value = "English variantText settings value", Culture = elementTypeVariation.VariesByCulture() ? "en-US" : null }
                    },
                    "en-US",
                    null)
            },
            true);

        AssertPropertyValuesWithFallback(
            "en-US",
            "English invariantText content value",
            "English variantText content value",
            "English invariantText settings value",
            "English variantText settings value");

        AssetEmptyPropertyValues("da-DK");

        AssertPropertyValuesWithFallback(
            "da-DK",
            "English invariantText content value",
            "English variantText content value",
            "English invariantText settings value",
            "English variantText settings value");

        void AssertPropertyValuesWithFallback(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var fallback = performFallbackToDefaultLanguage ? Fallback.ToDefaultLanguage : Fallback.ToLanguage;

            var publishedValueFallback = GetRequiredService<IPublishedValueFallback>();
            var value = publishedContent.Value<BlockListModel>(publishedValueFallback, "blocks", fallback: fallback);
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.That(blockListItem.Content.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText", culture: "en-US"), Is.EqualTo(expectedVariantContentValue));
            });

            Assert.That(blockListItem.Settings.Properties.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Settings.Value<string>("invariantText"), Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(blockListItem.Settings.Value<string>("variantText", culture: "en-US"), Is.EqualTo(expectedVariantSettingsValue));
            });
        }

        void AssetEmptyPropertyValues(string culture)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Is.Empty);
        }
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Publishing_After_Changing_Element_Property_From_Variant_To_Invariant_Does_Not_Keep_Old_Culture_Specific_Values(bool republishEnglish, bool republishDanish)
    {
        // 1. Create element type WITH culture variation
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        // 2. Create content with variant values and publish all cultures
        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The content value in Danish", Culture = "da-DK" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The settings value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The settings value in Danish", Culture = "da-DK" },
            },
            true);

        // Verify initial state
        AssertPropertyValues(
            "en-US",
            "The invariant content value",
            "The content value in English",
            "The invariant settings value",
            "The settings value in English");

        AssertPropertyValues(
            "da-DK",
            "The invariant content value",
            "The content value in Danish",
            "The invariant settings value",
            "The settings value in Danish");

        // 3. Change element property type to invariant (remove culture variation)
        foreach (var propertyType in elementType.PropertyTypes.Where(pt => pt.Alias == "variantText"))
        {
            propertyType.Variations = ContentVariation.Nothing;
        }

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        // RefreshContentTypeCache(elementType, contentType);

        // Verify element type properties are now invariant
        var refreshedElementType = ContentTypeService.Get(elementType.Key);
        Assert.That(refreshedElementType, Is.Not.Null, "Element type should exist after save");
        var variantTextProperty = refreshedElementType!.PropertyTypes.FirstOrDefault(p => p.Alias == "variantText");
        Assert.That(variantTextProperty, Is.Not.Null, "variantText property should exist");
        Assert.That(variantTextProperty!.VariesByCulture(), Is.False, $"variantText should NOT vary by culture after element type change. Variations={variantTextProperty.Variations}");

        // 4. Update the content values - simulating real-world scenario where:
        //    - Old variant values remain in the published JSON value (leftover from before element type change)
        //    - New invariant values replace the variant ones in the draft JSON value (after element type became invariant)
        //    This is the critical scenario that triggers the bug.
        var blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);

        // Update variant to invariant values
        blockListValue!.ContentData[0].Values.RemoveAll(value => value.Alias == "variantText");
        blockListValue.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The invariant content value",
            Culture = null
        });

        blockListValue.SettingsData[0].Values.RemoveAll(value => value.Alias == "variantText");
        blockListValue.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The invariant settings value",
            Culture = null
        });

        // Update Expose to only have invariant entry (no culture)
        blockListValue.Expose = blockListValue.Expose
            .Select(e => new BlockItemVariation(e.ContentKey, null, null))
            .DistinctBy(e => e.ContentKey)
            .ToList();

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        // 5. Publish selected cultures
        string[] culturesToPublish = republishEnglish && republishDanish
            ? ["en-US", "da-DK"]
            : republishEnglish
                ? ["en-US"]
                : republishDanish
                    ? ["da-DK"]
                    : throw new ArgumentException("Can't proceed without republishing at least one culture");
        PublishContent(content, contentType, culturesToPublish);

        // 6. Verify: Get the published value and check for duplicates in the raw data
        content = ContentService.GetById(content.Key)!;
        var publishedValue = (string?)content.Properties["blocks"]!.GetValue(null, null, published: true);
        Assert.That(publishedValue, Is.Not.Null, "Published value should not be null");

        var publishedBlockListValue = JsonSerializer.Deserialize<BlockListValue>(publishedValue);
        Assert.That(publishedBlockListValue, Is.Not.Null);

        // Verify ContentData entries are not duplicated
        Assert.That(publishedBlockListValue.ContentData, Has.Count.EqualTo(1), "Should have exactly 1 content data entry");
        Assert.That(publishedBlockListValue.SettingsData, Has.Count.EqualTo(1), "Should have exactly 1 settings data entry");

        // Verify Values within each ContentData are not duplicated
        // The bug causes the same property alias to appear multiple times with different cultures
        // (e.g., variantText:en-US AND variantText:null both present)
        // After element type becomes invariant, there should only be ONE value per alias (the invariant one)
        var aliasGroups = publishedBlockListValue.ContentData[0].Values.GroupBy(v => v.Alias);
        foreach (var group in aliasGroups)
        {
            Assert.That(
                group.Count(), Is.EqualTo(1),
                $"Property '{group.Key}' has multiple values with different cultures. Values: {string.Join(", ", group.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");
        }

        aliasGroups = publishedBlockListValue.SettingsData[0].Values.GroupBy(v => v.Alias);
        foreach (var group in aliasGroups)
        {
            Assert.That(
                group.Count(), Is.EqualTo(1),
                $"Property '{group.Key}' has multiple values with different cultures. Values: {string.Join(", ", group.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");
        }

        // Verify Expose entries are not duplicated
        Assert.That(publishedBlockListValue.Expose, Has.Count.EqualTo(1));

        void AssertPropertyValues(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });

            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Settings.Value<string>("invariantText"), Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(blockListItem.Settings.Value<string>("variantText"), Is.EqualTo(expectedVariantSettingsValue));
            });
        }
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Publishing_After_Changing_Element_Property_From_Invariant_To_Variant_Does_Not_Keep_Old_Invariant_Values(bool republishEnglish, bool republishDanish)
    {
        // 1. Create variant element type WITHOUT variant properties
        var elementType = await CreateElementType(ContentVariation.Culture);
        elementType.PropertyTypes.First(p => p.Alias == "variantText").Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        // 2. Create content with invariant values and publish
        var content = CreateContent(contentType, elementType, [], false);
        var blockListValue = BlockListPropertyValue(
            elementType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BlockProperty(
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The invariant content value" },
                    new() { Alias = "variantText", Value = "The original invariant value for content" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The invariant settings value" },
                    new() { Alias = "variantText", Value = "The original invariant value for settings" },
                },
                null,
                null));

        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US", "da-DK"]);

        // Verify initial state - both cultures should see the same invariant value
        AssertPropertyValues(
            "en-US",
            "The invariant content value",
            "The original invariant value for content",
            "The invariant settings value",
            "The original invariant value for settings");

        AssertPropertyValues(
            "da-DK",
            "The invariant content value",
            "The original invariant value for content",
            "The invariant settings value",
            "The original invariant value for settings");

        // 3. Change element type to variant (add culture variation)
        elementType.Variations = ContentVariation.Culture;
        foreach (var propertyType in elementType.PropertyTypes.Where(pt => pt.Alias == "variantText"))
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // Verify element type properties are now variant
        var refreshedElementType = ContentTypeService.Get(elementType.Key);
        Assert.That(refreshedElementType, Is.Not.Null, "Element type should exist after save");
        var variantTextProperty = refreshedElementType!.PropertyTypes.FirstOrDefault(p => p.Alias == "variantText");
        Assert.That(variantTextProperty, Is.Not.Null, "variantText property should exist");
        Assert.That(variantTextProperty!.VariesByCulture(), Is.True, $"variantText should vary by culture after element type change. Variations={variantTextProperty.Variations}");

        // 4. Update the content values - simulating real-world scenario where:
        //    - Old invariant value remains in the JSON (leftover from before element type change)
        //    - New variant values are added (after element type became variant)
        //    This is the critical scenario that could trigger a bug.
        blockListValue = JsonSerializer.Deserialize<BlockListValue>((string)content.Properties["blocks"]!.GetValue()!);

        // Add variant values for the property that is now variant
        blockListValue!.ContentData[0].Values.RemoveAll(value => value.Alias == "variantText");
        blockListValue.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The content value in English",
            Culture = "en-US"
        });
        blockListValue.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The content value in Danish",
            Culture = "da-DK"
        });

        blockListValue.SettingsData[0].Values.RemoveAll(value => value.Alias == "variantText");
        blockListValue.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The settings value in English",
            Culture = "en-US"
        });
        blockListValue.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The settings value in Danish",
            Culture = "da-DK"
        });

        // Update Expose to have culture-specific entries
        var contentKey = blockListValue.Expose[0].ContentKey;
        blockListValue.Expose =
        [
            new BlockItemVariation(contentKey, "en-US", null),
            new BlockItemVariation(contentKey, "da-DK", null)
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        // 5. Publish selected cultures
        string[] culturesToPublish = republishEnglish && republishDanish
            ? ["en-US", "da-DK"]
            : republishEnglish
                ? ["en-US"]
                : republishDanish
                    ? ["da-DK"]
                    : throw new ArgumentException("Can't proceed without republishing at least one culture");
        PublishContent(content, contentType, culturesToPublish);

        // 6. Verify published JSON doesn't contain old invariant values for variantText
        content = ContentService.GetById(content.Key)!;
        var publishedValue = (string?)content.Properties["blocks"]!.GetValue(null, null, published: true);
        Assert.That(publishedValue, Is.Not.Null, "Published value should not be null");

        var publishedBlockListValue = JsonSerializer.Deserialize<BlockListValue>(publishedValue);
        Assert.That(publishedBlockListValue, Is.Not.Null);

        // Verify ContentData entries are not duplicated
        Assert.That(publishedBlockListValue!.ContentData, Has.Count.EqualTo(1), "Should have exactly 1 content data entry");
        Assert.That(publishedBlockListValue.SettingsData, Has.Count.EqualTo(1), "Should have exactly 1 settings data entry");

        var variantTextValues = publishedBlockListValue.ContentData[0].Values.Where(v => v.Alias == "variantText").ToList();
        Assert.That(
            variantTextValues.Any(v => v.Culture is null), Is.False,
            $"variantText property should not have invariant values after changing to variant. Values: {string.Join(", ", variantTextValues.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");

        variantTextValues = publishedBlockListValue.SettingsData[0].Values.Where(v => v.Alias == "variantText").ToList();
        Assert.That(
            variantTextValues.Any(v => v.Culture is null), Is.False,
            $"variantText property should not have invariant values after changing to variant. Values: {string.Join(", ", variantTextValues.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");

        // Verify Expose entries are not duplicated
        var exposeGroups = publishedBlockListValue.Expose.GroupBy(e => (e.ContentKey, e.Culture, e.Segment));
        Assert.That(
            exposeGroups.All(g => g.Count() == 1), Is.True,
            $"Duplicate Expose entries found. Expose: {string.Join(", ", publishedBlockListValue.Expose.Select(e => $"{e.ContentKey}:{e.Culture}:{e.Segment}"))}");

        void AssertPropertyValues(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockListModel>("blocks");
            Assert.That(value, Is.Not.Null);
            Assert.That(value!, Has.Count.EqualTo(1));

            var blockListItem = value.First();
            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Content.Value<string>("invariantText"), Is.EqualTo(expectedInvariantContentValue));
                Assert.That(blockListItem.Content.Value<string>("variantText"), Is.EqualTo(expectedVariantContentValue));
            });

            Assert.Multiple(() =>
            {
                Assert.That(blockListItem.Settings.Value<string>("invariantText"), Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(blockListItem.Settings.Value<string>("variantText"), Is.EqualTo(expectedVariantSettingsValue));
            });
        }
    }

    /// <summary>
    /// Regression test for GitHub issue #21223.
    /// When a document with a culture-variant content type has an invariant BlockList property
    /// containing culture-variant blocks, and you publish all languages for the first time,
    /// the content should NOT show as having pending changes in either the default or other language.
    /// </summary>
    [Test]
    public async Task Publishing_All_Cultures_Should_Not_Mark_Content_As_Edited()
    {
        // Arrange: Create culture-variant content type with INVARIANT BlockList property
        // containing culture-variant element type.
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        // Create content without publishing first.
        var content = CreateContent(
            contentType,
            elementType,
            [
                new() { Alias = "invariantText", Value = "Invariant content value" },
                new() { Alias = "variantText", Value = "English content value", Culture = "en-US" },
                new() { Alias = "variantText", Value = "Danish content value", Culture = "da-DK" },
            ],
            [
                new() { Alias = "invariantText", Value = "Invariant settings value" },
                new() { Alias = "variantText", Value = "English settings value", Culture = "en-US" },
                new() { Alias = "variantText", Value = "Danish settings value", Culture = "da-DK" },
            ],
            publishContent: false);

        // Get the current block list value as JSON.
        var currentBlocksJson = (string)content.Properties["blocks"]!.GetValue()!;

        // Simulate backoffice edit workflow by going through ContentEditingService.UpdateAsync,
        // which processes values through FromEditor (where sorting happens).
        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "blocks", Value = currentBlocksJson },
            ],
            Variants =
            [
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
            ],
        };

        var updateResult = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True, "Content update should succeed");

        // Reload content to get fresh state.
        content = ContentService.GetById(content.Key)!;

        // Publish ALL cultures.
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        // Reload content from database to get fresh state with Edited flags set correctly.
        content = ContentService.GetById(content.Key)!;

        // Assert: Content should NOT show as edited for any culture after initial publish.
        // Bug #21223: Without the fix, the default language variant incorrectly shows as having
        // pending changes due to JSON serialization order differences between EditedValue and PublishedValue.
        Assert.Multiple(() =>
        {
            Assert.That(content.Edited, Is.False, "Content should not be marked as edited after publishing all cultures");
            Assert.That(content.IsCultureEdited("en-US"), Is.False, "en-US culture should not be marked as edited");
            Assert.That(content.IsCultureEdited("da-DK"), Is.False, "da-DK culture should not be marked as edited");
        });
    }

    [Test]
    public async Task Can_Perform_Language_Fallback_At_Block_Property_Level()
    {
        var defaultCulture = await GetRequiredService<ILanguageService>().GetDefaultIsoCodeAsync();
        Assert.That(defaultCulture, Is.EqualTo("en-US"));

        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

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
                            new() { Alias = "variantText", Value = "#2: The content value in Danish", Culture = "da-DK" },
                        },
                        [],
                        null,
                        null)
                ),
            ]);

        // make sure all blocks are exposed in both languages (the helper method won't detect some of them due to lacking language values)
        blockListValue.Expose =
        [
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[0].Key, Culture = "da-DK" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "en-US" },
            new() { ContentKey = blockListValue.ContentData[1].Key, Culture = "da-DK" },
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);
        PublishContent(content, contentType, ["en-US", "da-DK"]);

        SetVariationContext("en-US", null);

        var publishedContent = GetPublishedContent(content.Key);
        var value = publishedContent.Value<BlockListModel>("blocks");
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Has.Count.EqualTo(2));

        // assert property values in the English variation context.
        Assert.Multiple(() =>
        {
            // start block #1

            // "invariantText" has a value, so it won't perform fallback.
            Assert.That(value[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(value[0].Content.Value<string>("invariantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("#1: The invariant content value"));

            // "variantText" has a value in English (both implicit and explicit), so it won't perform fallback.
            Assert.That(value[0].Content.Value<string>("variantText"), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "en-US"), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "en-US", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("#1: The content value in English"));

            // "variantText" has no value in Danish, so it will perform fallback - both to the default language (en-US) and to a default value.
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK"), Is.EqualTo(string.Empty));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultLanguage), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));

            // start block #2

            // "invariantText" has no value, so it will perform fallback.
            Assert.That(value[1].Content.Value<string>("invariantText"), Is.EqualTo(string.Empty));
            Assert.That(value[1].Content.Value<string>("invariantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));

            // "variantText" has no value in English (neither implicit nor explicit), so it will perform fallback.
            Assert.That(value[1].Content.Value<string>("variantText"), Is.EqualTo(string.Empty));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "en-US"), Is.EqualTo(string.Empty));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "en-US", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));

            // "variantText" has a value in Danish, so it won't perform fallback.
            Assert.That(value[1].Content.Value<string>("variantText", culture: "da-DK"), Is.EqualTo("#2: The content value in Danish"));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultLanguage), Is.EqualTo("#2: The content value in Danish"));
        });

        SetVariationContext("da-DK", null);

        publishedContent = GetPublishedContent(content.Key);
        value = publishedContent.Value<BlockListModel>("blocks");
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Has.Count.EqualTo(2));

        // assert property values in the Danish variation context.
        Assert.Multiple(() =>
        {
            // start block #1

            // "invariantText" has a value, so it won't perform fallback.
            Assert.That(value[0].Content.Value<string>("invariantText"), Is.EqualTo("#1: The invariant content value"));
            Assert.That(value[0].Content.Value<string>("invariantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("#1: The invariant content value"));

            // "variantText" has no value in Danish (neither implicit nor explicit), so it will perform fallback - both to the default language (en-US) and to a default value.
            Assert.That(value[0].Content.Value<string>("variantText"), Is.EqualTo(string.Empty));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK"), Is.EqualTo(string.Empty));
            Assert.That(value[0].Content.Value<string>("variantText", fallback: Fallback.ToDefaultLanguage), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultLanguage), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));

            // "variantText" has a value in English, so it won't perform fallback.
            Assert.That(value[0].Content.Value<string>("variantText", culture: "en-US"), Is.EqualTo("#1: The content value in English"));
            Assert.That(value[0].Content.Value<string>("variantText", culture: "en-US", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("#1: The content value in English"));

            // start block #2

            // "invariantText" has no value, so it will perform fallback.
            Assert.That(value[1].Content.Value<string>("invariantText"), Is.EqualTo(string.Empty));
            Assert.That(value[1].Content.Value<string>("invariantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));

            // "variantText" has a value in Danish, so it won't perform fallback.
            Assert.That(value[1].Content.Value<string>("variantText"), Is.EqualTo("#2: The content value in Danish"));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "da-DK"), Is.EqualTo("#2: The content value in Danish"));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "da-DK", fallback: Fallback.ToDefaultLanguage), Is.EqualTo("#2: The content value in Danish"));
            Assert.That(value[1].Content.Value<string>("variantText", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("#2: The content value in Danish"));

            // "variantText" has no value in English, so it will perform fallback.
            Assert.That(value[1].Content.Value<string>("variantText", culture: "en-US"), Is.EqualTo(string.Empty));
            Assert.That(value[1].Content.Value<string>("variantText", culture: "en-US", fallback: Fallback.ToDefaultValue, defaultValue: "The default value"), Is.EqualTo("The default value"));
        });
    }
}
