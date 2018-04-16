using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the image cropper value editor.
    /// </summary>
    internal class ImageCropperConfigurationEditor : ConfigurationEditor<ImageCropperConfiguration>
    {
        /// <inheritdoc />
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);
            if (!d.ContainsKey("focalPoint")) d["focalPoint"] = new { left = 0.5, top = 0.5 };
            if (!d.ContainsKey("src")) d["src"] = "";
            return d;
        }
    }
}