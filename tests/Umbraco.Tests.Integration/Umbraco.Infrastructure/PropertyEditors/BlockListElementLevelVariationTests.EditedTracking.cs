using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal sealed partial class BlockListElementLevelVariationTests
{
    [Test]
    public async Task Editing_Nested_Culture_Value_Marks_Only_That_Culture_As_Edited()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)")
            .Build();

        SetBlocksValue(content, elementType, contentElementKey, settingsElementKey, "English", "Danish");
        ContentService.Save(content);
        PublishContent(content, contentType);

        SetBlocksValue(content, elementType, contentElementKey, settingsElementKey, "English", "Danish updated");
        ContentService.Save(content);

        IContent? updated = ContentService.GetById(content.Id);
        Assert.IsNotNull(updated);
        Assert.IsTrue(updated!.IsCultureEdited("da-DK"));
        Assert.IsFalse(updated.IsCultureEdited("en-US"));
        Assert.IsFalse(updated.IsCultureEdited(Constants.System.InvariantCulture));
    }

    // A newly added block is structural (it lives in the shared Layout), but only the cultures it is
    // actually exposed to see a different rendered output - an unexposed culture ignores it entirely at
    // render time. So the add is attributed to its exposed cultures, not to the invariant, mirroring how
    // an edit to an existing block is attributed.
    [Test]
    public async Task Adding_A_Block_Exposed_To_Both_Cultures_Marks_Both_Cultures_As_Edited_Not_The_Invariant()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var firstBlockContentKey = Guid.NewGuid();
        var firstBlockSettingsKey = Guid.NewGuid();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)")
            .Build();

        SetBlocksValue(content, elementType, firstBlockContentKey, firstBlockSettingsKey, "English", "Danish");
        ContentService.Save(content);
        PublishContent(content, contentType);

        var secondBlockContentKey = Guid.NewGuid();
        var secondBlockSettingsKey = Guid.NewGuid();

        var blocks = new List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)>
        {
            (firstBlockContentKey, firstBlockSettingsKey, CreateBlockProperty("English", "Danish")),
            (secondBlockContentKey, secondBlockSettingsKey, CreateBlockProperty("English 2", "Danish 2")),
        };
        var blockListValue = BlockListPropertyValue(elementType, blocks);
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), null, null);
        ContentService.Save(content);

        IContent? updated = ContentService.GetById(content.Id);
        Assert.IsNotNull(updated);
        Assert.IsTrue(updated!.IsCultureEdited("en-US"));
        Assert.IsTrue(updated.IsCultureEdited("da-DK"));
        Assert.IsFalse(updated.IsCultureEdited(Constants.System.InvariantCulture));
    }

    [Test]
    public async Task Adding_A_Block_Exposed_To_Only_One_Culture_Marks_Only_That_Culture_As_Edited()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var firstBlockContentKey = Guid.NewGuid();
        var firstBlockSettingsKey = Guid.NewGuid();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)")
            .Build();

        SetBlocksValue(content, elementType, firstBlockContentKey, firstBlockSettingsKey, "English", "Danish");
        ContentService.Save(content);
        PublishContent(content, contentType);

        var secondBlockContentKey = Guid.NewGuid();
        var secondBlockSettingsKey = Guid.NewGuid();

        var blocks = new List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)>
        {
            (firstBlockContentKey, firstBlockSettingsKey, CreateBlockProperty("English", "Danish")),
            (secondBlockContentKey, secondBlockSettingsKey, CreateBlockPropertySingleCulture("en-US", "English only")),
        };
        var blockListValue = BlockListPropertyValue(elementType, blocks);
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), null, null);
        ContentService.Save(content);

        IContent? updated = ContentService.GetById(content.Id);
        Assert.IsNotNull(updated);
        Assert.IsTrue(updated!.IsCultureEdited("en-US"));
        Assert.IsFalse(updated.IsCultureEdited("da-DK"));
        Assert.IsFalse(updated.IsCultureEdited(Constants.System.InvariantCulture));
    }

    [Test]
    public async Task Reordering_Existing_Blocks_Marks_The_Invariant_As_Edited()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var firstBlockContentKey = Guid.NewGuid();
        var firstBlockSettingsKey = Guid.NewGuid();
        var secondBlockContentKey = Guid.NewGuid();
        var secondBlockSettingsKey = Guid.NewGuid();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)")
            .Build();

        var orderedBlocks = new List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)>
        {
            (firstBlockContentKey, firstBlockSettingsKey, CreateBlockProperty("English", "Danish")),
            (secondBlockContentKey, secondBlockSettingsKey, CreateBlockProperty("English 2", "Danish 2")),
        };
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(BlockListPropertyValue(elementType, orderedBlocks)), null, null);
        ContentService.Save(content);
        PublishContent(content, contentType);

        // same two blocks, same content, only the order changes - a structural difference not
        // explained by any block being added or removed.
        var reorderedBlocks = new List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)>
        {
            (secondBlockContentKey, secondBlockSettingsKey, CreateBlockProperty("English 2", "Danish 2")),
            (firstBlockContentKey, firstBlockSettingsKey, CreateBlockProperty("English", "Danish")),
        };
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(BlockListPropertyValue(elementType, reorderedBlocks)), null, null);
        ContentService.Save(content);

        IContent? updated = ContentService.GetById(content.Id);
        Assert.IsNotNull(updated);
        Assert.IsTrue(updated!.IsCultureEdited(Constants.System.InvariantCulture));
        Assert.IsFalse(updated.IsCultureEdited("en-US"));
        Assert.IsFalse(updated.IsCultureEdited("da-DK"));
    }

    // Canary test for the historical class of bug fixed in issue #21223 (SortBlockItemValuesByCulture),
    // now re-introduced as a risk by GetChangedCultures: comparing deserialized non-primitive
    // BlockPropertyValue.Value instances with .Equals() would spuriously report a difference (they're
    // different JsonArray/JsonObject instances even when byte-identical), wrongly flagging an untouched
    // culture as edited. Canonical-JSON-text comparison must be used instead.
    [Test]
    public async Task Editing_One_Culture_Does_Not_Falsely_Flag_An_Unrelated_Culture_With_A_NonPrimitive_Nested_Value()
    {
        var elementType = await CreateElementTypeWithPicker(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, blockListDataType);

        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)")
            .Build();

        SetBlocksValueWithPicker(content, elementType, contentElementKey, settingsElementKey, "English", "Danish");
        ContentService.Save(content);
        PublishContent(content, contentType);

        // only the en-US text changes; the picker values (non-primitive) are byte-for-byte identical
        // on both cultures, and da-DK's text is untouched.
        SetBlocksValueWithPicker(content, elementType, contentElementKey, settingsElementKey, "English updated", "Danish");
        ContentService.Save(content);

        IContent? updated = ContentService.GetById(content.Id);
        Assert.IsNotNull(updated);
        Assert.IsTrue(updated!.IsCultureEdited("en-US"));
        Assert.IsFalse(updated.IsCultureEdited("da-DK"));
        Assert.IsFalse(updated.IsCultureEdited(Constants.System.InvariantCulture));
    }

    private void SetBlocksValue(IContent content, IContentType elementType, Guid contentElementKey, Guid settingsElementKey, string englishText, string danishText)
    {
        var blockListValue = BlockListPropertyValue(elementType, contentElementKey, settingsElementKey, CreateBlockProperty(englishText, danishText));
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), null, null);
    }

    private BlockProperty CreateBlockProperty(string englishText, string danishText) => new(
        new List<BlockPropertyValue>
        {
            new() { Alias = "invariantText", Culture = null, Value = "Invariant" },
            new() { Alias = "variantText", Culture = "en-US", Value = englishText },
            new() { Alias = "variantText", Culture = "da-DK", Value = danishText },
        },
        new List<BlockPropertyValue>(),
        null,
        null);

    private BlockProperty CreateBlockPropertySingleCulture(string culture, string text) => new(
        new List<BlockPropertyValue>
        {
            new() { Alias = "invariantText", Culture = null, Value = "Invariant" },
            new() { Alias = "variantText", Culture = culture, Value = text },
        },
        new List<BlockPropertyValue>(),
        null,
        null);

    private async Task<IContentType> CreateElementTypeWithPicker(ContentVariation variation)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("myElementTypeWithPicker")
            .WithName("My Element Type With Picker")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(variation)
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
            .WithVariations(variation)
            .Done()
            .AddPropertyType()
            .WithAlias("picker")
            .WithName("Picker")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MultiUrlPicker)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithVariations(variation)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private void SetBlocksValueWithPicker(IContent content, IContentType elementType, Guid contentElementKey, Guid settingsElementKey, string englishText, string danishText)
    {
        // identical, non-primitive nested picker values for both cultures - unchanged across calls.
        object CreatePickerValue(string name, string url) => new List<object>
        {
            new Dictionary<string, object?> { ["name"] = name, ["url"] = url },
        };

        var blockContentValues = new List<BlockPropertyValue>
        {
            new() { Alias = "invariantText", Culture = null, Value = "Invariant" },
            new() { Alias = "variantText", Culture = "en-US", Value = englishText },
            new() { Alias = "variantText", Culture = "da-DK", Value = danishText },
            new() { Alias = "picker", Culture = "en-US", Value = CreatePickerValue("Home", "/") },
            new() { Alias = "picker", Culture = "da-DK", Value = CreatePickerValue("Hjem", "/da") },
        };

        var blockProperty = new BlockProperty(blockContentValues, new List<BlockPropertyValue>(), null, null);
        var blockListValue = BlockListPropertyValue(elementType, contentElementKey, settingsElementKey, blockProperty);
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockListValue), null, null);
    }
}
