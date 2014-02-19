using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ImageCropperAlias, "Image Cropper", "imagecropper", ValueType = "JSON", HideLabel = false)]
    public class ImageCropperPropertyEditor : PropertyEditor
    {

        /// <summary>
        /// Creates our custom value editor
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();
            return new ImageCropperPropertyValueEditor(baseEditor);
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ImageCropperPreValueEditor();
        }


        public ImageCropperPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
                {
                    {"crops", "[]"},
                    {"focalPoint", "{left: 0.5, top: 0.5}"},
                    {"src", ""}
                };
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        internal class ImageCropperPreValueEditor : PreValueEditor
        {
            [PreValueField("crops", "Crop sizes", "cropsizes")]
            public string Crops { get; set; }
        }
    }
}
