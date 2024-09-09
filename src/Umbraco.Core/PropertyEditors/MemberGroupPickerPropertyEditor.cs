using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

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

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MemberGroupPickerPropertyValueEditor>(Attribute!);

    private class MemberGroupPickerPropertyValueEditor : DataValueEditor
    {
        private readonly IMemberGroupService _memberGroupService;

        public MemberGroupPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IMemberGroupService memberGroupService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => _memberGroupService = memberGroupService;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // the stored value is a CSV of member group integer IDs - need to transform them into the corresponding member group keys
            var value = base.ToEditor(property, culture, segment);
            if (value is not string stringValue || stringValue.IsNullOrWhiteSpace())
            {
                return value;
            }

            var memberGroupIds = stringValue
                .Split(Constants.CharArrays.Comma)
                .Select(memberGroupIdStringValue =>
                    int.TryParse(memberGroupIdStringValue, out int memberId) ? memberId : -1)
                .Where(id => id > 0)
                .ToArray();

            IEnumerable<IMemberGroup> memberGroups = _memberGroupService.GetByIdsAsync(memberGroupIds).GetAwaiter().GetResult();
            return string.Join(',', memberGroups.Select(group => group.Key));
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            // the editor value is a CSV of member group keys - need to store a CSV of the corresponding member group integer IDs
            if (editorValue.Value is not string stringValue)
            {
                return null;
            }

            Guid[] memberGroupKeys = stringValue
                .Split(Constants.CharArrays.Comma)
                .Select(memberGroupKeyStringValue => Guid.TryParse(memberGroupKeyStringValue, out Guid memberGroupKey)
                    ? memberGroupKey
                    : Guid.Empty)
                .Where(memberGroupKey => memberGroupKey != Guid.Empty)
                .ToArray();

            IMemberGroup[] memberGroups = memberGroupKeys
                .Select(memberGroupKey => _memberGroupService.GetAsync(memberGroupKey).GetAwaiter().GetResult())
                .WhereNotNull()
                .ToArray();

            return string.Join(',', memberGroups.Select(memberGroup => memberGroup.Id));
        }
    }
}
