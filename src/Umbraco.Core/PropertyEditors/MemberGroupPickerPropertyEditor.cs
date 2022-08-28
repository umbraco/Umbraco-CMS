namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MemberGroupPicker,
    "Member Group Picker",
    "membergrouppicker",
    ValueType = ValueTypes.Text,
    Group = Constants.PropertyEditors.Groups.People,
    Icon = Constants.Icons.MemberGroup,
    ValueEditorIsReusable = true)]
public class MemberGroupPickerPropertyEditor : DataEditor
{
    public MemberGroupPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;
}
