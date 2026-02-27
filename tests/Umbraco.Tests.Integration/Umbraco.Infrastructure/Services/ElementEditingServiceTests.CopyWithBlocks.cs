using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    public static void AddElementCopyingNotificationHandlers(IUmbracoBuilder builder)
        => builder
            .AddNotificationHandler<ElementCopyingNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ElementCopyingNotification, BlockGridPropertyNotificationHandler>()
            .AddNotificationHandler<ElementCopyingNotification, RichTextPropertyNotificationHandler>();

    [TestCase(false, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(false, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(false, Constants.PropertyEditors.Aliases.RichText)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(true, Constants.PropertyEditors.Aliases.RichText)]
    [ConfigureBuilder(ActionName = nameof(AddElementCopyingNotificationHandlers))]
    public async Task Copy_Element_With_Blocks_Generates_New_Block_Keys(bool variant, string editorAlias)
    {
        var (element, originalBlockKeys) = await CreateElementWithBlocksEditor(variant, editorAlias);

        var copyResult = await ElementEditingService
            .CopyAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(copyResult.Success);

        var copy = copyResult.Result!;
        Assert.AreNotEqual(element.Key, copy.Key);

        List<Guid> newKeys = [];
        if (variant)
        {
            foreach (var culture in copy.AvailableCultures)
            {
                var newVariantBlockValue = GetBlockValue(copy, "blocks", editorAlias, culture);
                newKeys.AddRange(
                    newVariantBlockValue.Layout
                        .SelectMany(x => x.Value)
                        .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));
            }
        }
        else
        {
            var newBlockValue = GetBlockValue(copy, "blocks", editorAlias);
            newKeys.AddRange(
                newBlockValue.Layout
                    .SelectMany(x => x.Value)
                    .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));
        }

        foreach (var newKey in newKeys)
        {
            Assert.IsFalse(originalBlockKeys.Contains(newKey), "Copied element blocks should have new keys.");
        }
    }

    [TestCase(false, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(false, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(false, Constants.PropertyEditors.Aliases.RichText)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(true, Constants.PropertyEditors.Aliases.RichText)]
    [ConfigureBuilder(ActionName = nameof(AddElementCopyingNotificationHandlers))]
    public async Task Copy_Element_With_Blocks_Preserves_Block_Structure(bool variant, string editorAlias)
    {
        var (element, _) = await CreateElementWithBlocksEditor(variant, editorAlias);

        var copyResult = await ElementEditingService.CopyAsync(element.Key, null, Constants.Security.SuperUserKey);
        Assert.IsTrue(copyResult.Success);

        var copy = copyResult.Result!;

        if (variant)
        {
            foreach (var culture in copy.AvailableCultures)
            {
                AssertBlockStructurePreserved(element, copy, "blocks", editorAlias, culture);
            }
        }
        else
        {
            AssertBlockStructurePreserved(element, copy, "blocks", editorAlias);
        }
    }

    private void AssertBlockStructurePreserved(
        IElement original,
        IElement copy,
        string propertyAlias,
        string editorAlias,
        string? culture = null)
    {
        var originalRaw = original.GetValue<string>(propertyAlias, culture);
        var copiedRaw = copy.GetValue<string>(propertyAlias, culture);
        Assert.IsNotNull(originalRaw);
        Assert.IsNotNull(copiedRaw);

        var originalBlockValue = GetBlockValue(original, propertyAlias, editorAlias, culture);
        var copiedBlockValue = GetBlockValue(copy, propertyAlias, editorAlias, culture);

        var normalizedOriginal = ReplaceBlockKeys(originalRaw, originalBlockValue);
        var normalizedCopy = ReplaceBlockKeys(copiedRaw, copiedBlockValue);

        var cultureLabel = culture != null ? $" (culture: {culture})" : string.Empty;
        Assert.AreEqual(
            normalizedOriginal,
            normalizedCopy,
            $"Copied block structure should match the original{cultureLabel}.");
    }

    private static string ReplaceBlockKeys(string json, BlockValue blockValue)
    {
        const string placeholder = "00000000-0000-0000-0000-000000000000";
        var blockKeys = blockValue.ContentData.Select(d => d.Key)
            .Concat(blockValue.SettingsData.Select(d => d.Key))
            .Distinct();

        return blockKeys.Aggregate(json, (current, key) => current.Replace(key.ToString(), placeholder));
    }

    private BlockValue GetBlockValue(IElement element, string propertyAlias, string editorAlias, string? culture = null)
    {
        var rawValue = element.GetValue<string>(propertyAlias, culture);
        Assert.IsNotNull(rawValue, $"Property '{propertyAlias}' should have a value (culture: {culture ?? "invariant"}).");

        return editorAlias switch
        {
            Constants.PropertyEditors.Aliases.BlockList => JsonSerializer.Deserialize<BlockListValue>(rawValue),
            Constants.PropertyEditors.Aliases.BlockGrid => JsonSerializer.Deserialize<BlockGridValue>(rawValue),
            Constants.PropertyEditors.Aliases.RichText => JsonSerializer.Deserialize<RichTextEditorValue>(rawValue).Blocks!,
            _ => throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported."),
        };
    }

    private async Task<(IElement Element, List<Guid> BlockKeys)> CreateElementWithBlocksEditor(bool variant, string editorAlias)
    {
        // Create element type for blocks
        var blockElementType = new ContentTypeBuilder()
            .WithAlias("blockElement")
            .WithName("Block Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(blockElementType, Constants.Security.SuperUserKey);

        // Create settings element type
        var settingsElementType = new ContentTypeBuilder()
            .WithAlias("blockSettings")
            .WithName("Block Settings")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(settingsElementType, Constants.Security.SuperUserKey);

        // Create block editor data type
        var dataType = DataTypeBuilder.CreateSimpleElementDataType(
            IOHelper,
            editorAlias,
            blockElementType.Key,
            settingsElementType.Key);
        var dataTypeAttempt = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.True(dataTypeAttempt.Success, $"Failed to create data type: {dataTypeAttempt.Exception?.Message}");

        // Create library element type with blocks property
        var elementType = new ContentTypeBuilder()
            .WithAlias("elementWithBlocks")
            .WithName("Element With Blocks")
            .WithAllowAsRoot(true)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .WithContentVariation(variant ? ContentVariation.Culture : ContentVariation.Nothing)
            .Build();

        if (variant)
        {
            // Set up the da-DK language
            var language = new LanguageBuilder()
                .WithCultureInfo("da-DK")
                .Build();
            await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);
        }

        var blocksPropertyType = new PropertyTypeBuilder<ContentTypeBuilder>(new ContentTypeBuilder())
            .WithPropertyEditorAlias(editorAlias)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("blocks")
            .WithName("Blocks")
            .WithDataTypeId(dataType.Id)
            .WithVariations(variant ? ContentVariation.Culture : ContentVariation.Nothing)
            .Build();
        elementType.AddPropertyType(blocksPropertyType);

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        string?[] cultures = variant
            ? ["en-US", "da-DK"]
            : [null];

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            Variants = variant
                ? cultures.Select(c => new VariantModel { Culture = c, Name = $"Element with Blocks {c}" })
                : [new VariantModel { Name = "Element with Blocks" }],
        };

        List<Guid> allBlockKeys = [];
        foreach (var culture in cultures)
        {
            var (blockValue, blockKeys) = CreateBlockValue(editorAlias, blockElementType, settingsElementType);
            createModel.Properties = createModel.Properties.Append(
                new PropertyValueModel
                {
                    Alias = "blocks",
                    Value = JsonSerializer.Serialize(blockValue),
                    Culture = culture,
                });
            allBlockKeys.AddRange(blockKeys);
        }

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return (result.Result.Content!, allBlockKeys);
    }

    private static (object BlockValue, IEnumerable<Guid> BlockKeys) CreateBlockValue(
        string editorAlias,
        IContentType elementContentType,
        IContentType settingsContentType)
    {
        switch (editorAlias)
        {
            case Constants.PropertyEditors.Aliases.BlockList:
                return CreateBlockValueOfType<BlockListValue, BlockListLayoutItem>(editorAlias, elementContentType, settingsContentType);
            case Constants.PropertyEditors.Aliases.BlockGrid:
                return CreateBlockValueOfType<BlockGridValue, BlockGridLayoutItem>(editorAlias, elementContentType, settingsContentType);
            case Constants.PropertyEditors.Aliases.RichText:
                var res = CreateBlockValueOfType<RichTextBlockValue, RichTextBlockLayoutItem>(editorAlias, elementContentType, settingsContentType);
                return (new RichTextEditorValue
                {
                    Markup = string.Join(string.Empty, res.BlockKeys.Chunk(2).Select(c => $"<umb-rte-block data-content-key=\"{c.First()}\"></umb-rte-block>")),
                    Blocks = res.BlockValue,
                }, res.BlockKeys);
            default:
                throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported.");
        }
    }

    private static (T BlockValue, IEnumerable<Guid> BlockKeys) CreateBlockValueOfType<T, TLayout>(
        string editorAlias,
        IContentType elementContentType,
        IContentType settingsContentType)
        where T : BlockValue, new()
        where TLayout : IBlockLayoutItem, new()
    {
        const int numberOfBlocks = 2;
        var blockKeys = Enumerable.Range(0, numberOfBlocks)
            .Select(_ => Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList())
            .ToList();
        return (new T
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                [editorAlias] = blockKeys.Select(blockKeyGroup =>
                    new TLayout
                    {
                        ContentKey = blockKeyGroup[0],
                        SettingsKey = blockKeyGroup[1],
                    }).OfType<IBlockLayoutItem>(),
            },
            ContentData = blockKeys.Select(blockKeyGroup => new BlockItemData
                {
                    Key = blockKeyGroup[0],
                    ContentTypeAlias = elementContentType.Alias,
                    ContentTypeKey = elementContentType.Key,
                    Values = [],
                })
                .ToList(),
            SettingsData = blockKeys.Select(blockKeyGroup => new BlockItemData
                {
                    Key = blockKeyGroup[1],
                    ContentTypeAlias = settingsContentType.Alias,
                    ContentTypeKey = settingsContentType.Key,
                    Values = [],
                })
                .ToList(),
        }, blockKeys.SelectMany(l => l));
    }
}
