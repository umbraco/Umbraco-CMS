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

/// <summary>
/// Converts the value stored by the Media Picker with Crops property editor into a strongly-typed object
/// representing the selected media item(s) and their associated crop data, making it accessible for use in code.
/// </summary>
[DefaultPropertyValueConverter]
public class MediaPickerWithCropsValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IApiMediaWithCropsBuilder _apiMediaWithCropsBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPickerWithCropsValueConverter"/> class.
    /// </summary>
    /// <param name="publishedMediaCache">Provides access to the cache of published media items.</param>
    /// <param name="publishedUrlProvider">Resolves URLs for published content and media.</param>
    /// <param name="publishedValueFallback">Handles fallback logic for published property values.</param>
    /// <param name="jsonSerializer">Serializes and deserializes JSON data for property values.</param>
    /// <param name="apiMediaWithCropsBuilder">Builds API representations of media items with crop data.</param>
    public MediaPickerWithCropsValueConverter(
        IPublishedMediaCache publishedMediaCache,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IJsonSerializer jsonSerializer,
        IApiMediaWithCropsBuilder apiMediaWithCropsBuilder)
    {
        _publishedMediaCache = publishedMediaCache;
        _publishedUrlProvider = publishedUrlProvider;
        _publishedValueFallback = publishedValueFallback;
        _jsonSerializer = jsonSerializer;
        _apiMediaWithCropsBuilder = apiMediaWithCropsBuilder;
    }

    /// <summary>
    /// Determines whether this converter applies to the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter is applicable; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MediaPicker3);

    /// <summary>
    /// Determines whether the specified value should be considered a valid property value at the given <paramref name="level"/>.
    /// For the <see cref="PropertyValueLevel.Source"/> level, this method also checks that the value is not an empty JSON array ("[]").
    /// </summary>
    /// <param name="value">The value to evaluate for validity.</param>
    /// <param name="level">The property value level at which to evaluate the value.</param>
    /// <returns>
    /// <c>true</c> if the value is valid; <c>false</c> if it is not valid; or <c>null</c> if the validity cannot be determined.
    /// </returns>
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

    /// <summary>
    /// Determines the CLR type returned for the property value, based on whether the property type allows multiple values.
    /// </summary>
    /// <param name="propertyType">The published property type to inspect.</param>
    /// <returns>
    /// <see cref="IEnumerable{MediaWithCrops}"/> if the property type allows multiple values; otherwise, <see cref="MediaWithCrops"/>.
    /// </returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsMultipleDataType(propertyType.DataType)
            ? typeof(IEnumerable<MediaWithCrops>)
            : typeof(MediaWithCrops);

    /// <summary>
    /// Gets the cache level that should be used for the specified media picker property type.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>
    /// Always returns <see cref="PropertyCacheLevel.Snapshot"/>, indicating the value is cached at the snapshot level.
    /// </returns>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>
        PropertyCacheLevel.Snapshot;

    /// <summary>
    /// Converts the intermediate value produced by the media picker with crops property editor into its final strongly-typed object representation.
    /// </summary>
    /// <param name="owner">The published element that contains the property being converted.</param>
    /// <param name="propertyType">The type information for the property being converted.</param>
    /// <param name="referenceCacheLevel">The cache level to use for resolving referenced entities.</param>
    /// <param name="inter">The intermediate value to convert, typically a JSON string or deserialized object.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <returns>
    /// If the property is configured for multiple selection, returns an <see cref="IEnumerable{MediaWithCrops}"/> containing the selected media items with crop data; if single selection, returns a single <see cref="MediaWithCrops"/> instance, or <c>null</c> if no value is present.
    /// </returns>
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
        MediaPicker3Configuration? configuration = propertyType.DataType.ConfigurationAs<MediaPicker3Configuration>();
        foreach (MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto dto in dtos)
        {
            IPublishedContent? mediaItem = _publishedMediaCache.GetById(preview, dto.MediaKey);
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

    /// <summary>
    /// Determines the cache level to use for the delivery API when accessing a media picker property with crops.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>The <see cref="PropertyCacheLevel"/> to be used for the delivery API.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    /// <summary>
    /// Determines the <see cref="PropertyCacheLevel"/> to use when expanding a media picker property for the Delivery API.
    /// </summary>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> representing the property being expanded.</param>
    /// <returns>The appropriate <see cref="PropertyCacheLevel"/> for Delivery API expansion.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    /// <summary>
    /// Gets the type used for delivery API property values for the specified published property type.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <returns>The <see cref="IEnumerable{IApiMediaWithCrops}"/> type used for delivery API property values.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiMediaWithCrops>);

    /// <summary>
    /// Converts the intermediate value representing media with crops to an object suitable for the Delivery API.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">Metadata describing the property type.</param>
    /// <param name="referenceCacheLevel">The cache level for property references.</param>
    /// <param name="inter">The intermediate value to convert, typically a <see cref="MediaWithCrops"/> instance or a collection thereof.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <param name="expanding">Indicates whether nested objects should be expanded during conversion.</param>
    /// <returns>
    /// An array of <see cref="IApiMediaWithCrops"/> representing the media items with crops for the Delivery API.
    /// Returns an empty array if no media items are present.
    /// For single media items, the result is a single-element array.
    /// </returns>
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
        dataType.ConfigurationAs<MediaPicker3Configuration>()?.Multiple ?? false;
}
