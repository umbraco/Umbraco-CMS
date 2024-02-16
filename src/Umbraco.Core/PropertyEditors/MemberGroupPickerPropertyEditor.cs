namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MemberGroupPicker,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class MemberGroupPickerPropertyEditor : DataEditor
{
    public MemberGroupPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
