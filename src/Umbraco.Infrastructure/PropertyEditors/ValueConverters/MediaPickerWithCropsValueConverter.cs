using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class MediaPickerWithCropsValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IApiMediaWithCropsBuilder _apiMediaWithCropsBuilder;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    // TODO KJA: constructor breakage
    public MediaPickerWithCropsValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IJsonSerializer jsonSerializer,
        IApiMediaWithCropsBuilder apiMediaWithCropsBuilder,
        IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _publishedUrlProvider = publishedUrlProvider;
        _publishedValueFallback = publishedValueFallback;
        _jsonSerializer = jsonSerializer;
        _apiMediaWithCropsBuilder = apiMediaWithCropsBuilder;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MediaPicker3);

    public override bool? IsValue(object? value, PropertyValueLevel level)
    {
        var isValue = base.IsValue(value, level);
        if (isValue != false && level == PropertyValueLevel.Source)
        {
            // Empty JSON array is not a value
            isValue = value?.ToString() != "[]";
        }

        return isValue;
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsMultipleDataType(propertyType.DataType)
            ? typeof(IEnumerable<MediaWithCrops>)
            : typeof(MediaWithCrops);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>
        PropertyCacheLevel.Snapshot;

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);
        if (string.IsNullOrEmpty(inter?.ToString()))
        {
            // Short-circuit on empty value
            return isMultiple ? Enumerable.Empty<MediaWithCrops>() : null;
        }

        var mediaItems = new List<MediaWithCrops>();
        IEnumerable<MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto> dtos =
            MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.Deserialize(_jsonSerializer, inter);
        MediaPicker3Configuration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<MediaPicker3Configuration>(propertyType.DataType.Key);
        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        foreach (MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto dto in dtos)
        {
            IPublishedContent? mediaItem = publishedSnapshot.Media?.GetById(preview, dto.MediaKey);
            if (mediaItem != null)
            {
                var localCrops = new ImageCropperValue
                {
                    Crops = dto.Crops,
                    FocalPoint = dto.FocalPoint,
                    Src = mediaItem.Url(_publishedUrlProvider),
                };

                localCrops.ApplyConfiguration(configuration);

                // TODO: This should be optimized/cached, as calling Activator.CreateInstance is slow
                Type mediaWithCropsType = typeof(MediaWithCrops<>).MakeGenericType(mediaItem.GetType());
                var mediaWithCrops = (MediaWithCrops)Activator.CreateInstance(mediaWithCropsType, mediaItem, _publishedValueFallback, localCrops)!;

                mediaItems.Add(mediaWithCrops);

                if (!isMultiple)
                {
                    // Short-circuit on single item
                    break;
                }
            }
        }

        return isMultiple ? mediaItems : mediaItems.FirstOrDefault();
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiMediaWithCrops>);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);

        IApiMediaWithCrops ToApiMedia(MediaWithCrops media) => _apiMediaWithCropsBuilder.Build(media);

        // NOTE: eventually we might implement this explicitly instead of piggybacking on the default object conversion. however, this only happens once per cache rebuild,
        // and the performance gain from an explicit implementation is negligible, so... at least for the time being this will do just fine.
        var converted = ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        if (isMultiple && converted is IEnumerable<MediaWithCrops> mediasWithCrops)
        {
            return mediasWithCrops.Select(ToApiMedia).ToArray();
        }
        if (isMultiple == false && converted is MediaWithCrops mediaWithCrops)
        {
            return new [] { ToApiMedia(mediaWithCrops) };
        }

        return Array.Empty<IApiMediaWithCrops>();
    }

    private bool IsMultipleDataType(PublishedDataType dataType) =>
        _dataTypeConfigurationCache.GetConfigurationAs<MediaPicker3Configuration>(dataType.Key)?.Multiple ?? false;
}
