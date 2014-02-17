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
    }
}
