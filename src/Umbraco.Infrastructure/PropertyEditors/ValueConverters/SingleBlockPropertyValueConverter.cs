// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.ValueConverters;

/// <summary>
/// Converts the value stored by a single block property editor in Umbraco into a strongly-typed object
/// that can be used within the application. This value converter is typically used to deserialize
/// the JSON representation of a single block into a .NET object for further processing or display.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class SingleBlockPropertyValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IProfilingLogger _proflog;
    private readonly BlockEditorConverter _blockConverter;
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly BlockListPropertyValueConstructorCache _constructorCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly BlockEditorVarianceHandler _blockEditorVarianceHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockPropertyValueConverter"/> class.
    /// </summary>
    /// <param name="proflog">The logger used for profiling and diagnostics.</param>
    /// <param name="blockConverter">The service responsible for converting block editor values.</param>
    /// <param name="apiElementBuilder">Builds API elements for block properties.</param>
    /// <param name="jsonSerializer">Handles JSON serialization and deserialization.</param>
    /// <param name="constructorCache">Caches constructors for block list property values.</param>
    /// <param name="variationContextAccessor">Provides access to the current variation context.</param>
    /// <param name="blockEditorVarianceHandler">Handles variance logic for block editors.</param>
    public SingleBlockPropertyValueConverter(
        IProfilingLogger proflog,
        BlockEditorConverter blockConverter,
        IApiElementBuilder apiElementBuilder,
        IJsonSerializer jsonSerializer,
        BlockListPropertyValueConstructorCache constructorCache,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler)
    {
        _proflog = proflog;
        _blockConverter = blockConverter;
        _apiElementBuilder = apiElementBuilder;
        _jsonSerializer = jsonSerializer;
        _constructorCache = constructorCache;
        _variationContextAccessor = variationContextAccessor;
        _blockEditorVarianceHandler = blockEditorVarianceHandler;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.SingleBlock);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof( BlockListItem);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString();

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
        using (!_proflog.IsEnabled(Core.Logging.LogLevel.Debug) ? null : _proflog.DebugDuration<BlockListPropertyValueConverter>(
                   $"ConvertPropertyToBlockList ({propertyType.DataType.Id})"))
        {
            return ConvertIntermediateToBlockListItem(owner, propertyType, referenceCacheLevel, inter, preview);
        }
    }

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(ApiBlockItem);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        BlockListItem? model = ConvertIntermediateToBlockListItem(owner, propertyType, referenceCacheLevel, inter, preview);

        return
            model?.CreateApiBlockItem(_apiElementBuilder);
    }

    private BlockListItem? ConvertIntermediateToBlockListItem(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        using (!_proflog.IsEnabled(LogLevel.Debug) ? null : _proflog.DebugDuration<SingleBlockPropertyValueConverter>(
                   $"ConvertPropertyToSingleBlock ({propertyType.DataType.Id})"))
        {
            // NOTE: The intermediate object is just a JSON string, we don't actually convert from source -> intermediate since source is always just a JSON string
            if (inter is not string intermediateBlockModelValue)
            {
                return null;
            }

            // Get configuration
            SingleBlockConfiguration? configuration = propertyType.DataType.ConfigurationAs<SingleBlockConfiguration>();
            if (configuration is null)
            {
                return null;
            }


            var creator = new SingleBlockPropertyValueCreator(_blockConverter, _variationContextAccessor, _blockEditorVarianceHandler, _jsonSerializer, _constructorCache);
            return creator.CreateBlockModel(owner, referenceCacheLevel, intermediateBlockModelValue, preview, configuration.Blocks);
        }
    }
}
