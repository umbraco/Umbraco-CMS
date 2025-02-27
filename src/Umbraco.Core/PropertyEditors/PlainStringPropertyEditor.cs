namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less string properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainString,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Text)] // NOTE: for ease of use it's called "String", but it's really stored as TEXT
public class PlainStringPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainStringPropertyEditor" /> class.
    /// </summary>
    public PlainStringPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
