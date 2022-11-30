using System.Collections;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     The media picker property value converter.
/// </summary>
[DefaultPropertyValueConverter]
public class MediaPickerValueConverter : PropertyValueConverterBase
{
    // hard-coding "image" here but that's how it works at UI level too
    private const string ImageTypeAlias = "image";

    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public MediaPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IPublishedModelFactory publishedModelFactory)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _publishedModelFactory = publishedModelFactory;
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

    private object? FirstOrDefault(IList mediaItems) => mediaItems.Count == 0 ? null : mediaItems[0];
}
