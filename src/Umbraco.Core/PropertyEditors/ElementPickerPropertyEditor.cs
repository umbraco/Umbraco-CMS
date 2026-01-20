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
        private readonly IJsonSerializer _jsonSerializer;

        public ElementPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
            => _jsonSerializer = jsonSerializer;

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value as string ?? value?.ToString();
            if (string.IsNullOrEmpty(asString))
            {
                yield break;
            }

            IEnumerable<string>? elementUdis = _jsonSerializer.Deserialize<IEnumerable<string>>(asString);
            if (elementUdis is null)
            {
                yield break;
            }

            foreach (var elementUdi in elementUdis)
            {
                if (UdiParser.TryParse(elementUdi, out Udi? udi))
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}
