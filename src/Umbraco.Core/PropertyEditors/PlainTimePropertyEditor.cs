namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less time properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainTime,
    EditorType.PropertyValue,
    "Configuration-less time",
    "not-applicable",
    Icon = "umb:edit",
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Time)]
public class PlainTimePropertyEditor : DataEditor
{
    public PlainTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, EditorType type = EditorType.PropertyValue)
        : base(dataValueEditorFactory, type)
        => SupportsReadOnly = true;
}
