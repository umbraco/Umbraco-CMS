using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Umbraco.Core.Media
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Gets the MIME type of an image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The MIME type of the image.</returns>
        public static string GetMimeType(this Image image)
        {
            var format = image.RawFormat;
            var codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
            return codec.MimeType;
        }
    }
}
