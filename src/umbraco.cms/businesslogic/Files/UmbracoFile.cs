using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Encoder = System.Text.Encoder;

namespace umbraco.cms.businesslogic.Files
{
    public class UmbracoFile : IFile
    {
        private string _fullFilePath;
        private string _fileName;
        private string _directoryName;
        private string _extension;
        private string _localName;
        private long _length;

        public UmbracoFile()
        {

        }

        public UmbracoFile(string fullFilePath)
        {
            _fullFilePath = fullFilePath;
            initialize();
        }

        public static UmbracoFile Save(HttpPostedFile file, string fullFileName)
        {
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            return Save(fileData, fullFileName);
        }

        public static UmbracoFile Save(HttpPostedFileBase file, string fullFileName)
        {
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
            }

            return Save(fileData, fullFileName);
        }

        public static UmbracoFile Save(Stream inputStream, string fullFileName){
           
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(inputStream))
            {
                fileData = binaryReader.ReadBytes((int)inputStream.Length);
            }

            return Save(fileData, fullFileName);
        }

        public static UmbracoFile Save(byte[] file, string fullFileName)
        {
            string fullFilePath = IO.IOHelper.MapPath(fullFileName);

            // create directories
            DirectoryInfo di = new DirectoryInfo(IO.IOHelper.MapPath(fullFilePath.Substring(0, fullFilePath.LastIndexOf(Path.DirectorySeparatorChar))));
            if (!di.Exists)
            {
                var currentDir = IO.IOHelper.MapPath(IO.SystemDirectories.Root);
                var rootDir = IO.IOHelper.MapPath(IO.SystemDirectories.Root);
                foreach (var dir in di.FullName.Substring(rootDir.Length).Split(Path.DirectorySeparatorChar))
                {
                    currentDir = Path.Combine(currentDir, dir);
                    if (!new DirectoryInfo(currentDir).Exists)
                    {
                        Directory.CreateDirectory(currentDir);
                    }
                }
            }

            File.WriteAllBytes(fullFilePath, file);
            return new UmbracoFile(fullFilePath);
        }

        public static UmbracoFile Save(HttpPostedFile file)
        {
            string tempDir = Path.Combine(IO.SystemDirectories.Media, "uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }
        //filebase overload...
        public static UmbracoFile Save(HttpPostedFileBase file)
        {
            string tempDir = Path.Combine(IO.SystemDirectories.Media, "uploads", Guid.NewGuid().ToString());
            return Save(file, tempDir);
        }

        private void initialize()
        {
            var fi = new FileInfo(_fullFilePath);
            _fileName = fi.Name;
            _length = fi.Length;
            _directoryName = fi.DirectoryName;
            _extension = fi.Extension.Substring(1).ToLowerInvariant();
            _localName =
                "/" + fi.FullName.Substring(IO.IOHelper.MapPath(IO.SystemDirectories.Root).Length).Replace(
                    Path.DirectorySeparatorChar.ToString(), "/");
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

        public string LocalName
        {
            get { return _localName; }
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


            FileStream fs = new FileStream(_fullFilePath,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            Image image = Image.FromStream(fs);
            var fileWidth = image.Width;
            var fileHeight = image.Height;
            fs.Close();
            image.Dispose();

            return new System.Tuple<int, int>(fileWidth, fileHeight);
        }

        public string Resize(int width, int height)
        {
            throwNotAnImageException();

            string fileNameThumb = DoResize(width, height, 0, String.Empty);

            return fileNameThumb.Substring(IO.IOHelper.MapPath(IO.SystemDirectories.Root).Length);
        }

        public string Resize(int maxWidthHeight, string fileNameAddition)
        {
            throwNotAnImageException();

            string fileNameThumb = DoResize(GetDimensions().Item1, GetDimensions().Item2, maxWidthHeight, fileNameAddition);

            return fileNameThumb.Substring(IO.IOHelper.MapPath(IO.SystemDirectories.Root).Length);
        }

        private string DoResize(int width, int height, int maxWidthHeight, string fileNameAddition)
        {

            FileStream fs = new FileStream(_fullFilePath,
                                           FileMode.Open, FileAccess.Read, FileShare.Read);
            Image image = Image.FromStream(fs);
            fs.Close();

            string fileNameThumb = String.IsNullOrEmpty(fileNameAddition) ?
                string.Format("{0}_UMBRACOSYSTHUMBNAIL.jpg", _fullFilePath.Substring(0, _fullFilePath.LastIndexOf("."))) :
                string.Format("{0}_{1}.jpg", _fullFilePath.Substring(0, _fullFilePath.LastIndexOf(".")), fileNameAddition);
            generateThumbnail(
                image,
                maxWidthHeight,
                width,
                height,
                _fullFilePath,
                _extension,
                fileNameThumb,
                maxWidthHeight == 0
                );
            image.Dispose();
            return fileNameThumb;
        }

        #endregion

        private void throwNotAnImageException()
        {
            if (!SupportsResizing)
                throw new NotAnImageException(string.Format("The file {0} is not an image, so can't get dimensions", _fileName));
        }


        private System.Tuple<int, int> generateThumbnail(System.Drawing.Image image, int maxWidthHeight, int fileWidth, int fileHeight, string fullFilePath, string ext, string thumbnailFileName, bool useFixedDimensions)
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
            bp.Save(thumbnailFileName.Replace("UMBRACOSYSTHUMBNAIL", string.Format("{0}x{1}", widthTh, heightTh)), codec, ep);
            bp.Dispose();
            g.Dispose();

            return new System.Tuple<int, int>(widthTh, heightTh);

        }


    }
}
