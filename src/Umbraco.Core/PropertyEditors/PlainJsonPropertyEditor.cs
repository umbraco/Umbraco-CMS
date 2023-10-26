namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less JSON properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainJson,
    EditorType.PropertyValue,
    "Configuration-less JSON",
    "not-applicable",
    Icon = "umb:edit",
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Json)]
public class PlainJsonPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainJsonPropertyEditor" /> class.
    /// </summary>
    public PlainJsonPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
