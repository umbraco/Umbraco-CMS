using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Element picker property editor that stores element keys
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ElementPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class ElementPickerPropertyEditor : DataEditor
{
    public ElementPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ElementPickerPropertyValueEditor>(Attribute!);

    internal sealed class ElementPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public ElementPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        // TODO ELEMENTS: implement reference tracking from element picker
        public IEnumerable<UmbracoEntityReference> GetReferences(object? value) => [];
    }
}
