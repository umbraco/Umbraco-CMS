using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the image cropper value editor.
    /// </summary>
    public class ImageCropperConfiguration
    {
        // fixme should not be a string!
        [ConfigurationField("crops", "Crop sizes", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
        public string Crops { get; set; }
    }
}