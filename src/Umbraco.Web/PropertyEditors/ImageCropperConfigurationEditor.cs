using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the image cropper value editor.
    /// </summary>
    internal class ImageCropperConfigurationEditor : ConfigurationEditor<ImageCropperConfiguration>
    {
        //fixme
        // BUT... focal point and src are NOT part of configuration?!
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            {"focalPoint", "{left: 0.5, top: 0.5}"},
            {"src", ""}
        };
    }
}