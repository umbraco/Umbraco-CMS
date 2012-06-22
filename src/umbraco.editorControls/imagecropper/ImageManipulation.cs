using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace umbraco.editorControls.imagecropper
{
    public class ImageTransform
    {
        public static void Execute(string sourceFile, string name, int cropX, int cropY, int cropWidth, int cropHeight, int sizeWidth, int sizeHeight, long quality)
        {
            if (!File.Exists(sourceFile)) return;

            string path = sourceFile.Substring(0, sourceFile.LastIndexOf('\\'));

            // TODO: Make configurable and move to imageInfo
            //if(File.Exists(String.Format(@"{0}\{1}.jpg", path, name))) return;

            byte[] buffer = null;

            using(FileStream fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                fs.Close();
            }

            Image image = Image.FromStream(new MemoryStream(buffer));

            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists) di.Create();

            using(Image croppedImage = cropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight)))
            {
                using(Image resizedImage = resizeImage(croppedImage, new Size(sizeWidth, sizeHeight)))
                {
                    using (Bitmap b = new Bitmap(resizedImage))
                    {
                        saveJpeg(String.Format("{0}/{1}.jpg", path, name), b, quality);
                    }
                }
            }


            //saveJpeg(
            //    String.Format("{0}/{1}.jpg", path, name),
            //    new Bitmap(
            //        resizeImage(cropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight)), new Size(sizeWidth, sizeHeight))),
            //    quality
            //    );                  

            //using (FileStream stm = new FileStream(sourceFile, FileMode.Open, FileAccess.Read)) 
            //{
            //using (Image image = Image.FromStream(stm))
            //{
            
            //}
            //stm.Close();
            //}


            //using (Image image = Image.FromFile(sourceFile))
            //{
            //    //image = cropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight));
            //    //cropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight));
            //    //image = resizeImage(image, new Size(sizeWidth, sizeHeight));
            //    //resizeImage(image, new Size(sizeWidth, sizeHeight));
            //    string path = sourceFile.Substring(0, sourceFile.LastIndexOf('\\') + 1) + "Crops";
            //    DirectoryInfo di = new DirectoryInfo(path);
            //    if (!di.Exists) di.Create();
            //    saveJpeg(
            //        String.Format("{0}/{1}.jpg", path, name),
            //        new Bitmap(
            //            resizeImage(cropImage(image, new Rectangle(cropX, cropY, cropWidth, cropHeight)), new Size(sizeWidth, sizeHeight))),
            //        quality
            //        );

            //    image.Dispose();
            //}
        }

        private static void saveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        private static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            return (Image)(bmpCrop);
        }

        private static Image resizeImage(Image imgToResize, Size size)
        {
            //int sourceWidth = imgToResize.Width;
            //int sourceHeight = imgToResize.Height;

            //float nPercent = 0;
            //float nPercentW = 0;
            //float nPercentH = 0;

            //nPercentW = ((float)size.Width / (float)sourceWidth);
            //nPercentH = ((float)size.Height / (float)sourceHeight);

            //if (nPercentH < nPercentW)
            //    nPercent = nPercentH;
            //else
            //    nPercent = nPercentW;

            //int destWidth = (int)(sourceWidth * nPercent);
            //int destHeight = (int)(sourceHeight * nPercent);







            int destWidth = size.Width;
            int destHeight = size.Height;

            Bitmap b = new Bitmap(destWidth, destHeight);

            ImageAttributes ia = new ImageAttributes();
            ia.SetWrapMode(WrapMode.TileFlipXY);

            Graphics g = Graphics.FromImage(b);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.White);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(imgToResize, new Rectangle(0, 0, destWidth, destHeight), 0, 0, imgToResize.Width,
                        imgToResize.Height, GraphicsUnit.Pixel, ia);
            
            ia.Dispose();
            g.Dispose();

            return b;





#if false

            int destWidth = size.Width;
            int destHeight = size.Height;

            using (Bitmap b = new Bitmap(destWidth, destHeight))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    using (ImageAttributes ia = new ImageAttributes())
                    {
                        ia.SetWrapMode(WrapMode.TileFlipXY);
                        g.Clear(Color.White);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(imgToResize, new Rectangle(0, 0, destWidth, destHeight), 0, 0, imgToResize.Width,
                                    imgToResize.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                return b;
            }

#endif

#if false
            int destWidth = size.Width;
            int destHeight = size.Height;

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
#endif
        }
    }
}