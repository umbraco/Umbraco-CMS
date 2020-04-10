using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Media Picker",
        "mediapicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPickerPropertyEditor : DataEditor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerPropertyEditor"/> class.
        /// </summary>
        public MediaPickerPropertyEditor(ILogger logger)
            : base(logger)
        {
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MediaPickerPropertyValueEditor(Attribute);

        internal class MediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPickerPropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                foreach (var udiStr in asString.Split(','))
                {
                    if (Udi.TryParse(udiStr, out var udi))
                        yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }


}
