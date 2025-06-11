using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests for the content blueprint editing service. Please notice that a lot of the functional tests are covered by the content
///     editing service tests, since these services share the same base implementation.
/// </summary>
public partial class ContentBlueprintEditingServiceTests : ContentEditingServiceTestsBase
{
    private IContentBlueprintContainerService ContentBlueprintContainerService => GetRequiredService<IContentBlueprintContainerService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private async Task<IContent> CreateInvariantContentBlueprint()
    {
        var contentType = CreateInvariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Initial Blueprint Name" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
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
            var (blockValue, blockKeys) = CreateBlockValue();
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

        (object BlockValue, IEnumerable<Guid> BlockKeys) CreateBlockValue()
        {
            switch (editorAlias)
            {
                case Constants.PropertyEditors.Aliases.BlockList:
                    return CreateBlockValueOfType<BlockListValue, BlockListLayoutItem>();
                case Constants.PropertyEditors.Aliases.BlockGrid:
                    return CreateBlockValueOfType<BlockGridValue, BlockGridLayoutItem>();
                case Constants.PropertyEditors.Aliases.RichText:
                    var res = CreateBlockValueOfType<RichTextBlockValue, RichTextBlockLayoutItem>();
                    return (new RichTextEditorValue { Markup = string.Empty, Blocks = res.BlockValue }, res.BlockKeys);
                default:
                    throw new NotSupportedException($"Editor alias '{editorAlias}' is not supported for block blueprints.");
            }
        }

        (T BlockValue, IEnumerable<Guid> BlockKeys) CreateBlockValueOfType<T, TLayout>()
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

    private async Task<IContent> CreateVariantContentBlueprint()
    {
        var contentType = await CreateVariantContentType();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Initial Blueprint English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Blueprint Danish Name" }
            ],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private ContentBlueprintCreateModel SimpleContentBlueprintCreateModel(Guid blueprintKey, Guid? containerKey)
    {
        var createModel = new ContentBlueprintCreateModel
        {
            Key = blueprintKey,
            ContentTypeKey = ContentType.Key,
            ParentKey = containerKey,
            Variants = [new VariantModel { Name = "Blueprint #1" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" },
            ],
        };
        return createModel;
    }

    private ContentBlueprintUpdateModel SimpleContentBlueprintUpdateModel()
    {
        var createModel = new ContentBlueprintUpdateModel
        {
            Variants = [new VariantModel { Name = "Blueprint #1 updated" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value updated" },
                new PropertyValueModel { Alias = "author", Value = "The author value updated" }
            ],
        };
        return createModel;
    }

    private IEntitySlim[] GetBlueprintChildren(Guid? containerKey)
        => EntityService.GetPagedChildren(containerKey, [UmbracoObjectTypes.DocumentBlueprintContainer], UmbracoObjectTypes.DocumentBlueprint, 0, 100, out _).ToArray();
}

