using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Core.PropertyEditors;

[DefaultPropertyValueConverter]
public class StringValueConverter : PropertyValueConverterBase
{
    private static readonly string[] PropertyTypeAliases =
    {
        Constants.PropertyEditors.Aliases.String
    };

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => PropertyTypeAliases.Contains(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType,
        object? source, bool preview)
        => source as string;
}
