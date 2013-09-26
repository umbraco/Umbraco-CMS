using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    public class UmbracoMediaFile
    {
        private readonly MediaFileSystem _fs;

        #region Constructors

        public UmbracoMediaFile()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        public UmbracoMediaFile(string path)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            Path = path;

            Initialize();
        }

        #endregion

        #region Static Methods

        //MB: Do we really need all these overloads? looking through the code, only one of them is actually used

        public static UmbracoMediaFile Save(HttpPostedFile file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoMediaFile Save(HttpPostedFileBase file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoMediaFile Save(Stream inputStream, string path)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(path, inputStream);

            return new UmbracoMediaFile(path);
        }

        public static UmbracoMediaFile Save(byte[] file, string relativePath)
        {
            return Save(new MemoryStream(file), relativePath);
        }

        public static UmbracoMediaFile Save(HttpPostedFile file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        //filebase overload...
        public static UmbracoMediaFile Save(HttpPostedFileBase file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        #endregion

        private long? _length;
        private Size? _size;

        /// <summary>
        /// Initialized values that don't require opening the file.
        /// </summary>
        private void Initialize()
        {
            Filename = _fs.GetFileName(Path);
            Extension = _fs.GetExtension(Path) != null
                ? _fs.GetExtension(Path).Substring(1).ToLowerInvariant()
                : "";
            Url = _fs.GetUrl(Path);
            Exists = _fs.FileExists(Path);
            if (Exists == false)
            {
                LogHelper.Warn<UmbracoMediaFile>("The media file doesn't exist: " + Path);
            }
        }

        public bool Exists { get; private set; }

        public string Filename { get; private set; }

        public string Extension { get; private set; }

        public string Path { get; private set; }

        public string Url { get; private set; }

        /// <summary>
        /// Get the length of the file in bytes
        /// </summary>
        /// <remarks>
        /// We are lazy loading this, don't go opening the file on ctor like we were doing.
        /// </remarks>
        public long Length
        {
            get
            {
                if (_length == null)
                {
                    if (Exists)
                    {
                        _length = _fs.GetSize(Path);
                    }
                    else
                    {
                        _length = -1;
                    }
                }
                return _length.Value;
            }
        }

        public bool SupportsResizing
        {
            get
            {
                return UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(Extension);
            }
        }

        public string GetFriendlyName()
        {
            return Filename.SplitPascalCasing().ToFirstUpperInvariant();
        }

        public Size GetDimensions()
        {
            if (_size == null)
            {
                if (_fs.FileExists(Path))
                {
                    EnsureFileSupportsResizing();

                    using (var fs = _fs.OpenFile(Path))
                    {
                        using (var image = Image.FromStream(fs))
                        {
                            var fileWidth = image.Width;
                            var fileHeight = image.Height;
                            _size = new Size(fileWidth, fileHeight);
                        }
                    }
                }
                else
                {
                    _size = new Size(-1, -1);
                }                
            }
            return _size.Value;
        }

        public string Resize(int width, int height)
        {
            if (Exists)
            {
                EnsureFileSupportsResizing();

                var fileNameThumb = DoResize(width, height, 0, string.Empty);

                return _fs.GetUrl(fileNameThumb);    
            }
            return string.Empty;
        }

        public string Resize(int maxWidthHeight, string fileNameAddition)
        {
            if (Exists)
            {
                EnsureFileSupportsResizing();

                var fileNameThumb = DoResize(GetDimensions().Width, GetDimensions().Height, maxWidthHeight, fileNameAddition);

                return _fs.GetUrl(fileNameThumb);    
            }
            return string.Empty;
        }

        private string DoResize(int width, int height, int maxWidthHeight, string fileNameAddition)
        {
            using (var fs = _fs.OpenFile(Path))
            {
                using (var image = Image.FromStream(fs))
                {
                    var fileNameThumb = string.IsNullOrWhiteSpace(fileNameAddition)
                        ? string.Format("{0}_UMBRACOSYSTHUMBNAIL.jpg", Path.Substring(0, Path.LastIndexOf(".", StringComparison.Ordinal)))
                        : string.Format("{0}_{1}.jpg", Path.Substring(0, Path.LastIndexOf(".", StringComparison.Ordinal)), fileNameAddition);

                    var thumbnail = GenerateThumbnail(image, maxWidthHeight, width, height, fileNameThumb, maxWidthHeight == 0);

                    return thumbnail.FileName;
                }
            }    
        }

        private void EnsureFileSupportsResizing()
        {
            if (SupportsResizing == false)
                throw new InvalidOperationException(string.Format("The file {0} is not an image, so can't get dimensions", Filename));
        }

        private ResizedImage GenerateThumbnail(Image image, int maxWidthHeight, int fileWidth, int fileHeight, string thumbnailFileName, bool useFixedDimensions)
        {
            // Generate thumbnail
            float f = 1;
            if (useFixedDimensions == false)
            {
                var fx = (float)image.Size.Width / (float)maxWidthHeight;
                var fy = (float)image.Size.Height / (float)maxWidthHeight;

                // must fit in thumbnail size
                f = Math.Max(fx, fy);
            }

            var widthTh = (int)Math.Round((float)fileWidth / f);
            var heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            using (var bp = new Bitmap(widthTh, heightTh))
            {
                using (var g = Graphics.FromImage(bp))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    // Copy the old image to the new and resized
                    var rect = new Rectangle(0, 0, widthTh, heightTh);
                    g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

                    // Copy metadata
                    var imageEncoders = ImageCodecInfo.GetImageEncoders();

                    var codec = Extension.ToLower() == "png" || Extension.ToLower() == "gif"
                        ? imageEncoders.Single(t => t.MimeType.Equals("image/png"))
                        : imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));

                    // Set compresion ratio to 90%
                    var ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                    // Save the new image using the dimensions of the image
                    var newFileName = thumbnailFileName.Replace("UMBRACOSYSTHUMBNAIL", string.Format("{0}x{1}", widthTh, heightTh));
                    using (var ms = new MemoryStream())
                    {
                        bp.Save(ms, codec, ep);
                        ms.Seek(0, 0);

                        _fs.AddFile(newFileName, ms);
                    }

                    return new ResizedImage(widthTh, heightTh, newFileName);
                }
            }
        }
    }
}
