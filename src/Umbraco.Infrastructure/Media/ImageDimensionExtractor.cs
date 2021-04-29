using System;
using System.IO;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Infrastructure.Media
{
    internal class ImageDimensionExtractor : IImageDimensionExtractor
    {
        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="stream">A stream containing the image bytes.</param>
        /// <returns>The dimension of the image.</returns>
        /// <remarks>First try with EXIF as it is faster and does not load the entire image
        /// in memory. Fallback to GDI which means loading the image in memory and thus
        /// use potentially large amounts of memory.</remarks>
        public ImageSize GetDimensions(Stream stream)
        {
            // Try to load with exif
            try
            {
                if (ExifImageDimensionExtractor.TryGetDimensions(stream, out var width, out var height))
                {
                    return new ImageSize(width, height);
                }
            }
            catch
            {
                // We will just swallow, just means we can't read exif data, we don't want to log an error either
            }

            // we have no choice but to try to read in via GDI
            try
            {
                if (stream.CanRead && stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                using (var image = Image.Load(stream))
                {
                    var fileWidth = image.Width;
                    var fileHeight = image.Height;
                    return new ImageSize(fileWidth, fileHeight);
                }
            }
            catch (Exception)
            {
                // We will just swallow, just means we can't read via GDI, we don't want to log an error either
            }

            return new ImageSize(Constants.Conventions.Media.DefaultSize, Constants.Conventions.Media.DefaultSize);
        }
    }
}
