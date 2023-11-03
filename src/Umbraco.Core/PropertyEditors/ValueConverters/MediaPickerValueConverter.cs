using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The media picker property value converter.
/// </summary>
[DefaultPropertyValueConverter]
[Obsolete("Please use the MediaPicker3 instead, will be removed in V13")]
public class MediaPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    // hard-coding "image" here but that's how it works at UI level too
    private const string ImageTypeAlias = "image";

    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V13")]
    public MediaPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory)
        : this(
            publishedSnapshotAccessor,
            publishedModelFactory,
            StaticServiceProvider.Instance.GetRequiredService<IApiMediaBuilder>())
    {
    }

    public MediaPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        // annoyingly not even ActivatorUtilitiesConstructor can fix ambiguous constructor exceptions.
        // we need to keep this unused parameter, at least until the other constructor is removed
        IPublishedModelFactory publishedModelFactory,
        IApiMediaBuilder apiMediaBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _apiMediaBuilder = apiMediaBuilder;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MediaPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);
        return isMultiple
            ? typeof(IEnumerable<IPublishedContent>)
            : typeof(IPublishedContent);
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    // the intermediate value of this editor is the picked IPublishedContent items (or item in single picker mode).
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        Udi[]? udis = ParseMediaUdis(source);
        return udis is null
            ? null
            : ConvertMediaUdisToIntermediateObject(udis, propertyType);
    }

    private bool IsMultipleDataType(PublishedDataType dataType)
    {
        MediaPickerConfiguration? config =
            ConfigurationEditor.ConfigurationAs<MediaPickerConfiguration>(dataType.Configuration);
        return config?.Multiple ?? false;
    }

    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel cacheLevel,
        object? inter,
        bool preview) => inter;

    // for backwards compat let's convert the intermediate value (either a list of content items or a single content item) to an array of media UDIs
    public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => inter is IEnumerable<IPublishedContent> medias
            ? medias.Select(m => new GuidUdi(Constants.UdiEntityType.Media, m.Key)).ToArray()
            : inter is IPublishedContent media
                ? new[] { new GuidUdi(Constants.UdiEntityType.Media, media.Key) }
                : null;

    // the API cache level must be Snapshot in order to facilitate nested field expansion and limiting
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiMedia>);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);

        if (isMultiple && inter is IEnumerable<IPublishedContent> items)
        {
            return items.Select(_apiMediaBuilder.Build).ToArray();
        }

        if (isMultiple == false && inter is IPublishedContent item)
        {
            return new[] { _apiMediaBuilder.Build(item) };
        }

        return Array.Empty<ApiMedia>();
    }

    private Udi[]? ParseMediaUdis(object? source)
        => source?.ToString()?
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(item
                => UdiParser.TryParse(item, out Udi? udi) && udi.EntityType is Constants.UdiEntityType.Media
                    ? udi
                    : null)
            .WhereNotNull()
            .ToArray();

    private object? ConvertMediaUdisToIntermediateObject(Udi[] udis, IPublishedPropertyType propertyType)
    {
        if (udis.Any() is false)
        {
            return null;
        }

        var isMultiple = IsMultipleDataType(propertyType.DataType);

        var mediaItems = new List<IPublishedContent>();

        IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        foreach (Udi udi in udis)
        {
            if (udi is not GuidUdi guidUdi)
            {
                continue;
            }

            IPublishedContent? item = publishedSnapshot?.Media?.GetById(guidUdi.Guid);
            if (item != null)
            {
                mediaItems.Add(item);
            }

            if (isMultiple is false && mediaItems.Any())
            {
                break;
            }
        }

        return isMultiple ? mediaItems : mediaItems.FirstOrDefault();
    }
}
