using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Media picker property editors that stores crop data
    /// </summary>
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    [PropertyEditor(Constants.PropertyEditors.MediaCropperAlias, "Media Cropper", PropertyEditorValueTypes.Text, "mediacropper", IsParameterEditor = true, Group = "media", Icon = "icon-crop")]
    public class MediaCropperPropertyEditor : PropertyEditor
    {
        public MediaCropperPropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "udi"},
                {"focalPoint", "{left: 0.5, top: 0.5}"},
                {"src", ""}
            };
        }

        internal IDictionary<string, object> InternalPreValues;
        
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MediaCropperPreValueEditor();
        }

        internal class MediaCropperPreValueEditor : PreValueEditor
        {
            [PreValueField("crops", "Crop sizes", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
            public string Crops { get; set; }

            public MediaCropperPreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField()
                {
                    Key = "multiPicker",
                    View = "boolean",
                    Name = "Pick multiple items"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "startNodeId",
                    View = "mediapicker",
                    Name = "Start node",
                    Config = new Dictionary<string, object>
                    {
                        {"idType", "udi"}
                    }
                });
            }
        }
    }
}
