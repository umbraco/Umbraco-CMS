using System.Collections.Generic;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration.Models
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

        public IEnumerable<IImagingAutoFillUploadField> AutoFillImageProperties { get; set; } = DefaultImagingAutoFillUploadField;

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
