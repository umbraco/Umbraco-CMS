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
    /// <summary>
    /// Tests whether the user can update the variant values of existing blocks inside an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
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

    /// <summary>
    /// Tests whether the user can add new variant blocks to an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
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

    /// <summary>
    /// Tests whether the user can update the variant values of existing blocks inside an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
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

    /// <summary>
    /// Tests whether the user can add new variant blocks to an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) },
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] },
            },
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        blockListValue = savedBlocksValue is null ? null : JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // limited user access means invariant data is inaccessible since AllowEditInvariantFromNonDefault is disabled
        if (updateWithLimitedUserAccess)
        {
            Assert.IsNull(blockListValue);
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

                Assert.AreEqual("#1: The second content value in Danish", blockListValue.ContentData[0].Values.Single(v => v.Culture == "da-DK").Value);
                Assert.AreEqual("#1: The second settings value in Danish", blockListValue.SettingsData[0].Values.Single(v => v.Culture == "da-DK").Value);

                Assert.AreEqual("#2: The second content value in Danish", blockListValue.ContentData[1].Values.Single(v => v.Culture == "da-DK").Value);
                Assert.AreEqual("#2: The second settings value in Danish", blockListValue.SettingsData[1].Values.Single(v => v.Culture == "da-DK").Value);
            });
        }
    }

    /// <summary>
    /// Tests whether the user can add/remove new variant blocks to an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_BlockStructureManipulation_For_Limited_Users_Without_AllowEditInvariantFromNonDefault(
            bool updateWithLimitedUserAccess)
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

        var firstContentElementKey = Guid.NewGuid();
        var firstSettingsElementKey = Guid.NewGuid();

        var secondContentElementKey = Guid.NewGuid();
        var secondSettingsElementKey = Guid.NewGuid();

        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    firstContentElementKey,
                    firstSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null)),
                (
                    secondContentElementKey,
                    secondSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var newContentElementKey = Guid.NewGuid();
        RemoveBlock(blockListValue, firstContentElementKey);
        AddBlock(
            blockListValue,
            new BlockItemData
            {
                Key = newContentElementKey,
                ContentTypeAlias = elementType.Alias,
                ContentTypeKey = elementType.Key,
                Values = new List<BlockPropertyValue> {
                    new() { Alias = "invariantText", Value = "#new: The new invariant settings value" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in Danish", Culture = "da-DK" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in German", Culture = "de-DE" },
                },
            },
            null,
            elementType);

        var updateModel = new ContentUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
            },
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {
                // new one can't be added
                Assert.AreEqual(0, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == newContentElementKey));
                Assert.AreEqual(0, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#new") == true)));
                Assert.AreEqual(0, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#new") == true)));

                // can't remove first
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == firstContentElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == firstSettingsElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(4, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));

                // second wasn't touched
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == secondSettingsElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == secondContentElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
                Assert.AreEqual(4, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                // add new one, did not add settings
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == newContentElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#new") == true)));
                Assert.AreEqual(0, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#new") == true)));

                // first one removed
                Assert.AreEqual(0, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == firstContentElementKey));
                Assert.AreEqual(0, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == firstSettingsElementKey));
                Assert.AreEqual(0, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(0, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));

                // second wasn't touched
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == secondSettingsElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == secondContentElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
                Assert.AreEqual(4, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
            });
        }
    }

    /// <summary>
    /// Tests whether the user can add/remove new variant blocks to an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Can_Handle_BlockStructureManipulation_For_Limited_Users_With_AllowEditInvariantFromNonDefault(
            bool updateWithLimitedUserAccess)
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

        var firstContentElementKey = Guid.NewGuid();
        var firstSettingsElementKey = Guid.NewGuid();

        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    firstContentElementKey,
                    firstSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null)),
                (
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        var newContentElementKey = Guid.NewGuid();
        RemoveBlock(blockListValue, firstContentElementKey);
        AddBlock(
            blockListValue,
            new BlockItemData
            {
                Key = newContentElementKey,
                ContentTypeAlias = elementType.Alias,
                ContentTypeKey = elementType.Key,
                Values = new List<BlockPropertyValue> {
                    new() { Alias = "invariantText", Value = "#new: The new invariant settings value" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in English", Culture = "en-US" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in Danish", Culture = "da-DK" },
                    new() { Alias = "variantText", Value = "#new: The new settings value in German", Culture = "de-DE" },
                },
            },
            null,
            elementType);

        var updateModel = new ContentUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
            },
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        // In both cases we are allowed to change the invariant structure
        // But the amount of new cultured values we can add differs
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == newContentElementKey));
            Assert.AreEqual(0, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == firstContentElementKey));
            Assert.AreEqual(0, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == firstSettingsElementKey));
            Assert.AreEqual(updateWithLimitedUserAccess ? 2 : 4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#new") == true)));
        });
    }

    /// <summary>
    /// Tests whether the user can update the variant values of existing blocks inside an invariant blocklist
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_ClearBlocks_Limited_User_Access_To_Languages_Without_AllowEditInvariantFromNonDefault(bool updateWithLimitedUserAccess)
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

        var serializedBlockListValue = JsonSerializer.Serialize(blockListValue);
        content.Properties["blocks"]!.SetValue(serializedBlockListValue);
        ContentService.Save(content);

        var updateModel = new ContentUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = null },
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] },
            },
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();

        // limited user access means English and German should not have been updated - changes should be rolled back to the initial block values
        if (updateWithLimitedUserAccess)
        {
            Assert.NotNull(savedBlocksValue);
            Assert.AreEqual(serializedBlockListValue, savedBlocksValue);
        }
        else
        {
            Assert.IsNull(savedBlocksValue);
        }
    }

    /// <summary>
    /// Tests whether the user can add/remove a value for a given culture
    /// </summary>
    /// <param name="updateWithLimitedUserAccess">true => danish only which is not the default. false => admin which is all languages</param>
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Handle_ValueRemoval_For_Limited_Users(
            bool updateWithLimitedUserAccess)
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

        var firstContentElementKey = Guid.NewGuid();
        var firstSettingsElementKey = Guid.NewGuid();

        var secondContentElementKey = Guid.NewGuid();
        var secondSettingsElementKey = Guid.NewGuid();

        var blockListValue = BlockListPropertyValue(
            elementType,
            [
                (
                    firstContentElementKey,
                    firstSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null)),
                (
                    secondContentElementKey,
                    secondSettingsElementKey,
                    new BlockProperty(
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first content value in German", Culture = "de-DE" },
                        },
                        new List<BlockPropertyValue> {
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in German", Culture = "de-DE" },
                        },
                        null,
                        null))
            ]);

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue));
        ContentService.Save(content);

        // remove a value the limited user can remove
        blockListValue.ContentData.First().Values.RemoveAll(value => value.Culture == "da-DK");
        blockListValue.SettingsData.First().Values.RemoveAll(value => value.Culture == "da-DK");
        // remove a value the admin user can remove
        blockListValue.ContentData.First().Values.RemoveAll(value => value.Culture == "en-US");
        blockListValue.SettingsData.First().Values.RemoveAll(value => value.Culture == "en-US");

        var updateModel = new ContentUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
            },
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, userKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key);
        var savedBlocksValue = content?.Properties["blocks"]?.GetValue()?.ToString();
        Assert.NotNull(savedBlocksValue);
        blockListValue = JsonSerializer.Deserialize<BlockListValue>(savedBlocksValue);

        if (updateWithLimitedUserAccess)
        {
            Assert.Multiple(() =>
            {

                // Should only have removed the danish value
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == firstContentElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == firstSettingsElementKey));
                Assert.AreEqual(3, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(3, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(0, blockListValue.ContentData.First().Values.Count(value => value.Culture == "da-DK"));
                Assert.AreEqual(1, blockListValue.ContentData.First().Values.Count(value => value.Culture == "en-US"));
                Assert.AreEqual(0, blockListValue.SettingsData.First().Values.Count(value => value.Culture == "da-DK"));
                Assert.AreEqual(1, blockListValue.SettingsData.First().Values.Count(value => value.Culture == "en-US"));

                // second wasn't touched
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == secondSettingsElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == secondContentElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
                Assert.AreEqual(4, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                // both danish and english should be removed
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == firstContentElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == firstSettingsElementKey));
                Assert.AreEqual(2, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(2, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#1") == true)));
                Assert.AreEqual(0, blockListValue.ContentData.First().Values.Count(value => value.Culture == "da-DK"));
                Assert.AreEqual(0, blockListValue.ContentData.First().Values.Count(value => value.Culture == "en-US"));
                Assert.AreEqual(0, blockListValue.SettingsData.First().Values.Count(value => value.Culture == "da-DK"));
                Assert.AreEqual(0, blockListValue.SettingsData.First().Values.Count(value => value.Culture == "en-US"));

                // second wasn't touched
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.SettingsKey == secondSettingsElementKey));
                Assert.AreEqual(1, blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Count(layoutItem => layoutItem.ContentKey == secondContentElementKey));
                Assert.AreEqual(4, blockListValue.ContentData.Sum(contentData => contentData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
                Assert.AreEqual(4, blockListValue.SettingsData.Sum(settingsData => settingsData.Values.Count(value => value.Value?.ToString()?.StartsWith("#2") == true)));
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
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "blocks", Value = JsonSerializer.Serialize(blockListValue) }
            },
            Variants = new[]
            {
                new VariantModel { Name = content.GetCultureName("en-US")!, Culture = "en-US", Properties = [] },
                new VariantModel { Name = content.GetCultureName("da-DK")!, Culture = "da-DK", Properties = [] },
                new VariantModel { Name = content.GetCultureName("de-DE")!, Culture = "de-DE", Properties = [] }
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

    private void AddBlock(BlockListValue listValue, BlockItemData contentData, BlockItemData? settingsData, IContentType elementType)
    {
        listValue.ContentData.Add(contentData);
        if (settingsData != null)
        {
            listValue.SettingsData.Add(settingsData);
        }

        var cultures = elementType.VariesByCulture()
            ? contentData.Values.Select(value => value.Culture)
                .WhereNotNull()
                .Distinct()
                .ToArray()
            : [null];
        if (cultures.Any() is false)
        {
            cultures = [null];
        }

        var segments = elementType.VariesBySegment()
            ? contentData.Values.Select(value => value.Segment)
                .Distinct()
                .ToArray()
            : [null];

        foreach (var exposeItem in cultures.SelectMany(culture => segments.Select(segment =>
                     new BlockItemVariation(contentData.Key, culture, segment))))
        {
            listValue.Expose.Add(exposeItem);
        }


        listValue.Layout[Constants.PropertyEditors.Aliases.BlockList] = listValue
            .Layout[Constants.PropertyEditors.Aliases.BlockList]
            .Append(new BlockListLayoutItem { ContentKey = contentData.Key, SettingsKey = settingsData?.Key });
    }

    private void RemoveBlock(BlockListValue listValue, Guid blockKey)
    {
        // remove the item from the layout
        var layoutItem = listValue.Layout[Constants.PropertyEditors.Aliases.BlockList].First(x => x.ContentKey == blockKey);
        listValue.Layout[Constants.PropertyEditors.Aliases.BlockList] = listValue.Layout[Constants.PropertyEditors.Aliases.BlockList].Where(layout => layout.ContentKey != blockKey);
        listValue.ContentData.RemoveAll(contentData => contentData.Key == blockKey);
        if (layoutItem.SettingsKey != null)
        {
            listValue.SettingsData.RemoveAll(settingsData => settingsData.Key == layoutItem.SettingsKey);
        }

        listValue.Expose.RemoveAll(exposeItem => exposeItem.ContentKey == blockKey);
    }
}
