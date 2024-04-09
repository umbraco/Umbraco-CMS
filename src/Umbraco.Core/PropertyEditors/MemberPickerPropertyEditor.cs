namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MemberPicker,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class MemberPickerPropertyEditor : DataEditor
{
    public MemberPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
