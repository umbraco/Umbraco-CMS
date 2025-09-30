using System.Globalization;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public class ContentPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    public ContentPickerValueConverter(
        IPublishedContentCache publishedContentCache,
        IApiContentBuilder apiContentBuilder)
    {
        _publishedContentCache = publishedContentCache;
        _apiContentBuilder = apiContentBuilder;
    }

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
        IPublishedContent? content = GetContent(propertyType, inter);
        return content ?? inter;
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IApiContent);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IPublishedContent? content = GetContent(propertyType, inter);
        if (content == null)
        {
            return null;
        }

        return _apiContentBuilder.Build(content);
    }

    private IPublishedContent? GetContent(IPublishedPropertyType propertyType, object? inter)
    {
        if (inter == null)
        {
            return null;
        }

        if ((propertyType.Alias != null &&
             PropertiesToExclude.Contains(propertyType.Alias.ToLower(CultureInfo.InvariantCulture))) == false)
        {
            IPublishedContent? content;
            if (inter is int id)
            {
                content = _publishedContentCache.GetById(id);
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

                content = _publishedContentCache.GetById(udi.Guid);
                if (content != null && content.ContentType.ItemType == PublishedItemType.Content)
                {
                    return content;
                }
            }
        }

        return null;
    }
}
