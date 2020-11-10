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
        Constants.PropertyEditors.Aliases.MediaPicker3,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Media Picker v3",
        "mediapicker3",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPicker3PropertyEditor : DataEditor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyEditor"/> class.
        /// </summary>
        public MediaPicker3PropertyEditor(ILogger logger)
            : base(logger)
        {
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MediaPicker3PropertyValueEditor(Attribute);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPicker3PropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
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
