using System;
using System.Drawing;
using System.IO;
using Umbraco.Core;
using Umbraco.Web.Media.Exif;

namespace Umbraco.Web.Media
{
    public static class ImageHelper
    {
        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <param name="stream">A stream containing the image bytes.</param>
        /// <returns>The dimension of the image.</returns>
        /// <remarks>First try with EXIF as it is faster and does not load the entire image
        /// in memory. Fallback to GDI which means loading the image in memory and thus
        /// use potentially large amounts of memory.</remarks>
        public static Size GetDimensions(Stream stream)
        {   
            //Try to load with exif
            try
            {
                var jpgInfo = ImageFile.FromStream(stream);

                if (jpgInfo != null
                    && jpgInfo.Format != ImageFileFormat.Unknown
                    && jpgInfo.Properties.ContainsKey(ExifTag.PixelYDimension)
                    && jpgInfo.Properties.ContainsKey(ExifTag.PixelXDimension))
                {
                    var height = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelYDimension].Value);
                    var width = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelXDimension].Value);
                    if (height > 0 && width > 0)
                    {
                        return new Size(width, height);
                    }
                }
            }
            catch
            {
                //We will just swallow, just means we can't read exif data, we don't want to log an error either
            }

            //we have no choice but to try to read in via GDI
            try
            {
                using (var image = Image.FromStream(stream))
                {
                    var fileWidth = image.Width;
                    var fileHeight = image.Height;
                    return new Size(fileWidth, fileHeight);
                }
            }
            catch (Exception)
            {
                //We will just swallow, just means we can't read via GDI, we don't want to log an error either
            }

            return new Size(Constants.Conventions.Media.DefaultSize, Constants.Conventions.Media.DefaultSize);            
        }
    }
}
