namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less decimal properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainInteger,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Integer)]
public class PlainIntegerPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainIntegerPropertyEditor" /> class.
    /// </summary>
    public PlainIntegerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
