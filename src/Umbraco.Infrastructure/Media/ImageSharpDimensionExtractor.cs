using System.IO;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core.Media;
using Size = System.Drawing.Size;

namespace Umbraco.Cms.Infrastructure.Media
{
    internal class ImageSharpDimensionExtractor : IImageDimensionExtractor
    {
        private readonly Configuration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpDimensionExtractor" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ImageSharpDimensionExtractor(Configuration configuration)
            => _configuration = configuration;

        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="stream">A stream containing the image bytes.</param>
        /// <returns>
        /// The dimension of the image.
        /// </returns>
        public Size? GetDimensions(Stream? stream)
        {
            Size? size = null;

            IImageInfo imageInfo = Image.Identify(_configuration, stream);
            if (imageInfo != null)
            {
                size = new Size(imageInfo.Width, imageInfo.Height);
            }

            return size;
        }
    }
}
