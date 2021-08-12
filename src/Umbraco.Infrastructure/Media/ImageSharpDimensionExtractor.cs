using System.IO;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core.Media;
using Size = System.Drawing.Size;

namespace Umbraco.Cms.Infrastructure.Media
{
    internal class ImageSharpDimensionExtractor : IImageDimensionExtractor
    {
        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="stream">A stream containing the image bytes.</param>
        /// <returns>
        /// The dimension of the image.
        /// </returns>
        public Size? GetDimensions(Stream stream)
        {
            if (stream.CanRead && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            Size? size = null;

            IImageInfo imageInfo = Image.Identify(stream);
            if (imageInfo != null)
            {
                size = new Size(imageInfo.Width, imageInfo.Height);
            }

            return size;
        }
    }
}
