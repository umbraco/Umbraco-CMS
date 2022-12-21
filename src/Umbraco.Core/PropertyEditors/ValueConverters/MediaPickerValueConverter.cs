using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ContentApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The media picker property value converter.
/// </summary>
[DefaultPropertyValueConverter]
public class MediaPickerValueConverter : PropertyValueConverterBase, IContentApiPropertyValueConverter
{
    // hard-coding "image" here but that's how it works at UI level too
    private const string ImageTypeAlias = "image";

    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedValueFallback _publishedValueFallback;
    private readonly IContentNameProvider _contentNameProvider;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public MediaPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory)
        : this(
            publishedSnapshotAccessor,
            publishedModelFactory,
            StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IPublishedValueFallback>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentNameProvider>())
    {
    }

    public MediaPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        // annoyingly not even ActivatorUtilitiesConstructor can fix ambiguous constructor exceptions.
        // we need to keep this unused parameter, at least until the other constructor is removed
        IPublishedModelFactory publishedModelFactory,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IContentNameProvider contentNameProvider)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _publishedUrlProvider = publishedUrlProvider;
        _publishedValueFallback = publishedValueFallback;
        _contentNameProvider = contentNameProvider;
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

    public Type GetContentApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<ApiMedia>);

    public object? ConvertIntermediateToContentApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var isMultiple = IsMultipleDataType(propertyType.DataType);

        ApiMedia ToApiMedia(IPublishedContent media)
        {
            var customProperties = media
                .Properties
                .Where(p => p.Alias.StartsWith("umbraco") == false)
                .ToDictionary(p => p.Alias, p => p.GetContentApiValue());

            return new ApiMedia(media.Key,
                _contentNameProvider.GetName(media),
                media.ContentType.Alias, _publishedUrlProvider.GetMediaUrl(media, UrlMode.Relative), media.Value<string>(_publishedValueFallback, Constants.Conventions.Media.Extension), media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Width), media.Value<int?>(_publishedValueFallback, Constants.Conventions.Media.Height), customProperties);
        }

        // NOTE: eventually we might implement this explicitly instead of piggybacking on the default object conversion. however, this only happens once per cache rebuild,
        // and the performance gain from an explicit implementation is negligible, so... at least for the time being this will do just fine.
        var converted = ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);
        if (isMultiple && converted is IEnumerable<IPublishedContent> items)
        {
            return items.Select(ToApiMedia).ToArray();
        }

        if (isMultiple == false && converted is IPublishedContent item)
        {
            return new[] { ToApiMedia(item) };
        }

        return Array.Empty<ApiMedia>();
    }

    private object? FirstOrDefault(IList mediaItems) => mediaItems.Count == 0 ? null : mediaItems[0];
}
