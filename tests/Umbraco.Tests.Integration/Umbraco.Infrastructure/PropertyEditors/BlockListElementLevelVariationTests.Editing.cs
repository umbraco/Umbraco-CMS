using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal partial class BlockListElementLevelVariationTests
{
    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Handle_Limited_User_Access_To_Languages_With_AllowEditInvariantFromNonDefault(bool updateWithLimitedUserAccess)
    {
        await LanguageService.CreateAsync(
            new Language("de-DE", "German"), Constants.Security.SuperUserKey);
        var userKey = updateWithLimitedUserAccess
            ? (await CreateLimitedUser()).Key
            : Constants.Security.SuperUserKey;

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var content = CreateContent(contentType, elementType, [], false);
        content.SetCultureName("Home (de)", "de-DE");
        ContentService.Save(content);

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
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in German", Culture = "de-DE" }
                        },
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
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in German", Culture = "de-DE" }
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);


        blockListValue.ContentData[0].Values[0].Value = "#1: The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "#1: The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "#1: The second content value in Danish";
        blockListValue.ContentData[0].Values[3].Value = "#1: The second content value in German";
        blockListValue.SettingsData[0].Values[0].Value = "#1: The second invariant settings value";
        blockListValue.SettingsData[0].Values[1].Value = "#1: The second settings value in English";
        blockListValue.SettingsData[0].Values[2].Value = "#1: The second settings value in Danish";
        blockListValue.SettingsData[0].Values[3].Value = "#1: The second settings value in German";

        blockListValue.ContentData[1].Values[0].Value = "#2: The second invariant content value";
        blockListValue.ContentData[1].Values[1].Value = "#2: The second content value in English";
        blockListValue.ContentData[1].Values[2].Value = "#2: The second content value in Danish";
        blockListValue.ContentData[1].Values[3].Value = "#2: The second content value in German";
        blockListValue.SettingsData[1].Values[0].Value = "#2: The second invariant settings value";
        blockListValue.SettingsData[1].Values[1].Value = "#2: The second settings value in English";
        blockListValue.SettingsData[1].Values[2].Value = "#2: The second settings value in Danish";
        blockListValue.SettingsData[1].Values[3].Value = "#2: The second settings value in German";

        var updateModel = new ContentUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // the Danish and invariant values should be updated regardless of the executing user
        Assert.Multiple(() =>
        {
            Assert.AreEqual("#1: The second invariant content value", blockListValue.ContentData[0].Values[0].Value);
            Assert.AreEqual("#1: The second content value in Danish", blockListValue.ContentData[0].Values[2].Value);
            Assert.AreEqual("#1: The second invariant settings value", blockListValue.SettingsData[0].Values[0].Value);
            Assert.AreEqual("#1: The second settings value in Danish", blockListValue.SettingsData[0].Values[2].Value);

            Assert.AreEqual("#2: The second invariant content value", blockListValue.ContentData[1].Values[0].Value);
            Assert.AreEqual("#2: The second content value in Danish", blockListValue.ContentData[1].Values[2].Value);
            Assert.AreEqual("#2: The second invariant settings value", blockListValue.SettingsData[1].Values[0].Value);
            Assert.AreEqual("#2: The second settings value in Danish", blockListValue.SettingsData[1].Values[2].Value);
        });

        // limited user access means English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual("#1: The first content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The first settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The first content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The first settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The first content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The first settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The first content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The first settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual("#1: The second content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The second settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The second content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The second settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The second content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The second settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The second content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The second settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Handle_Limited_User_Access_To_Languages_With_AllowEditInvariantFromNonDefault_WithoutInitialValues(bool updateWithLimitedUserAccess)
    {
        await LanguageService.CreateAsync(
            new Language("de-DE", "German"), Constants.Security.SuperUserKey);
        var userKey = updateWithLimitedUserAccess
            ? (await CreateLimitedUser()).Key
            : Constants.Security.SuperUserKey;

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var content = CreateContent(contentType, elementType, [], false);
        content.SetCultureName("Home (de)", "de-DE");
        ContentService.Save(content);


        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The second content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The second invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in German", Culture = "de-DE" }
                        },
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
                            new() { Alias = "variantText", Value = "#2: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The second content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The second content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The second invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in German", Culture = "de-DE" }
                        },
                        null,
                        null
                    )
                )
            ]
        );

        var updateModel = new ContentUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // the Danish and invariant values should be updated regardless of the executing user
        Assert.Multiple(() =>
        {
            Assert.AreEqual("#1: The second invariant content value", blockListValue.ContentData[0].Values.Single(v => v.Culture == null).Value);
            Assert.AreEqual("#1: The second content value in Danish", blockListValue.ContentData[0].Values.Single(v => v.Culture == "da-DK").Value);
            Assert.AreEqual("#1: The second invariant settings value", blockListValue.SettingsData[0].Values.Single(v => v.Culture == null).Value);
            Assert.AreEqual("#1: The second settings value in Danish", blockListValue.SettingsData[0].Values.Single(v => v.Culture == "da-DK").Value);

            Assert.AreEqual("#2: The second invariant content value", blockListValue.ContentData[1].Values.Single(v => v.Culture == null).Value);
            Assert.AreEqual("#2: The second content value in Danish", blockListValue.ContentData[1].Values.Single(v => v.Culture == "da-DK").Value);
            Assert.AreEqual("#2: The second invariant settings value", blockListValue.SettingsData[1].Values.Single(v => v.Culture == null).Value);
            Assert.AreEqual("#2: The second settings value in Danish", blockListValue.SettingsData[1].Values.Single(v => v.Culture == "da-DK").Value);
        });

        // limited user access means English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, blockListValue.ContentData[0].Values.Count);
                Assert.AreEqual(2, blockListValue.ContentData[1].Values.Count);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(4, blockListValue.ContentData[0].Values.Count);
                Assert.AreEqual(4, blockListValue.ContentData[1].Values.Count);

                Assert.AreEqual("#1: The second content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The second settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The second content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The second settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The second content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The second settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The second content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The second settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_Limited_User_Access_To_Languages_Without_AllowEditInvariantFromNonDefault(bool updateWithLimitedUserAccess)
    {
        await LanguageService.CreateAsync(
            new Language("de-DE", "German"), Constants.Security.SuperUserKey);
        var userKey = updateWithLimitedUserAccess
            ? (await CreateLimitedUser()).Key
            : Constants.Security.SuperUserKey;

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var content = CreateContent(contentType, elementType, [], false);
        content.SetCultureName("Home (de)", "de-DE");
        ContentService.Save(content);

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
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in German", Culture = "de-DE" }
                        },
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
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in German", Culture = "de-DE" }
                        },
                        null,
                        null
                    )
                )
            ]
        );

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        blockListValue.ContentData[0].Values[0].Value = "#1: The second invariant content value";
        blockListValue.ContentData[0].Values[1].Value = "#1: The second content value in English";
        blockListValue.ContentData[0].Values[2].Value = "#1: The second content value in Danish";
        blockListValue.ContentData[0].Values[3].Value = "#1: The second content value in German";
        blockListValue.SettingsData[0].Values[0].Value = "#1: The second invariant settings value";
        blockListValue.SettingsData[0].Values[1].Value = "#1: The second settings value in English";
        blockListValue.SettingsData[0].Values[2].Value = "#1: The second settings value in Danish";
        blockListValue.SettingsData[0].Values[3].Value = "#1: The second settings value in German";

        blockListValue.ContentData[1].Values[0].Value = "#2: The second invariant content value";
        blockListValue.ContentData[1].Values[1].Value = "#2: The second content value in English";
        blockListValue.ContentData[1].Values[2].Value = "#2: The second content value in Danish";
        blockListValue.ContentData[1].Values[3].Value = "#2: The second content value in German";
        blockListValue.SettingsData[1].Values[0].Value = "#2: The second invariant settings value";
        blockListValue.SettingsData[1].Values[1].Value = "#2: The second settings value in English";
        blockListValue.SettingsData[1].Values[2].Value = "#2: The second settings value in Danish";
        blockListValue.SettingsData[1].Values[3].Value = "#2: The second settings value in German";

        var updateModel = new ContentUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // the Danish values should be updated regardless of the executing user
        Assert.Multiple(() =>
        {
            Assert.AreEqual("#1: The second content value in Danish", blockListValue.ContentData[0].Values[2].Value);
            Assert.AreEqual("#1: The second settings value in Danish", blockListValue.SettingsData[0].Values[2].Value);

            Assert.AreEqual("#2: The second content value in Danish", blockListValue.ContentData[1].Values[2].Value);
            Assert.AreEqual("#2: The second settings value in Danish", blockListValue.SettingsData[1].Values[2].Value);
        });

        // limited user access means invariant, English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {

                Assert.AreEqual("#1: The first invariant content value", blockListValue.ContentData[0].Values[0].Value);
                Assert.AreEqual("#1: The first invariant settings value", blockListValue.SettingsData[0].Values[0].Value);
                Assert.AreEqual("#1: The first content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The first settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The first content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The first settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The first invariant content value", blockListValue.ContentData[1].Values[0].Value);
                Assert.AreEqual("#2: The first invariant settings value", blockListValue.SettingsData[1].Values[0].Value);
                Assert.AreEqual("#2: The first content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The first settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The first content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The first settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual("#1: The second invariant content value", blockListValue.ContentData[0].Values[0].Value);
                Assert.AreEqual("#1: The second invariant settings value", blockListValue.SettingsData[0].Values[0].Value);
                Assert.AreEqual("#1: The second content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The second settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The second content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The second settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The second invariant content value", blockListValue.ContentData[1].Values[0].Value);
                Assert.AreEqual("#2: The second invariant settings value", blockListValue.SettingsData[1].Values[0].Value);
                Assert.AreEqual("#2: The second content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The second settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The second content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The second settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_Limited_User_Access_To_Languages_Without_AllowEditInvariantFromNonDefault_WithoutInitialValue(bool updateWithLimitedUserAccess)
    {
        await LanguageService.CreateAsync(
            new Language("de-DE", "German"), Constants.Security.SuperUserKey);
        var userKey = updateWithLimitedUserAccess
            ? (await CreateLimitedUser()).Key
            : Constants.Security.SuperUserKey;

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);
        var content = CreateContent(contentType, elementType, [], false);
        content.SetCultureName("Home (de)", "de-DE");
        ContentService.Save(content);

        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The second invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The second content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The second invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The second settings value in German", Culture = "de-DE" }
                        },
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
                            new() { Alias = "variantText", Value = "#2: The second content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The second content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The second content value in German", Culture = "de-DE" }
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The second invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The second settings value in German", Culture = "de-DE" }
                        },
                        null,
                        null
                    )
                )
            ]
        );

        var updateModel = new ContentUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // the Danish values should be updated regardless of the executing user
        Assert.Multiple(() =>
        {
            Assert.AreEqual("#1: The second content value in Danish", blockListValue.ContentData[0].Values.Single(v => v.Culture == "da-DK").Value);
            Assert.AreEqual("#1: The second settings value in Danish", blockListValue.SettingsData[0].Values.Single(v => v.Culture == "da-DK").Value);

            Assert.AreEqual("#2: The second content value in Danish", blockListValue.ContentData[1].Values.Single(v => v.Culture == "da-DK").Value);
            Assert.AreEqual("#2: The second settings value in Danish", blockListValue.SettingsData[1].Values.Single(v => v.Culture == "da-DK").Value);
        });

        // limited user access means invariant, English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, blockListValue.ContentData[0].Values.Count);
                Assert.AreEqual(1, blockListValue.ContentData[1].Values.Count);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual(4, blockListValue.ContentData[0].Values.Count);
                Assert.AreEqual(4, blockListValue.ContentData[1].Values.Count);

                Assert.AreEqual("#1: The second invariant content value", blockListValue.ContentData[0].Values[0].Value);
                Assert.AreEqual("#1: The second invariant settings value", blockListValue.SettingsData[0].Values[0].Value);
                Assert.AreEqual("#1: The second content value in English", blockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("#1: The second settings value in English", blockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("#1: The second content value in German", blockListValue.ContentData[0].Values[3].Value);
                Assert.AreEqual("#1: The second settings value in German", blockListValue.SettingsData[0].Values[3].Value);

                Assert.AreEqual("#2: The second invariant content value", blockListValue.ContentData[1].Values[0].Value);
                Assert.AreEqual("#2: The second invariant settings value", blockListValue.SettingsData[1].Values[0].Value);
                Assert.AreEqual("#2: The second content value in English", blockListValue.ContentData[1].Values[1].Value);
                Assert.AreEqual("#2: The second settings value in English", blockListValue.SettingsData[1].Values[1].Value);
                Assert.AreEqual("#2: The second content value in German", blockListValue.ContentData[1].Values[3].Value);
                Assert.AreEqual("#2: The second settings value in German", blockListValue.SettingsData[1].Values[3].Value);
            });
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Handle_Limited_User_Access_To_Languages_In_Nested_Blocks_Without_Access_With_AllowEditInvariantFromNonDefault(bool updateWithLimitedUserAccess)
    {
        await LanguageService.CreateAsync(
            new Language("de-DE", "German"), Constants.Security.SuperUserKey);
        var userKey = updateWithLimitedUserAccess
            ? (await CreateLimitedUser()).Key
            : Constants.Security.SuperUserKey;
        var nestedElementType = CreateElementType(ContentVariation.Culture);
        var nestedBlockListDataType = await CreateBlockListDataType(nestedElementType);

        var rootElementType = new ContentTypeBuilder()
            .WithAlias("myRootElementType")
            .WithName("My Root Element Type")
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
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
                                new() { Alias = "variantText", Value = "The first nested content value in German", Culture = "de-DE" },
                            },
                            new List<BlockPropertyValue>
                            {
                                new() { Alias = "invariantText", Value = "The first nested invariant settings value" },
                                new() { Alias = "variantText", Value = "The first nested settings value in English", Culture = "en-US" },
                                new() { Alias = "variantText", Value = "The first nested settings value in Danish", Culture = "da-DK" },
                                new() { Alias = "variantText", Value = "The first nested settings value in German", Culture = "de-DE" },
                            },
                            null,
                            null))
                }
            },
            [],
            false);
        content.SetCultureName("Home (de)", "de-DE");
        ContentService.Save(content);

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
                    new() { Alias = "variantText", Value = "The second nested content value in German", Culture = "de-DE" },
                },
                new List<BlockPropertyValue>
                {
                    new() { Alias = "invariantText", Value = "The second nested invariant settings value" },
                    new() { Alias = "variantText", Value = "The second nested settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "The second nested settings value in Danish", Culture = "da-DK" },
                    new() { Alias = "variantText", Value = "The second nested settings value in German", Culture = "de-DE" },
                },
                null,
                null));

        var updateModel = new ContentUpdateModel
        {
            Properties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US" },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK" },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        var nestedBlocksPropertyValue = blockListValue.ContentData
            .FirstOrDefault()?.Values
            .FirstOrDefault(v => v.Alias == "nestedBlocks")?.Value?.ToString();
        Assert.IsNotNull(nestedBlocksPropertyValue);
        var nestedBlockListValue = JsonSerializer.Deserialize<BlockListValue>(nestedBlocksPropertyValue);


        // the Danish and invariant values should be updated regardless of the executing user
        Assert.Multiple(() =>
        {
            Assert.AreEqual("The second nested invariant content value", nestedBlockListValue.ContentData[0].Values[0].Value);
            Assert.AreEqual("The second nested content value in Danish", nestedBlockListValue.ContentData[0].Values[2].Value);

            Assert.AreEqual("The second nested invariant settings value", nestedBlockListValue.SettingsData[0].Values[0].Value);
            Assert.AreEqual("The second nested settings value in Danish", nestedBlockListValue.SettingsData[0].Values[2].Value);
        });

        // limited user access means English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The first nested content value in English", nestedBlockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("The first nested content value in German", nestedBlockListValue.ContentData[0].Values[3].Value);

                Assert.AreEqual("The first nested settings value in English", nestedBlockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("The first nested settings value in German", nestedBlockListValue.SettingsData[0].Values[3].Value);
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The second nested content value in English", nestedBlockListValue.ContentData[0].Values[1].Value);
                Assert.AreEqual("The second nested content value in German", nestedBlockListValue.ContentData[0].Values[3].Value);

                Assert.AreEqual("The second nested settings value in English", nestedBlockListValue.SettingsData[0].Values[1].Value);
                Assert.AreEqual("The second nested settings value in German", nestedBlockListValue.SettingsData[0].Values[3].Value);
            });
        }
    }

    [Test]
    public async Task Can_Align_Culture_Variance_For_Variant_Element_Types()
    {
        var elementType = CreateElementType(ContentVariation.Culture);
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
            false);

        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        // re-fetch content
        content = ContentService.GetById(content.Key);

        var valueEditor = (BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor)blockListDataType.Editor!.GetValueEditor();

        var blockListValue = valueEditor.ToEditor(content!.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(blockListValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.ContentData.Count);
            Assert.AreEqual(2, blockListValue.ContentData.First().Values.Count);
            var invariantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.AreEqual("en-US", variantValue.Culture);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.SettingsData.Count);
            Assert.AreEqual(2, blockListValue.SettingsData.First().Values.Count);
            var invariantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.AreEqual("en-US", variantValue.Culture);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.Expose.Count);
            Assert.AreEqual("en-US", blockListValue.Expose.First().Culture);
        });
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Nothing)]
    public async Task Can_Turn_Invariant_Element_Variant(ContentVariation contentTypeVariation)
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(contentTypeVariation, blockListDataType);

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
            false);

        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(p => p.Alias == "variantText").Variations = ContentVariation.Culture;
        ContentTypeService.Save(elementType);

        // re-fetch content
        content = ContentService.GetById(content.Key);

        var valueEditor = (BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor)blockListDataType.Editor!.GetValueEditor();

        var blockListValue = valueEditor.ToEditor(content!.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(blockListValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.ContentData.Count);
            Assert.AreEqual(2, blockListValue.ContentData.First().Values.Count);
            var invariantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.AreEqual("en-US", variantValue.Culture);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.SettingsData.Count);
            Assert.AreEqual(2, blockListValue.SettingsData.First().Values.Count);
            var invariantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.AreEqual("en-US", variantValue.Culture);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.Expose.Count);
            Assert.AreEqual("en-US", blockListValue.Expose.First().Culture);
        });
    }

    [TestCase(ContentVariation.Nothing)]
    [TestCase(ContentVariation.Culture)]
    public async Task Can_Turn_Variant_Element_Invariant(ContentVariation contentTypeVariation)
    {
        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(contentTypeVariation, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "Variant content in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "Variant content in Danish", Culture = "da-DK" }
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "Variant settings in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "Variant settings in Danish", Culture = "da-DK" }
            },
            false);

        elementType.Variations = ContentVariation.Nothing;
        elementType.PropertyTypes.First(p => p.Alias == "variantText").Variations = ContentVariation.Nothing;
        ContentTypeService.Save(elementType);

        // re-fetch content
        content = ContentService.GetById(content.Key);

        var valueEditor = (BlockListPropertyEditorBase.BlockListEditorPropertyValueEditor)blockListDataType.Editor!.GetValueEditor();

        var blockListValue = valueEditor.ToEditor(content!.Properties["blocks"]!) as BlockListValue;
        Assert.IsNotNull(blockListValue);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.ContentData.Count);
            Assert.AreEqual(2, blockListValue.ContentData.First().Values.Count);
            var invariantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.ContentData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.IsNull(variantValue.Culture);
            Assert.AreEqual("Variant content in English", variantValue.Value);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.SettingsData.Count);
            Assert.AreEqual(2, blockListValue.SettingsData.First().Values.Count);
            var invariantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "invariantText");
            var variantValue = blockListValue.SettingsData.First().Values.First(value => value.Alias == "variantText");
            Assert.IsNull(invariantValue.Culture);
            Assert.IsNull(variantValue.Culture);
            Assert.AreEqual("Variant settings in English", variantValue.Value);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.Expose.Count);
            Assert.IsNull(blockListValue.Expose.First().Culture);
        });
    }

    private async Task<IUser> CreateLimitedUser()
    {
        var userGroupService = GetRequiredService<IUserGroupService>();
        var userService = GetRequiredService<IUserService>();

        var danish = await LanguageService.GetAsync("da-DK");
        Assert.IsNotNull(danish);

        var user = UserBuilder.CreateUser();
        userService.Save(user);

        var group = UserGroupBuilder.CreateUserGroup();
        group.ClearAllowedLanguages();
        group.AddAllowedLanguage(danish.Id);

        var userGroupResult = await userGroupService.CreateAsync(group, Constants.Security.SuperUserKey, [user.Key]);
        Assert.IsTrue(userGroupResult.Success);

        return user;
    }
}
