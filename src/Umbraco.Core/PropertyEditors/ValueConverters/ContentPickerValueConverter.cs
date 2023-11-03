using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public class ContentPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private static readonly List<string> PropertiesToExclude = new()
    {
        Constants.Conventions.Content.InternalRedirectId.ToLower(CultureInfo.InvariantCulture),
        Constants.Conventions.Content.Redirect.ToLower(CultureInfo.InvariantCulture),
    };

    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiContentBuilder _apiContentBuilder;

    [Obsolete("Use constructor that takes all parameters, scheduled for removal in V14")]
    public ContentPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        : this(
            publishedSnapshotAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IApiContentBuilder>())
    {
    }

    public ContentPickerValueConverter(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiContentBuilder apiContentBuilder)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _apiContentBuilder = apiContentBuilder;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.ContentPicker);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IPublishedContent);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Elements;

    // the intermediate value of this editor is the picked IPublishedContent item.
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
                return GetContent(propertyType, attemptConvertInt.Result);
            }
        }

        // Don't attempt to convert to int for UDI
        if (source is string strSource
            && !string.IsNullOrWhiteSpace(strSource)
            && !strSource.StartsWith("umb")
            && int.TryParse(strSource, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return GetContent(propertyType, intValue);
        }

        Attempt<Udi> attemptConvertUdi = source.TryConvertTo<Udi>();
        if (attemptConvertUdi.Success)
        {
            return GetContent(propertyType, attemptConvertUdi.Result);
        }

        return null;
    }

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => inter;

    [Obsolete("The current implementation of XPath is suboptimal and will be removed entirely in a future version. Scheduled for removal in v14")]
    public override object? ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => inter is IPublishedContent content
            ? Udi.Create(Constants.UdiEntityType.Document, content.Key)
            : null;

    // the API cache level must be Snapshot in order to facilitate nested field expansion and limiting
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IApiContent);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is IPublishedContent content
            ? _apiContentBuilder.Build(content)
            : null;

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

        return null;
    }
}
