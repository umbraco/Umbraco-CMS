using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
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

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        Udi[]? nodeIds = source.ToString()?
            .Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(UdiParser.Parse)
            .ToArray();
        return nodeIds;
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
        object? source,
        bool preview)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);

        var udis = (Udi[]?)source;
        var mediaItems = new List<IPublishedContent>();

        if (source == null)
        {
            return isMultiple ? mediaItems : null;
        }

        if (udis?.Any() ?? false)
        {
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
            }

            return isMultiple ? mediaItems : FirstOrDefault(mediaItems);
        }

        return source;
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Elements;

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiMedia>);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);

        // NOTE: eventually we might implement this explicitly instead of piggybacking on the default object conversion. however, this only happens once per cache rebuild,
        // and the performance gain from an explicit implementation is negligible, so... at least for the time being this will do just fine.
        var converted = ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        if (isMultiple && converted is IEnumerable<IPublishedContent> items)
        {
            return items.Select(_apiMediaBuilder.Build).ToArray();
        }

        if (isMultiple == false && converted is IPublishedContent item)
        {
            return new[] { _apiMediaBuilder.Build(item) };
        }

        return Array.Empty<ApiMedia>();
    }

    private object? FirstOrDefault(IList mediaItems) => mediaItems.Count == 0 ? null : mediaItems[0];
}
