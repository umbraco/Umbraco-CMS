namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less time properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainTime,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.Time)]
public class PlainTimePropertyEditor : DataEditor
{
    public PlainTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
