using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Attributes;
using IContent = Umbraco.Cms.Core.Models.IContent;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    public static void AddScaffoldedNotificationHandler(IUmbracoBuilder builder)
        => builder
            .AddNotificationHandler<ContentScaffoldedNotification, ContentScaffoldedNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockGridPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, RichTextPropertyNotificationHandler>();

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(AddScaffoldedNotificationHandler))]
    public async Task Can_Get_Scaffold(bool variant)
    {
        var blueprint = await (variant ? CreateVariantContentBlueprint() : CreateInvariantContentBlueprint());
        try
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = notification =>
            {
                foreach (var propertyValue in notification.Scaffold.Properties.SelectMany(property => property.Values))
                {
                    propertyValue.EditedValue += " scaffolded edited";
                    propertyValue.PublishedValue += " scaffolded published";
                }
            };
            var result = await ContentBlueprintEditingService.GetScaffoldedAsync(blueprint.Key);
            Assert.IsNotNull(result);
            Assert.AreNotEqual(blueprint.Key, result.Key);
            Assert.AreEqual(
                blueprint.ContentType.Key,
                result.ContentType.Key,
                "The content type of the scaffolded content should match the original blueprint content type.");
            Assert.AreEqual(
                blueprint.Properties.Select(p => (p.Alias, p.PropertyType.Key)),
                result.Properties.Select(p => (p.Alias, p.PropertyType.Key)),
                "The properties of the scaffolded content should match the original blueprint properties.");

            var propertyValues = result.Properties.SelectMany(property => property.Values).ToArray();
            Assert.IsNotEmpty(propertyValues);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(propertyValues.All(value => value.EditedValue is string stringValue && stringValue.EndsWith(" scaffolded edited")));
                Assert.IsTrue(propertyValues.All(value => value.PublishedValue is string stringValue && stringValue.EndsWith(" scaffolded published")));
            });
        }
        finally
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = null;
        }
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Scaffold()
    {
        var result = await ContentBlueprintEditingService.GetScaffoldedAsync(Guid.NewGuid());
        Assert.IsNull(result);
    }

    [TestCase(false, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(false, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(false, Constants.PropertyEditors.Aliases.RichText)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockList)]
    [TestCase(true, Constants.PropertyEditors.Aliases.BlockGrid)]
    [TestCase(true, Constants.PropertyEditors.Aliases.RichText)]
    [ConfigureBuilder(ActionName = nameof(AddScaffoldedNotificationHandler))]
    public async Task Get_Scaffold_With_Blocks_Generates_New_Block_Ids(bool variant, string editorAlias)
    {
        var blueprint = await CreateBlueprintWithBlocksEditor(variant, editorAlias);
        var result = await ContentBlueprintEditingService.GetScaffoldedAsync(blueprint.Content.Key);
        Assert.IsNotNull(result);
        Assert.AreNotEqual(blueprint.Content.Key, result.Key);

        List<Guid> newKeys = [];
        var newInvariantBlocklist = GetBlockValue("invariantBlocks");
        newKeys.AddRange(
            newInvariantBlocklist.Layout
            .SelectMany(x => x.Value)
            .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));

        if (variant)
        {
            foreach (var culture in result.AvailableCultures)
            {
                var newVariantBlocklist = GetBlockValue("blocks", culture);
                newKeys.AddRange(
                    newVariantBlocklist.Layout
                        .SelectMany(x => x.Value)
                        .SelectMany(v => new List<Guid> { v.ContentKey, v.SettingsKey!.Value }));
            }
        }

        foreach (var newKey in newKeys)
        {
            Assert.IsFalse(blueprint.BlockKeys.Contains(newKey), "The blocks in a content item generated from a template should have new keys.");
        }

        return;

        BlockValue GetBlockValue(string propertyAlias, string? culture = null)
        {
            return editorAlias switch
            {
                Constants.PropertyEditors.Aliases.BlockList => JsonSerializer.Deserialize<BlockListValue>(result.GetValue<string>(propertyAlias, culture)),
                Constants.PropertyEditors.Aliases.BlockGrid => JsonSerializer.Deserialize<BlockGridValue>(result.GetValue<string>(propertyAlias, culture)),
                Constants.PropertyEditors.Aliases.RichText => JsonSerializer.Deserialize<RichTextEditorValue>(result.GetValue<string>(propertyAlias, culture)).Blocks!,
                _ => throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported for block blueprints."),
            };
        }
    }

    public class ContentScaffoldedNotificationHandler : INotificationHandler<ContentScaffoldedNotification>
    {
        public static Action<ContentScaffoldedNotification>? ContentScaffolded { get; set; }

        public void Handle(ContentScaffoldedNotification notification) => ContentScaffolded?.Invoke(notification);
    }

    private async Task<(IContent Content, List<Guid> BlockKeys)> CreateBlueprintWithBlocksEditor(bool variant, string editorAlias)
    {
        var contentType = variant ? await CreateVariantContentType() : CreateInvariantContentType();

        // Create element type
        var elementContentType = new ContentTypeBuilder()
            .WithAlias("elementType")
            .WithName("Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementContentType, Constants.Security.SuperUserKey);

        // Create settings element type
        var settingsContentType = new ContentTypeBuilder()
            .WithAlias("settingsType")
            .WithName("Settings")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(settingsContentType, Constants.Security.SuperUserKey);

        // Create blocks datatype using the created elements
        var dataType = DataTypeBuilder.CreateSimpleElementDataType(IOHelper, editorAlias, elementContentType.Key, settingsContentType.Key);
        var dataTypeAttempt = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.True(dataTypeAttempt.Success, $"Failed to create data type: {dataTypeAttempt.Exception?.Message}");

        // Create new blocks property types
        var invariantPropertyType = new PropertyTypeBuilder<ContentTypeBuilder>(new ContentTypeBuilder())
            .WithPropertyEditorAlias(editorAlias)
            .WithValueStorageType(ValueStorageType.Ntext)
            .WithAlias("invariantBlocks")
            .WithName("Invariant Blocks")
            .WithDataTypeId(dataType.Id)
            .WithVariations(ContentVariation.Nothing)
            .Build();
        contentType.AddPropertyType(invariantPropertyType);

        if (contentType.VariesByCulture())
        {
            var propertyType = new PropertyTypeBuilder<ContentTypeBuilder>(new ContentTypeBuilder())
                .WithPropertyEditorAlias(editorAlias)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithAlias("blocks")
                .WithName("Blocks")
                .WithDataTypeId(dataType.Id)
                .WithVariations(contentType.Variations)
                .Build();
            contentType.AddPropertyType(propertyType);
        }

        // Update the content type with the new blocks property type
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        string?[] cultures = contentType.VariesByCulture()
            ? [null, "en-US", "da-DK"]
            : [null];

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = cultures.Where(c => variant ? c != null : c == null).Select(c => new VariantModel { Culture = c, Name = $"Initial Blueprint {c}" }),
        };

        List<Guid> allBlockKeys = [];
        foreach (var culture in cultures)
        {
            var (blockValue, blockKeys) = CreateBlockValue(editorAlias, elementContentType, settingsContentType);
            createModel.Properties = createModel.Properties.Append(
                new PropertyValueModel
                {
                    Alias = culture == null ? "invariantBlocks" : "blocks",
                    Value = JsonSerializer.Serialize(blockValue),
                    Culture = culture,
                });
            allBlockKeys.AddRange(blockKeys);
        }

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return (result.Result.Content, allBlockKeys);
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
                throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported for block blueprints.");
        }
    }

    private static (T BlockValue, IEnumerable<Guid> BlockKeys) CreateBlockValueOfType<T, TLayout>(
        string editorAlias,
        IContentType elementContentType,
        IContentType settingsContentType)
        where T : BlockValue, new()
        where TLayout : IBlockLayoutItem, new()
    {
        // Generate two pairs of Guids as a list of tuples
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
