using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Ensures that no matter what is selected in (editor), the value results in a string.
/// </summary>
/// <remarks>
///     <para>
///         For more details see issues http://issues.umbraco.org/issue/U4-3776 (MNTP)
///         and http://issues.umbraco.org/issue/U4-4160 (media picker).
///     </para>
///     <para>
///         The cache level is set to .Content because the string is supposed to depend
///         on the source value only, and not on any other content. It is NOT appropriate
///         to use that converter for values whose .ToString() would depend on other content.
///     </para>
/// </remarks>
[DefaultPropertyValueConverter]
public class MustBeStringValueConverter : PropertyValueConverterBase
{
    private static readonly string[] Aliases = { Constants.PropertyEditors.Aliases.MultiNodeTreePicker };

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Aliases.Contains(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview) => source?.ToString();
}
