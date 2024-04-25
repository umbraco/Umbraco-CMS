using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

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

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MemberPickerPropertyValueEditor>(Attribute!);

    private class MemberPickerPropertyValueEditor : DataValueEditor
    {
        private readonly IMemberService _memberService;

        public MemberPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IMemberService memberService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => _memberService = memberService;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // the stored value is either an UDI or an integer ID - need to transform this into the corresponding member key
            var value = base.ToEditor(property, culture, segment);
            if (value is not string stringValue || stringValue.IsNullOrWhiteSpace())
            {
                return value;
            }

            if (UdiParser.TryParse<GuidUdi>(stringValue, out GuidUdi? guidUdi))
            {
                return guidUdi.Guid;
            }

            if (int.TryParse(stringValue, out int memberId))
            {
                return _memberService.GetById(memberId)?.Key;
            }

            return null;
        }

        // the editor value is expected to be the member key - store it as the member UDI
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is string stringValue && Guid.TryParse(stringValue, out Guid memberKey)
                ? new GuidUdi(Constants.UdiEntityType.Member, memberKey)
                : null;
    }
}
