using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

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

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<UserPickerPropertyValueEditor>(Attribute!);

    private class UserPickerPropertyValueEditor : DataValueEditor
    {
        private readonly IUserService _userService;

        public UserPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IUserService userService)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => _userService = userService;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // the stored value is the user ID - need to transform this into the corresponding user key
            var value = base.ToEditor(property, culture, segment);
            if (value is not string stringValue || stringValue.IsNullOrWhiteSpace())
            {
                return value;
            }

            return int.TryParse(stringValue, out var userId)
                ? _userService.GetUserById(userId)?.Key
                : null;
        }

        // the editor value is expected to be the user key - store it as the user ID
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is string stringValue && Guid.TryParse(stringValue, out Guid userKey)
                ? _userService.GetAsync(userKey).GetAwaiter().GetResult()?.Id
                : null;
    }
}
