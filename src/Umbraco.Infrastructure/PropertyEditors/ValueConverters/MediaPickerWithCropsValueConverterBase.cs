using System.Collections.Concurrent;
using System.Reflection;
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
/// Converts the value stored by a media picker property editor into a strongly-typed object representing the
/// selected media item(s) and their associated crop data, making it accessible for use in code.
/// </summary>
/// <remarks>
/// There is one media picker editor per number of items a picker holds, so that the type a media picker property
/// yields follows from the editor rather than from the configuration of the data type it is used through. Both store
/// the same value - a JSON array - which is what this base reads.
/// </remarks>
public abstract class MediaPickerWithCropsValueConverterBase : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly ConcurrentDictionary<Type, ConstructorInvoker> _mediaWithCropsFactories = new();

    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedMediaCache _publishedMediaCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IApiMediaWithCropsBuilder _apiMediaWithCropsBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPickerWithCropsValueConverterBase"/> class.
    /// </summary>
    /// <param name="publishedMediaCache">Provides access to the cache of published media items.</param>
    /// <param name="publishedUrlProvider">Resolves URLs for published content and media.</param>
    /// <param name="publishedValueFallback">Handles fallback logic for published property values.</param>
    /// <param name="jsonSerializer">Serializes and deserializes JSON data for property values.</param>
    /// <param name="apiMediaWithCropsBuilder">Builds API representations of media items with crop data.</param>
    protected MediaPickerWithCropsValueConverterBase(
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
    /// Gets a value indicating whether the editor this converter serves holds more than one media item.
    /// </summary>
    protected abstract bool HoldsMultipleItems { get; }

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
    /// Gets the cache level that should be used for the specified media picker property type.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>
    /// Always returns <see cref="PropertyCacheLevel.Snapshot"/>, indicating the value is cached at the snapshot level.
    /// </returns>
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) =>
        PropertyCacheLevel.Snapshot;

    /// <summary>
    /// Converts the intermediate value produced by a media picker property editor into its final strongly-typed object representation.
    /// </summary>
    /// <param name="owner">The published element that contains the property being converted.</param>
    /// <param name="propertyType">The type information for the property being converted.</param>
    /// <param name="referenceCacheLevel">The cache level to use for resolving referenced entities.</param>
    /// <param name="inter">The intermediate value to convert, typically a JSON string or deserialized object.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <returns>
    /// An <see cref="IEnumerable{MediaWithCrops}" /> containing the selected media items with crop data, or - for an
    /// editor holding a single item - one <see cref="MediaWithCrops" /> instance, or <c>null</c> if no value is present.
    /// </returns>
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (string.IsNullOrEmpty(inter?.ToString()))
        {
            // Short-circuit on empty value
            return HoldsMultipleItems ? Enumerable.Empty<MediaWithCrops>() : null;
        }

        var mediaItems = new List<MediaWithCrops>();
        IEnumerable<MediaPickerPropertyEditorBase.MediaPickerPropertyValueEditor.MediaWithCropsDto> dtos =
            MediaPickerPropertyEditorBase.MediaPickerPropertyValueEditor.Deserialize(_jsonSerializer, inter);
        MediaPickerConfigurationBase? configuration = propertyType.DataType.ConfigurationAs<MediaPickerConfigurationBase>();
        foreach (MediaPickerPropertyEditorBase.MediaPickerPropertyValueEditor.MediaWithCropsDto dto in dtos)
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

                MediaWithCrops mediaWithCrops = CreateMediaWithCrops(mediaItem, _publishedValueFallback, localCrops);

                mediaItems.Add(mediaWithCrops);

                if (HoldsMultipleItems is false)
                {
                    // Short-circuit on single item
                    break;
                }
            }
        }

        return HoldsMultipleItems ? mediaItems : mediaItems.FirstOrDefault();
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
    /// <remarks>
    /// Every media picker yields a collection to the Delivery API, including one holding a single item, so that the
    /// shape of a headless response does not depend on which media picker a property is edited through.
    /// </remarks>
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
        IApiMediaWithCrops ToApiMedia(MediaWithCrops media) => _apiMediaWithCropsBuilder.Build(media);

        // NOTE: eventually we might implement this explicitly instead of piggybacking on the default object conversion. however, this only happens once per cache rebuild,
        // and the performance gain from an explicit implementation is negligible, so... at least for the time being this will do just fine.
        var converted = ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        if (HoldsMultipleItems && converted is IEnumerable<MediaWithCrops> mediasWithCrops)
        {
            return mediasWithCrops.Select(ToApiMedia).ToArray();
        }

        if (HoldsMultipleItems is false && converted is MediaWithCrops mediaWithCrops)
        {
            return new[] { ToApiMedia(mediaWithCrops) };
        }

        return Array.Empty<IApiMediaWithCrops>();
    }

    private static MediaWithCrops CreateMediaWithCrops(
        IPublishedContent mediaItem,
        IPublishedValueFallback publishedValueFallback,
        ImageCropperValue localCrops)
    {
        ConstructorInvoker factory =
            _mediaWithCropsFactories.GetOrAdd(mediaItem.GetType(), static mediaType =>
            {
                Type closedType = typeof(MediaWithCrops<>).MakeGenericType(mediaType);
                ConstructorInfo ctor = closedType.GetConstructor(
                    [mediaType, typeof(IPublishedValueFallback), typeof(ImageCropperValue)])!;
                return ConstructorInvoker.Create(ctor);
            });

        return (MediaWithCrops)factory.Invoke(mediaItem, publishedValueFallback, localCrops);
    }
}
