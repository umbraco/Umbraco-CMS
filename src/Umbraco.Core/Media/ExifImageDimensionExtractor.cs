using System;
using System.IO;
using Umbraco.Cms.Core.Media.Exif;

namespace Umbraco.Cms.Core.Media
{
    public static class ExifImageDimensionExtractor
    {
        public static bool TryGetDimensions(Stream stream, out int width, out int height)
        {
            var jpgInfo = ImageFile.FromStream(stream);
            height = -1;
            width = -1;
            if (jpgInfo != null
                && jpgInfo.Format != ImageFileFormat.Unknown
                && jpgInfo.Properties.ContainsKey(ExifTag.PixelYDimension)
                && jpgInfo.Properties.ContainsKey(ExifTag.PixelXDimension))
            {
                height = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelYDimension].Value);
                width = Convert.ToInt32(jpgInfo.Properties[ExifTag.PixelXDimension].Value);
            }

            return height > 0 && width > 0;
        }
    }
}
