namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.UserPicker,
    ValueType = ValueTypes.Integer,
    ValueEditorIsReusable = true)]
public class UserPickerPropertyEditor : DataEditor
{
    public UserPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
