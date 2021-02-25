// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for content imaging settings.
    /// </summary>
    public class ContentImagingSettings
    {
        private static readonly ImagingAutoFillUploadField[] s_defaultImagingAutoFillUploadField =
        {
            new ImagingAutoFillUploadField
            {
                Alias = Constants.Conventions.Media.File,
                WidthFieldAlias = Constants.Conventions.Media.Width,
                HeightFieldAlias = Constants.Conventions.Media.Height,
                ExtensionFieldAlias = Constants.Conventions.Media.Extension,
                LengthFieldAlias = Constants.Conventions.Media.Bytes,
            }
        };

        /// <summary>
        /// Gets or sets a value for the collection of accepted image file extensions.
        /// </summary>
        public string[] ImageFileTypes { get; set; } = new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" };

        /// <summary>
        /// Gets or sets a value for the imaging autofill following media file upload fields.
        /// </summary>
        public ImagingAutoFillUploadField[] AutoFillImageProperties { get; set; } = s_defaultImagingAutoFillUploadField;
    }
}
