using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Media;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller used to return images for media
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ImagesController : UmbracoAuthorizedApiController
    {
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IContentSection _contentSection;
        private readonly IImageUrlGenerator _imageUrlGenerator;

        [Obsolete("This constructor will be removed in a future release.  Please use the constructor with the IImageUrlGenerator overload")]
        public ImagesController(IMediaFileSystem mediaFileSystem, IContentSection contentSection) : this (mediaFileSystem, contentSection, Current.ImageUrlGenerator)
        {
        }
        public ImagesController(IMediaFileSystem mediaFileSystem, IContentSection contentSection, IImageUrlGenerator imageUrlGenerator)
        {
            _mediaFileSystem = mediaFileSystem;
            _contentSection = contentSection;
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
            if (_contentSection.IsImageFile(ext) == false)
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
            var imageUrl = _imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(imagePath) { UpScale = false, Width = width, AnimationProcessMode = "first", ImageCropMode = "max", CacheBusterValue = rnd });

            response.Headers.Location = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
            return response;
        }
        
    }
}
