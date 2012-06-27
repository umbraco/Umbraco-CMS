using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using Encoder = System.Text.Encoder;

namespace umbraco.cms.businesslogic.media
{
    public class UmbracoImageMediaFactory : UmbracoMediaFactory
    {
        public override string MediaTypeAlias
        {
            get { return "Image"; }
        }

        public override List<string> Extensions
        {
            get { return new List<string> { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" }; }
        }

        public override void DoHandleMedia(Media media, PostedMediaFile postedFile, BusinessLogic.User user)
        {
            // Get Image object, width and height
            var image = System.Drawing.Image.FromStream(postedFile.InputStream);
            var fileWidth = image.Width;
            var fileHeight = image.Height;

            // Get umbracoFile property
            var propertyId = media.getProperty("umbracoFile").Id;

            // Get paths
            var destFileName = ConstructDestFileName(propertyId, postedFile.FileName);
            var destPath = ConstructDestPath(propertyId);
            var destFilePath = VirtualPathUtility.Combine(destPath, destFileName);
            var ext = VirtualPathUtility.GetExtension(destFileName).Substring(1);

            var absoluteDestPath = HttpContext.Current.Server.MapPath(destPath);
            var absoluteDestFilePath = HttpContext.Current.Server.MapPath(destFilePath);

            // Set media properties
            media.getProperty("umbracoFile").Value = destFilePath;
            media.getProperty("umbracoWidth").Value = fileWidth;
            media.getProperty("umbracoHeight").Value = fileHeight;
            media.getProperty("umbracoBytes").Value = postedFile.ContentLength;

            if (media.getProperty("umbracoExtension") != null)
                media.getProperty("umbracoExtension").Value = ext;

            if (media.getProperty("umbracoExtensio") != null)
                media.getProperty("umbracoExtensio").Value = ext;

            // Create directory
            if (UmbracoSettings.UploadAllowDirectories)
                Directory.CreateDirectory(absoluteDestPath);

            // Generate thumbnail
            var thumbDestFilePath = Path.Combine(absoluteDestPath, Path.GetFileNameWithoutExtension(destFileName) + "_thumb");
            GenerateThumbnail(image, 100, fileWidth, fileHeight, thumbDestFilePath + ".jpg");

            // Generate additional thumbnails based on PreValues set in DataTypeDefinition uploadField
            GenerateAdditionalThumbnails(image, fileWidth, fileHeight, thumbDestFilePath);

            image.Dispose();

            // Save file
            postedFile.SaveAs(absoluteDestFilePath);

            // Close stream
            postedFile.InputStream.Close();

            // Save media
            media.Save();
        }

        private static void GenerateAdditionalThumbnails(Image image, int fileWidth, int fileHeight, string destFilePath)
        {
            var uploadFieldDataTypeId = new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c");

            DataTypeDefinition dataTypeDef = null;

            try
            {
                // Get DataTypeDefinition of upload field
                dataTypeDef = DataTypeDefinition.GetByDataTypeId(uploadFieldDataTypeId);
            }
            catch { }

            if (dataTypeDef != null)
            {
                // Get PreValues
                var preValues = PreValues.GetPreValues(dataTypeDef.Id);

                var thumbnails = "";
                if (preValues.Count > 0)
                    thumbnails = ((PreValue)preValues[0]).Value;

                if (thumbnails != "")
                {
                    var thumbnailSizes = thumbnails.Split(";".ToCharArray());
                    foreach (var thumb in thumbnailSizes.Where(thumb => thumb != ""))
                    {
                        GenerateThumbnail(image, int.Parse(thumb), fileWidth, fileHeight,
                                          destFilePath + "_" + thumb + ".jpg");
                    }
                }
            }
        }

        private static void GenerateThumbnail(Image image, int maxWidthHeight, int fileWidth, int fileHeight, string thumbnailFileName)
        {
            // Generate thumbnailee
            var fx = (float)fileWidth / maxWidthHeight;
            var fy = (float)fileHeight / maxWidthHeight;

            // must fit in thumbnail size
            var f = Math.Max(fx, fy); //if (f < 1) f = 1;
            var widthTh = (int)Math.Round(fileWidth / f);
            var heightTh = (int)Math.Round(fileHeight / f);

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

            // Copy the old image to the new and resized
            var rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            var codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (var i = 0; codec == null && i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals("image/jpeg"))
                    codec = codecs[i];
            }

            // Set compresion ratio to 90%
            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

            // Save the new image
            if (codec != null)
            {
                bp.Save(thumbnailFileName, codec, ep);
            }
            else
            {
                // Log error
                Log.Add(LogTypes.Error, UmbracoEnsuredPage.CurrentUser, -1, "Multiple file upload: Can't find appropriate codec");
            }

            bp.Dispose();
            g.Dispose();
        }
    }
}
