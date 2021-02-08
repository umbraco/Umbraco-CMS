using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker3,
        EditorType.PropertyValue,
        "Media Picker v3",
        "mediapicker3",
        ValueType = ValueTypes.Json,
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

        internal class MediaPicker3PropertyValueEditor : DataValueEditor
        {
            ///<remarks>
            /// Note no FromEditor() and ToEditor() methods
            /// We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string
            /// </remarks>
            public MediaPicker3PropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
            {
            }

            // TODO: Perhaps needed?! from IDataValueReference
            //public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            //{
            //    throw new NotImplementedException();
            //}    

        }



    }

}
