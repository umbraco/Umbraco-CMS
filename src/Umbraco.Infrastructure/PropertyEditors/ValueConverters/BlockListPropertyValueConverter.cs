// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.BlockListConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class BlockListPropertyValueConverter : BlockPropertyValueConverterBase<BlockListModel, BlockListItem, BlockListLayoutItem, BlockConfiguration>, IHeadlessPropertyValueConverter
{
    private readonly IProfilingLogger _proflog;
    private readonly IHeadlessElementBuilder _headlessElementBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter)
        : this(proflog, blockConverter, StaticServiceProvider.Instance.GetRequiredService<IHeadlessElementBuilder>())
    {
    }

    public BlockListPropertyValueConverter(IProfilingLogger proflog, BlockEditorConverter blockConverter, IHeadlessElementBuilder headlessElementBuilder)
        : base(blockConverter)
    {
        _proflog = proflog;
        _headlessElementBuilder = headlessElementBuilder;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockList);

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => ConvertIntermediateToBlockListModel(owner, propertyType, referenceCacheLevel, inter, preview);

    /// <inheritdoc />
    public Type GetHeadlessPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<HeadlessBlockListModel>);

    /// <inheritdoc />
    public object? ConvertIntermediateToHeadlessObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        BlockListModel? model = ConvertIntermediateToBlockListModel(owner, propertyType, referenceCacheLevel, inter, preview);

        return new HeadlessBlockListModel(
            model != null
                ? model.Select(item => new HeadlessBlockItem(
                    _headlessElementBuilder.Build(item.Content),
                    item.Settings != null ? _headlessElementBuilder.Build(item.Settings) : null))
                : Array.Empty<HeadlessBlockItem>());
    }

    private BlockListModel? ConvertIntermediateToBlockListModel(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
        using (_proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            // Get configuration
            BlockListConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockListConfiguration>();
            if (configuration is null)
            {
                return null;
            }

            BlockListModel CreateEmptyModel() => BlockListModel.Empty;

            BlockListModel CreateModel(IList<BlockListItem> items) => new BlockListModel(items);

            BlockListModel blockModel = UnwrapBlockModel(referenceCacheLevel, inter, preview, configuration.Blocks, CreateEmptyModel, CreateModel);

            return blockModel;
        }
    }

    protected override BlockEditorDataConverter CreateBlockEditorDataConverter() => new BlockListEditorDataConverter();

    protected override BlockItemActivator<BlockListItem> CreateBlockItemActivator() => new BlockListItemActivator(BlockEditorConverter);

    private class BlockListItemActivator : BlockItemActivator<BlockListItem>
    {
        public BlockListItemActivator(BlockEditorConverter blockConverter) : base(blockConverter)
        {
        }

        protected override Type GenericItemType => typeof(BlockListItem<,>);
    }
}
