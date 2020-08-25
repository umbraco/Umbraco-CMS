using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller used to return images for media
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ImagesController : UmbracoAuthorizedApiController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IContentSettings _contentSettings;
        private readonly IImageUrlGenerator _imageUrlGenerator;

        public ImagesController(IMediaFileSystem mediaFileSystem, IContentSettings contentSettings,
            IImageUrlGenerator imageUrlGenerator)
        {
            _mediaFileSystem = mediaFileSystem;
            _contentSettings = contentSettings;
            _imageUrlGenerator = imageUrlGenerator;
        }

        /// <summary>
        /// Gets the big thumbnail image for the original image path
        /// </summary>
        /// <param name="originalImagePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// If there is no original image is found then this will return not found.
        /// </remarks>
        public HttpResponseMessage GetBigThumbnail(string originalImagePath)
        {
            return string.IsNullOrWhiteSpace(originalImagePath)
                ? Request.CreateResponse(HttpStatusCode.OK)
                : GetResized(originalImagePath, 500);
        }

        /// <summary>
        /// Gets a resized image for the image at the given path
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        /// <remarks>
        /// If there is no media, image property or image file is found then this will return not found.
        /// </remarks>
        public HttpResponseMessage GetResized(string imagePath, int width)
        {
            var ext = Path.GetExtension(imagePath);

            // we need to check if it is an image by extension
            if (_contentSettings.IsImageFile(ext) == false)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            //redirect to ImageProcessor thumbnail with rnd generated from last modified time of original media file
            var response = Request.CreateResponse(HttpStatusCode.Found);

            DateTimeOffset? imageLastModified = null;
            try
            {
                imageLastModified = _mediaFileSystem.GetLastModified(imagePath);
            }
            catch (Exception)
            {
                // if we get an exception here it's probably because the image path being requested is an image that doesn't exist
                // in the local media file system. This can happen if someone is storing an absolute path to an image online, which
                // is perfectly legal but in that case the media file system isn't going to resolve it.
                // so ignore and we won't set a last modified date.
            }

            var rnd = imageLastModified.HasValue ? $"&rnd={imageLastModified:yyyyMMddHHmmss}" : null;
            var imageUrl = _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(imagePath)
            {
                UpScale = false, Width = width, AnimationProcessMode = "first", ImageCropMode = "max",
                CacheBusterValue = rnd
            });

            response.Headers.Location = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
            return response;
        }

        /// <summary>
        /// Gets a processed image for the image at the given path
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="focalPointLeft"></param>
        /// <param name="focalPointTop"></param>
        /// <param name="animationProcessMode"></param>
        /// <param name="mode"></param>
        /// <param name="upscale"></param>
        /// <param name="cacheBusterValue"></param>
        /// <param name="crop"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        /// <remarks>
        /// If there is no media, image property or image file is found then this will return not found.
        /// </remarks>
        public string GetProcessedImageUrl(string imagePath,
            int? width = null,
            int? height = null,
            decimal? focalPointLeft = null,
            decimal? focalPointTop = null,
            string animationProcessMode = "first",
            string mode = "max",
            bool upscale = false,
            string cacheBusterValue = "",
            decimal? cropX1 = null,
            decimal? cropX2 = null,
            decimal? cropY1 = null,
            decimal? cropY2 = null
            )
        {


            var options = new ImageUrlGenerationOptions(imagePath)
            {
                AnimationProcessMode = animationProcessMode,
                CacheBusterValue = cacheBusterValue,
                Height = height,
                ImageCropMode = mode,
                UpScale = upscale,
                Width = width,
                Crop = (cropX1.HasValue && cropX2.HasValue && cropY1.HasValue && cropY2.HasValue) ? new ImageUrlGenerationOptions.CropCoordinates(cropX1.Value, cropY1.Value, cropX2.Value, cropY2.Value) : null,
                FocalPoint = new ImageUrlGenerationOptions.FocalPointPosition(focalPointTop.GetValueOrDefault(0.5m), focalPointLeft.GetValueOrDefault(0.5m)),
            };
            if (focalPointLeft.HasValue && focalPointTop.HasValue)
            {
                options.FocalPoint =
                    new ImageUrlGenerationOptions.FocalPointPosition(focalPointTop.Value, focalPointLeft.Value);
            }

            return _imageUrlGenerator.GetImageUrl(options);
        }

        public class FocalPointPositionModel
        {
            public decimal Left { get; set; }
            public decimal Top { get; set; }
        }

        /// <summary>
        /// The bounds of the crop within the original image, in whatever units the registered
        /// IImageUrlGenerator uses, typically a percentage between 0 and 100.
        /// </summary>
        public class CropCoordinatesModel
        {

            public decimal X1 { get; set; }
            public decimal Y1 { get; set; }
            public decimal X2 { get; set;}
            public decimal Y2 { get; set;}
        }
    }
}
