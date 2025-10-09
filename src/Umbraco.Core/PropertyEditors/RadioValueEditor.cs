using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

[Obsolete("This is no longer used and has been migrated to an internal class within RadioButtonsPropertyEditor. Scheduled for removal in Umbraco 17.")]
public class RadioValueEditor : DataValueEditor
{
    public RadioValueEditor(
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute) =>
        Validators.Add(new RadioValueValidator());
}
