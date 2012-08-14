using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;
using Encoder = System.Text.Encoder;

namespace umbraco.cms.businesslogic.Files
{
    public class UmbracoFile : IFile
    {
        private string _path;
        private string _fileName;
        private string _extension;
        private string _url;
        private long _length;

        private IFileSystem _fs;

        #region Constructors

        public UmbracoFile()
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider(FileSystemProvider.Media);
        }

        public UmbracoFile(string path)
        {
            _fs = FileSystemProviderManager.Current.GetFileSystemProvider(FileSystemProvider.Media);

            _path = path;

            initialize();
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
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider(FileSystemProvider.Media);
            fs.AddFile(path, inputStream);

            return new UmbracoFile(path);
        }

        public static UmbracoFile Save(byte[] file, string relativePath)
        {
            return Save(new MemoryStream(file), relativePath);
        }

        public static UmbracoFile Save(HttpPostedFile file)
        {
            string tempDir = System.IO.Path.Combine(IO.SystemDirectories.Media, "uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        //filebase overload...
        public static UmbracoFile Save(HttpPostedFileBase file)
        {
            string tempDir = System.IO.Path.Combine(IO.SystemDirectories.Media, "uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        #endregion

        private void initialize()
        {
            _fileName = System.IO.Path.GetFileName(_path);
            _length = _fs.GetSize(_path);
            _extension = System.IO.Path.GetExtension(_path) != null
                ? System.IO.Path.GetExtension(_path).Substring(1).ToLowerInvariant()
                : "";
            _url = _fs.GetUrl(_path);
        }

        #region IFile Members

        public string Filename
        {
            get { return _fileName; }
        }

        public string Extension
        {

            get { return _extension; }
        }

        [Obsolete("LocalName is obsolete, please use Url instead", false)]
        public string LocalName
        {
            get { return Url; }
        }

        public string Path
        {
            get { return _path; }
        }

        public string Url
        {
            get { return _url; }
        }

        public long Length
        {
            get { return _length; }
        }

        public bool SupportsResizing
        {
            get
            {
                if (("," + UmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", _extension)))
                {
                    return true;
                }
                return false;
            }
        }

        public string GetFriendlyName()
        {
            return helpers.Casing.SpaceCamelCasing(_fileName);
        }

        public System.Tuple<int, int> GetDimensions()
        {
            throwNotAnImageException();

            var fs = _fs.OpenFile(_path);
            var image = Image.FromStream(fs);
            var fileWidth = image.Width;
            var fileHeight = image.Height;
            fs.Close();
            image.Dispose();

            return new System.Tuple<int, int>(fileWidth, fileHeight);
        }

        public string Resize(int width, int height)
        {
            throwNotAnImageException();

            var fileNameThumb = DoResize(width, height, 0, String.Empty);

            return _fs.GetUrl(fileNameThumb);
        }

        public string Resize(int maxWidthHeight, string fileNameAddition)
        {
            throwNotAnImageException();

            var fileNameThumb = DoResize(GetDimensions().Item1, GetDimensions().Item2, maxWidthHeight, fileNameAddition);

            return _fs.GetUrl(fileNameThumb);
        }

        private string DoResize(int width, int height, int maxWidthHeight, string fileNameAddition)
        {
            var fs = _fs.OpenFile(_path);
            var image = Image.FromStream(fs);
            fs.Close();

            string fileNameThumb = String.IsNullOrEmpty(fileNameAddition) ?
                string.Format("{0}_UMBRACOSYSTHUMBNAIL.jpg", _path.Substring(0, _path.LastIndexOf("."))) :
                string.Format("{0}_{1}.jpg", _path.Substring(0, _path.LastIndexOf(".")), fileNameAddition);

            fileNameThumb = generateThumbnail(
                image,
                maxWidthHeight,
                width,
                height,
                _path,
                _extension,
                fileNameThumb,
                maxWidthHeight == 0
                ).FileName;

            image.Dispose();
            
            return fileNameThumb;
        }

        #endregion

        private void throwNotAnImageException()
        {
            if (!SupportsResizing)
                throw new NotAnImageException(string.Format("The file {0} is not an image, so can't get dimensions", _fileName));
        }


        private ResizedImage generateThumbnail(System.Drawing.Image image, int maxWidthHeight, int fileWidth, int fileHeight, string fullFilePath, string ext, string thumbnailFileName, bool useFixedDimensions)
        {
            // Generate thumbnail
            float f = 1;
            if (!useFixedDimensions)
            {
                var fx = (float)image.Size.Width / (float)maxWidthHeight;
                var fy = (float)image.Size.Height / (float)maxWidthHeight;

                // must fit in thumbnail size
                f = Math.Max(fx, fy); //if (f < 1) f = 1;
            }

            var widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            var bp = new Bitmap(widthTh, heightTh);
            var g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Copy the old image to the new and resized
            var rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            var imageEncoders = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            if (Extension == "png" || Extension == ".gif")
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/png"));
            else
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));


            // Set compresion ratio to 90%
            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            // Save the new image using the dimensions of the image
            string newFileName = thumbnailFileName.Replace("UMBRACOSYSTHUMBNAIL",
                                                           string.Format("{0}x{1}", widthTh, heightTh));
            var ms = new MemoryStream();
            bp.Save(ms, codec, ep);
            ms.Seek(0, 0);

            _fs.AddFile(newFileName, ms);
            
            ms.Close();
            bp.Dispose();
            g.Dispose();

            return new ResizedImage(widthTh, heightTh, newFileName);

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
