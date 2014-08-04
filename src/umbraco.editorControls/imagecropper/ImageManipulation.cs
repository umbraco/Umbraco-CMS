using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using Umbraco.Core.IO;
using System.IO;


namespace umbraco.editorControls.imagecropper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ImageTransform
    {
        private static readonly MediaFileSystem _fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

        public static void Execute(string sourceFile, string name, int cropX, int cropY, int cropWidth, int cropHeight, int sizeWidth, int sizeHeight, long quality)
        {

            if (!_fs.FileExists(sourceFile)) return;


            string path = string.Empty;

            //http or local filesystem
            if (sourceFile.Contains("/"))
                path = sourceFile.Substring(0, sourceFile.LastIndexOf('/'));
            else
                path = sourceFile.Substring(0, sourceFile.LastIndexOf('\\'));

            // TODO: Make configurable and move to imageInfo
            //if(File.Exists(String.Format(@"{0}\{1}.jpg", path, name))) return;

            //Do we need this check as we are always working with images that are already in a folder??
            //DirectoryInfo di = new DirectoryInfo(path);
            //if (!di.Exists) di.Create();

            using (var stream = _fs.OpenFile(sourceFile))
            using (var image = Image.FromStream(stream))
            using (var croppedImage = CropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight)))
            using (var resizedImage = ResizeImage(croppedImage, new Size(sizeWidth, sizeHeight)))
            using (var b = new Bitmap(resizedImage))
            {
                SaveJpeg(String.Format("{0}/{1}.jpg", path, name), b, quality);
            }
           
        }

        private static void SaveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            using (var fileStream = new MemoryStream())
            {
                img.Save(fileStream, jpegCodec, encoderParams);
                fileStream.Position = 0;
                _fs.AddFile(path, fileStream, true);    
            }

        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        private static Image CropImage(Image img, Rectangle cropArea)
        {
            if (cropArea.Right > img.Width)
                cropArea.Width -= (cropArea.Right - img.Width);

            if (cropArea.Bottom > img.Height)
                cropArea.Height -= (cropArea.Bottom - img.Height);

            var bmpCrop = new Bitmap(cropArea.Width, cropArea.Height);

            using (var graphics = Graphics.FromImage(bmpCrop))
            {
                graphics.DrawImage(img, new Rectangle(0, 0, bmpCrop.Width, bmpCrop.Height), cropArea, GraphicsUnit.Pixel);
            }

            return bmpCrop;
        }

        private static Image ResizeImage(Image imgToResize, Size size)
        {
            int destWidth = size.Width;
            int destHeight = size.Height;

            Bitmap b = new Bitmap(destWidth, destHeight);

            using (var ia = new ImageAttributes())
            {
                ia.SetWrapMode(WrapMode.TileFlipXY);

                using (Graphics g = Graphics.FromImage(b))
                {
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(imgToResize, new Rectangle(0, 0, destWidth, destHeight), 0, 0, imgToResize.Width,
                                imgToResize.Height, GraphicsUnit.Pixel, ia);

                    ia.Dispose();
                }
            }
            

            return b;
        }
    }
}