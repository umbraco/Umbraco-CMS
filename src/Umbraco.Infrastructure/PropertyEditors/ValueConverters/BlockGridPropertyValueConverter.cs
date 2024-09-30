// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class BlockGridPropertyValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
    {
        private readonly IProfilingLogger _proflog;
        private readonly BlockEditorConverter _blockConverter;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IApiElementBuilder _apiElementBuilder;
        private readonly BlockGridPropertyValueConstructorCache _constructorCache;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

        [Obsolete("Use the constructor that takes all parameters, scheduled for removal in V16")]
        public BlockGridPropertyValueConverter(
            IProfilingLogger proflog,
            BlockEditorConverter blockConverter,
            IJsonSerializer jsonSerializer,
            IApiElementBuilder apiElementBuilder,
            BlockGridPropertyValueConstructorCache constructorCache)
            : this(proflog, blockConverter, jsonSerializer, apiElementBuilder, constructorCache,
                StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>(),
                StaticServiceProvider.Instance.GetRequiredService<BlockEditorVarianceHandler>())
        {
        }

        public BlockGridPropertyValueConverter(
            IProfilingLogger proflog,
            BlockEditorConverter blockConverter,
            IJsonSerializer jsonSerializer,
            IApiElementBuilder apiElementBuilder,
            BlockGridPropertyValueConstructorCache constructorCache,
            IVariationContextAccessor variationContextAccessor,
            BlockEditorVarianceHandler blockEditorVarianceHandler)
        {
            _proflog = proflog;
            _blockConverter = blockConverter;
            _jsonSerializer = jsonSerializer;
            _apiElementBuilder = apiElementBuilder;
            _constructorCache = constructorCache;
            _variationContextAccessor = variationContextAccessor;
            _blockEditorVarianceHandler = blockEditorVarianceHandler;
        }

        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.BlockGrid);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(BlockGridModel);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        /// <inheritdoc />
        public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
            => ConvertIntermediateToBlockGridModel(owner, propertyType, referenceCacheLevel, inter, preview);

        /// <inheritdoc />
        public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

        /// <inheritdoc />
        public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        /// <inheritdoc />
        public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(ApiBlockGridModel);

        /// <inheritdoc />
        public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        {
            const int defaultColumns = 12;

            BlockGridModel? blockGridModel = ConvertIntermediateToBlockGridModel(owner, propertyType, referenceCacheLevel, inter, preview);
            if (blockGridModel == null)
            {
                return new ApiBlockGridModel(defaultColumns, Array.Empty<ApiBlockGridItem>());
            }

            ApiBlockGridItem CreateApiBlockGridItem(BlockGridItem item)
                => new ApiBlockGridItem(
                    _apiElementBuilder.Build(item.Content),
                    item.Settings != null
                        ? _apiElementBuilder.Build(item.Settings)
                        : null,
                    item.RowSpan,
                    item.ColumnSpan,
                    item.AreaGridColumns ?? blockGridModel.GridColumns ?? defaultColumns,
                    item.Areas.Select(CreateApiBlockGridArea).ToArray());

            ApiBlockGridArea CreateApiBlockGridArea(BlockGridArea area)
                => new ApiBlockGridArea(
                    area.Alias,
                    area.RowSpan,
                    area.ColumnSpan,
                    area.Select(CreateApiBlockGridItem).ToArray());

            var model = new ApiBlockGridModel(
                blockGridModel.GridColumns ?? defaultColumns,
                blockGridModel.Select(CreateApiBlockGridItem).ToArray());

            return model;
        }

        private BlockGridModel? ConvertIntermediateToBlockGridModel(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        {
            using (!_proflog.IsEnabled(LogLevel.Debug) ? null : _proflog.DebugDuration<BlockGridPropertyValueConverter>($"ConvertPropertyToBlockGrid ({propertyType.DataType.Id})"))
            {
                // NOTE: this is to retain backwards compatability
                if (inter is null)
                {
                    return BlockGridModel.Empty;
                }

                // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
                if (inter is not string intermediateBlockModelValue)
                {
                    return null;
                }

                // Get configuration
                BlockGridConfiguration? configuration = propertyType.DataType.ConfigurationAs<BlockGridConfiguration>();
                if (configuration is null)
                {
                    return null;
                }

                var creator = new BlockGridPropertyValueCreator(_blockConverter, _variationContextAccessor, _blockEditorVarianceHandler, _jsonSerializer, _constructorCache);
                return creator.CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, configuration.Blocks, configuration.GridColumns);
            }
        }
    }
}
