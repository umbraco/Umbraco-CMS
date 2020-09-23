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

        public string[] ImageFileTypes { get; set; } = new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" };

        public ImagingAutoFillUploadField[] AutoFillImageProperties { get; set; } = DefaultImagingAutoFillUploadField;
    }
}
