using System.Collections.Generic;
using System.Text.Json.Serialization;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    public class ContentImagingSettings
    {
        private static readonly ImagingAutoFillUploadField[] DefaultImagingAutoFillUploadField =
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

        public IEnumerable<string> ImageFileTypes { get; set; } = new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" };

        [JsonPropertyName("AutoFillImageProperties")]
        public IEnumerable<IImagingAutoFillUploadField> ImageAutoFillProperties { get; set; } = DefaultImagingAutoFillUploadField;

        private class ImagingAutoFillUploadField : IImagingAutoFillUploadField
        {
            public string Alias { get; set; }
            public string WidthFieldAlias { get; set; }
            public string HeightFieldAlias { get; set; }
            public string LengthFieldAlias { get; set; }
            public string ExtensionFieldAlias { get; set; }
        }
    }
}
