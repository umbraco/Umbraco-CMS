using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal class ContentPickerValueConverter : PropertyValueConverterBase
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    public ContentPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor) =>
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.ContentPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IPublishedContent);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Elements;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        if (source is not string)
        {
            Attempt<int> attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
            {
                return attemptConvertInt.Result;
            }
        }

        // Don't attempt to convert to int for UDI
        if (source is string strSource
            && !string.IsNullOrWhiteSpace(strSource)
            && !strSource.StartsWith("umb")
            && int.TryParse(strSource, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        Attempt<Udi> attemptConvertUdi = source.TryConvertTo<Udi>();
        if (attemptConvertUdi.Success)
        {
            return attemptConvertUdi.Result;
        }

        return null;
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter == null)
        {
            return null;
        }

        if ((propertyType.Alias != null &&
             PropertiesToExclude.Contains(propertyType.Alias.ToLower(CultureInfo.InvariantCulture))) == false)
        {
            IPublishedContent? content;
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            if (inter is int id)
            {
                content = publishedSnapshot.Content?.GetById(id);
                if (content != null)
                {
                    return content;
                }
            }
            else
            {
                if (inter is not GuidUdi udi)
                {
                    return null;
                }

                content = publishedSnapshot.Content?.GetById(udi.Guid);
                if (content != null && content.ContentType.ItemType == PublishedItemType.Content)
                {
                    return content;
                }
            }
        }

        return inter;
    }

    public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter == null)
        {
            return null;
        }

        return inter.ToString();
    }
}
