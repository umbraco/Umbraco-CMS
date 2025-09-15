using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///    A value converter for the missing property editor, which always returns a string.
/// </summary>
[DefaultPropertyValueConverter]
public class MissingPropertyEditorValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => "Umb.PropertyEditorUi.Missing".Equals(propertyType.EditorUiAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(string);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString() ?? string.Empty;
}
