using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.Files
{
    public class UmbracoFile : IFile
    {
        private readonly MediaFileSystem _fs;

        #region Constructors

        public UmbracoFile()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        public UmbracoFile(string path)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            Path = path;

            Initialize();
        }

        #endregion

        #region Static Methods

        //MB: Do we really need all these overloads? looking through the code, only one of them is actually used

        public static UmbracoFile Save(HttpPostedFile file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoFile Save(HttpPostedFileBase file, string path)
        {
            return Save(file.InputStream, path);
        }

        public static UmbracoFile Save(Stream inputStream, string path)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(path, inputStream);

            return new UmbracoFile(path);
        }

        public static UmbracoFile Save(byte[] file, string relativePath)
        {
            return Save(new MemoryStream(file), relativePath);
        }

        public static UmbracoFile Save(HttpPostedFile file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        //filebase overload...
        public static UmbracoFile Save(HttpPostedFileBase file)
        {
            var tempDir = System.IO.Path.Combine("uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        #endregion

        private void Initialize()
        {
            Filename = _fs.GetFileName(Path);
            Length = _fs.GetSize(Path);
            Extension = _fs.GetExtension(Path) != null
                ? _fs.GetExtension(Path).Substring(1).ToLowerInvariant()
                : "";
            Url = _fs.GetUrl(Path);
        }

        #region IFile Members

        public string Filename { get; private set; }

        public string Extension { get; private set; }

        [Obsolete("LocalName is obsolete, please use Url instead", false)]
        public string LocalName
        {
            get { return Url; }
        }

        public string Path { get; private set; }

        public string Url { get; private set; }

        public long Length { get; private set; }

        public bool SupportsResizing
        {
            get
            {
                return ("," + UmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", Extension));
            }
        }

        public string GetFriendlyName()
        {
            return Filename.SplitPascalCasing().ToFirstUpperInvariant();
        }

        public System.Tuple<int, int> GetDimensions()
        {
            EnsureFileSupportsResizing();

            var fs = _fs.OpenFile(Path);
            var image = Image.FromStream(fs);
            var fileWidth = image.Width;
            var fileHeight = image.Height;
            fs.Close();
            image.Dispose();

            return new System.Tuple<int, int>(fileWidth, fileHeight);
        }

        public string Resize(int width, int height)
        {
            EnsureFileSupportsResizing();

            var fileNameThumb = DoResize(width, height, 0, string.Empty);

            return _fs.GetUrl(fileNameThumb);
        }

        public string Resize(int maxWidthHeight, string fileNameAddition)
        {
            EnsureFileSupportsResizing();

            var fileNameThumb = DoResize(GetDimensions().Item1, GetDimensions().Item2, maxWidthHeight, fileNameAddition);

            return _fs.GetUrl(fileNameThumb);
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

        #endregion

        private void EnsureFileSupportsResizing()
        {
            if (SupportsResizing == false)
                throw new NotAnImageException(string.Format("The file {0} is not an image, so can't get dimensions", Filename));
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

    internal class ResizedImage
    {
        public ResizedImage()
        {
        }

        public ResizedImage(int width, int height, string fileName)
        {
            Width = width;
            Height = height;
            FileName = fileName;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public string FileName { get; set; }
    }
}
