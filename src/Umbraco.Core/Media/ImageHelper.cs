using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Media.Exif;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Provides helper methods for managing images.
    /// </summary>
    internal static class ImageHelper
    {
        private static readonly Dictionary<int, string> DefaultSizes = new Dictionary<int, string>
        {
            { 100, "thumb" },
            { 500, "big-thumb" }
        };

        /// <summary>
        /// Gets a value indicating whether the file extension corresponds to an image.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>A value indicating whether the file extension corresponds to an image.</returns>
        public static bool IsImageFile(string extension)
        {
            if (extension == null) return false;
            extension = extension.TrimStart('.');
            return UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(extension);
        }

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

                if (jpgInfo.Format != ImageFileFormat.Unknown
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
            catch (Exception)
            {
                //We will just swallow, just means we can't read exif data, we don't want to log an error either
            }

            //we have no choice but to try to read in via GDI
            using (var image = Image.FromStream(stream))
            {

                var fileWidth = image.Width;
                var fileHeight = image.Height;
                return new Size(fileWidth, fileHeight);
            }           
        }

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

        #region GenerateThumbnails

        public static IEnumerable<ResizedImage> GenerateThumbnails(
            IFileSystem fs,
            Image image,
            string filepath,
            string preValue)
        {
            if (string.IsNullOrWhiteSpace(preValue))
                return GenerateThumbnails(fs, image, filepath);

            var additionalSizes = new List<int>();
            var sep = preValue.Contains(",") ? "," : ";";
            var values = preValue.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values)
            {
                int size;
                if (int.TryParse(value, out size))
                    additionalSizes.Add(size);
            }

            return GenerateThumbnails(fs, image, filepath, additionalSizes);
        }

        public static IEnumerable<ResizedImage> GenerateThumbnails(
            IFileSystem fs,
            Image image,
            string filepath, 
            IEnumerable<int> additionalSizes = null)
        {
            var w = image.Width;
            var h = image.Height;

            var sizes = additionalSizes == null ? DefaultSizes.Keys : DefaultSizes.Keys.Concat(additionalSizes);

            // start with default sizes,
            // add additional sizes,
            // filter out duplicates,
            // filter out those that would be larger that the original image
            // and create the thumbnail
            return sizes
                .Distinct()
                .Where(x => w >= x && h >= x)
                .Select(x => GenerateResized(fs, image, filepath, DefaultSizes.ContainsKey(x) ? DefaultSizes[x] : "", x))
                .ToList(); // now
        }

        public static IEnumerable<ResizedImage> GenerateThumbnails(
            IFileSystem fs, 
            Stream filestream,
            string filepath,
            PropertyType propertyType)
        {
            // get the original image from the original stream
            if (filestream.CanSeek) filestream.Seek(0, 0); // fixme - what if we cannot seek?
            using (var image = Image.FromStream(filestream))
            {
                return GenerateThumbnails(fs, image, filepath, propertyType);
            }
        }

        public static IEnumerable<ResizedImage> GenerateThumbnails(
            IFileSystem fs, 
            Image image,
            string filepath,
            PropertyType propertyType)
        {
            // if the editor is an upload field, check for additional thumbnail sizes
            // that can be defined in the prevalue for the property data type. otherwise,
            // just use the default sizes.
            var sizes = propertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias
                ? ApplicationContext.Current.Services.DataTypeService
                    .GetPreValuesByDataTypeId(propertyType.DataTypeDefinitionId)
                    .FirstOrDefault()
                : string.Empty;

            return GenerateThumbnails(fs, image, filepath, sizes);
        }

        #endregion

        #region GenerateResized - Generate at resized filepath derived from origin filepath

        public static ResizedImage GenerateResized(IFileSystem fs, Image originImage, string originFilepath, string sizeName, int maxWidthHeight)
        {
            return GenerateResized(fs, originImage, originFilepath, sizeName, maxWidthHeight, -1, -1);
        }

        public static ResizedImage GenerateResized(IFileSystem fs, Image originImage, string originFilepath, string sizeName, int fixedWidth, int fixedHeight)
        {
            return GenerateResized(fs, originImage, originFilepath, sizeName, -1, fixedWidth, fixedHeight);
        }

        public static ResizedImage GenerateResized(IFileSystem fs, Image originImage, string originFilepath, string sizeName, int maxWidthHeight, int fixedWidth, int fixedHeight)
        {
            if (string.IsNullOrWhiteSpace(sizeName))
                sizeName = "UMBRACOSYSTHUMBNAIL";
            var extension = Path.GetExtension(originFilepath) ?? string.Empty;
            var filebase = originFilepath.TrimEnd(extension);
            var resizedFilepath = filebase + "_" + sizeName + ".jpg";

            return GenerateResizedAt(fs, originImage, resizedFilepath, maxWidthHeight, fixedWidth, fixedHeight);
        }

        #endregion

        #region GenerateResizedAt - Generate at specified resized filepath

        public static ResizedImage GenerateResizedAt(IFileSystem fs, Image originImage, string resizedFilepath, int maxWidthHeight)
        {
            return GenerateResizedAt(fs, originImage, resizedFilepath, maxWidthHeight, -1, -1);
        }

        public static ResizedImage GenerateResizedAt(IFileSystem fs, Image originImage, int fixedWidth, int fixedHeight, string resizedFilepath)
        {
            return GenerateResizedAt(fs, originImage, resizedFilepath, -1, fixedWidth, fixedHeight);
        }

        public static ResizedImage GenerateResizedAt(IFileSystem fs, Image originImage, string resizedFilepath, int maxWidthHeight, int fixedWidth, int fixedHeight)
        {
            // target dimensions
            int width, height;

            // if maxWidthHeight then get ratio
            if (maxWidthHeight > 0)
            {
                var fx = (float) originImage.Size.Width / maxWidthHeight;
                var fy = (float) originImage.Size.Height / maxWidthHeight;
                var f = Math.Max(fx, fy); // fit in thumbnail size
                width = (int) Math.Round(originImage.Size.Width / f);
                height = (int) Math.Round(originImage.Size.Height / f);
                if (width == 0) width = 1;
                if (height == 0) height = 1;
            }
            else if (fixedWidth > 0 && fixedHeight > 0)
            {
                width = fixedWidth;
                height = fixedHeight;
            }
            else
            {
                width = height = 1;
            }

            // create new image with best quality settings
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // if the image size is rather large we cannot use the best quality interpolation mode
                // because we'll get out of mem exceptions. So we detect how big the image is and use
                // the mid quality interpolation mode when the image size exceeds our max limit.
                graphics.InterpolationMode = originImage.Width > 5000 || originImage.Height > 5000
                    ? InterpolationMode.Bilinear // mid quality
                    : InterpolationMode.HighQualityBicubic; // best quality

                // everything else is best-quality
                graphics.SmoothingMode = SmoothingMode.HighQuality;                    
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // copy the old image to the new and resize
                var rect = new Rectangle(0, 0, width, height);
                graphics.DrawImage(originImage, rect, 0, 0, originImage.Width, originImage.Height, GraphicsUnit.Pixel);

                // copy metadata
                // fixme - er... no?

                // get an encoder - based upon the file type
                var extension = (Path.GetExtension(resizedFilepath) ?? "").TrimStart('.').ToLowerInvariant();
                var encoders = ImageCodecInfo.GetImageEncoders();
                var encoder = extension == "png" || extension == "gif"
                    ? encoders.Single(t => t.MimeType.Equals("image/png"))
                    : encoders.Single(t => t.MimeType.Equals("image/jpeg"));

                // set compresion ratio to 90%
                var encoderParams = new EncoderParameters();
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                // save the new image
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, encoder, encoderParams);
                    stream.Seek(0, 0);
                    if (resizedFilepath.Contains("UMBRACOSYSTHUMBNAIL"))
                    {
                        var filepath = resizedFilepath.Replace("UMBRACOSYSTHUMBNAIL", maxWidthHeight.ToInvariantString());
                        fs.AddFile(filepath, stream);
                        // TODO: Remove this, this is ONLY here for backwards compatibility but it is essentially completely unusable see U4-5385
                        stream.Seek(0, 0);
                        resizedFilepath = resizedFilepath.Replace("UMBRACOSYSTHUMBNAIL", width + "x" + height);
                    }

                    fs.AddFile(resizedFilepath, stream);
                }

                return new ResizedImage(resizedFilepath, width, height);
            }
        }

        #endregion

        #region Inner classes

        public class ResizedImage
        {
            public ResizedImage()
            { }

            public ResizedImage(string filepath, int width, int height)
            {
                Filepath = filepath;
                Width = width;
                Height = height;
            }

            public string Filepath { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        #endregion
    }
}
