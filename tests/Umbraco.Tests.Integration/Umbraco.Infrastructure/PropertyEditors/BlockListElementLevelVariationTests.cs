using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal partial class BlockListElementLevelVariationTests : BlockEditorElementVariationTestBase
{
    public static new void ConfigureAllowEditInvariantFromNonDefaultTrue(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(config =>
            config.AllowEditInvariantFromNonDefault = true);
    }

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockList,
            new BlockListConfiguration.BlockConfiguration[]
            {
                new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
            });

    private IContent CreateContent(IContentType contentType, IContentType elementType, IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues, bool publishContent)
        => CreateContent(
            contentType,
            elementType,
            new[] { new BlockProperty(blockContentValues, blockSettingsValues, null, null) },
            publishContent);

    private IContent CreateContent(IContentType contentType, IContentType elementType, IEnumerable<BlockProperty> blocksProperties, bool publishContent)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType);
        contentBuilder = contentType.VariesByCulture()
            ? contentBuilder
                .WithCultureName("en-US", "Home (en)")
                .WithCultureName("da-DK", "Home (da)")
            : contentBuilder.WithName("Home");

        var content = contentBuilder.Build();

        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();
        foreach (var blocksProperty in blocksProperties)
        {
            var blockListValue = BlockListPropertyValue(elementType, contentElementKey, settingsElementKey, blocksProperty);
            var propertyValue = JsonSerializer.Serialize(blockListValue);
            content.Properties["blocks"]!.SetValue(propertyValue, blocksProperty.Culture, blocksProperty.Segment);
        }

        ContentService.Save(content);

        if (publishContent)
        {
            PublishContent(content, contentType);
        }

        return content;
    }

    private BlockListValue BlockListPropertyValue(IContentType elementType, Guid contentElementKey, Guid settingsElementKey, BlockProperty blocksProperty)
        => BlockListPropertyValue(elementType, [(contentElementKey, settingsElementKey, blocksProperty)]);

    private BlockListValue BlockListPropertyValue(IContentType elementType, List<(Guid contentElementKey, Guid settingsElementKey, BlockProperty BlocksProperty)> blocks)
    {
        var expose = new List<BlockItemVariation>();
        foreach (var block in blocks)
        {
            var cultures = elementType.VariesByCulture()
                ? new[] { block.BlocksProperty.Culture }
                    .Union(block.BlocksProperty.BlockContentValues.Select(value => value.Culture))
                    .WhereNotNull()
                    .Distinct()
                    .ToArray()
                : [null];
            if (cultures.Any() is false)
            {
                cultures = [null];
            }

            var segments = elementType.VariesBySegment()
                ? new[] { block.BlocksProperty.Segment }
                    .Union(block.BlocksProperty.BlockContentValues.Select(value => value.Segment))
                    .Distinct()
                    .ToArray()
                : [null];

            expose.AddRange(cultures.SelectMany(culture => segments.Select(segment =>
                new BlockItemVariation(block.contentElementKey, culture, segment))));
        }

        return new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                {
                    Constants.PropertyEditors.Aliases.BlockList,
                    blocks.Select(block => new BlockListLayoutItem
                    {
                        ContentKey = block.contentElementKey, SettingsKey = block.settingsElementKey
                    }).ToArray()
                }
            },
            ContentData =
                blocks.Select(block => new BlockItemData
                {
                    Key = block.contentElementKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values = block.BlocksProperty.BlockContentValues
                }).ToList(),
            SettingsData = blocks.Select(block => new BlockItemData
            {
                Key = block.settingsElementKey,
                ContentTypeAlias = elementType.Alias,
                ContentTypeKey = elementType.Key,
                Values = block.BlocksProperty.BlockSettingsValues
            }).ToList(),
            Expose = expose
        };
    }

    private void PublishContent(IContent content, IContentType contentType, string[]? culturesToPublish = null)
    {
        culturesToPublish ??= contentType.VariesByCulture()
            ? ["en-US", "da-DK"]
            : ["*"];
        PublishContent(content, culturesToPublish);
    }

    private async Task<IPublishedContent> CreatePublishedContent(ContentVariation variation, IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues)
    {
        var elementType = CreateElementType(variation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(variation, blockListDataType);

        var content = CreateContent(contentType, elementType, blockContentValues, blockSettingsValues, true);
        return GetPublishedContent(content.Key);
    }

    private IContentType CreateElementTypeWithValidation(ContentVariation contentVariation = ContentVariation.Culture)
    {
        var elementType = CreateElementType(contentVariation);
        foreach (var propertyType in elementType.PropertyTypes)
        {
            propertyType.Mandatory = true;
            propertyType.ValidationRegExp = "^Valid.*$";
        }

        ContentTypeService.Save(elementType);
        return elementType;
    }

    private async Task<(IContentType RootElementType, IContentType NestedElementType)> CreateElementTypeWithValidationAndNestedBlocksAsync()
    {
        var nestedElementType = CreateElementTypeWithValidation();
        var nestedBlockListDataType = await CreateBlockListDataType(nestedElementType);

        var rootElementType = new ContentTypeBuilder()
            .WithAlias("myRootElementType")
            .WithName("My Root Element Type")
            .WithIsElement(true)
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyType()
            .WithAlias("invariantText")
            .WithName("Invariant text")
            .WithMandatory(true)
            .WithValidationRegExp("^Valid.*$")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithValueStorageType(ValueStorageType.Nvarchar)
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantText")
            .WithName("Variant text")
            .WithMandatory(true)
            .WithValidationRegExp("^Valid.*$")
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

        return (rootElementType, nestedElementType);
    }

    private class BlockProperty
    {
        public BlockProperty(IList<BlockPropertyValue> blockContentValues, IList<BlockPropertyValue> blockSettingsValues, string? culture, string? segment)
        {
            BlockContentValues = blockContentValues;
            BlockSettingsValues = blockSettingsValues;
            Culture = culture;
            Segment = segment;
        }

        public IList<BlockPropertyValue> BlockContentValues { get; }

        public IList<BlockPropertyValue> BlockSettingsValues { get; }

        public string? Culture { get; }

        public string? Segment { get; }
    }
}
