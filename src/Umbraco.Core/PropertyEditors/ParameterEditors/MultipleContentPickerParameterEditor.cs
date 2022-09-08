using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors;

/// <summary>
///     Represents a parameter editor of some sort.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
    EditorType.MacroParameter,
    "Multiple Content Picker",
    "contentpicker")]
public class MultipleContentPickerParameterEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleContentPickerParameterEditor" /> class.
    /// </summary>
    public MultipleContentPickerParameterEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        // configure
        DefaultConfiguration.Add("multiPicker", "1");
        DefaultConfiguration.Add("minNumber", 0);
        DefaultConfiguration.Add("maxNumber", 0);
        SupportsReadOnly = true;
    }

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleContentPickerParamateterValueEditor>(Attribute!);

    internal class MultipleContentPickerParamateterValueEditor : MultiplePickerParamateterValueEditorBase
    {
        public MultipleContentPickerParamateterValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IEntityService entityService)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute, entityService)
        {
        }

        public override string UdiEntityType { get; } = Constants.UdiEntityType.Document;

        public override UmbracoObjectTypes UmbracoObjectType { get; } = UmbracoObjectTypes.Document;
    }
}
