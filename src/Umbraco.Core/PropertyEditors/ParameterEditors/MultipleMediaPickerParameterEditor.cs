using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors;

/// <summary>
///     Represents a multiple media picker macro parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultipleMediaPicker,
    EditorType.MacroParameter,
    "Multiple Media Picker",
    "mediapicker",
    ValueType = ValueTypes.Text)]
public class MultipleMediaPickerParameterEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleMediaPickerParameterEditor" /> class.
    /// </summary>
    public MultipleMediaPickerParameterEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        DefaultConfiguration.Add("multiPicker", "1");
        SupportsReadOnly = true;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleMediaPickerPropertyValueEditor>(Attribute!);

    internal class MultipleMediaPickerPropertyValueEditor : MultiplePickerParamateterValueEditorBase
    {
        public MultipleMediaPickerPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IEntityService entityService)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute, entityService)
        {
        }

        public override string UdiEntityType { get; } = Constants.UdiEntityType.Media;

        public override UmbracoObjectTypes UmbracoObjectType { get; } = UmbracoObjectTypes.Media;
    }
}
