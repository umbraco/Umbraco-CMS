using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller used to return images for media
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ImagesController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Gets the big thumbnail image for the media id
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        /// <remarks>
        /// If there is no media, image property or image file is found then this will return not found.
        /// </remarks>
        public HttpResponseMessage GetBigThumbnail(int mediaId)
        {
            var media = Services.MediaService.GetById(mediaId);
            if (media == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            
            var imageProp = media.Properties[Constants.Conventions.Media.File];
            if (imageProp == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var imagePath = imageProp.Value.ToString();
            return GetBigThumbnail(imagePath);
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
                : GetResized(originalImagePath, 500, "big-thumb");
        }

        /// <summary>
        /// Gets a resized image for the media id
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        /// <remarks>
        /// If there is no media, image property or image file is found then this will return not found.
        /// </remarks>
        public HttpResponseMessage GetResized(int mediaId, int width)
        {
            var media = Services.MediaService.GetById(mediaId);
            if (media == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            
            var imageProp = media.Properties[Constants.Conventions.Media.File];
            if (imageProp == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var imagePath = imageProp.Value.ToString();
            return GetResized(imagePath, width);
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
            return GetResized(imagePath, width, Convert.ToString(width));
        }

        //TODO: We should delegate this to ImageProcessing

        /// <summary>
        /// Gets a resized image - if the requested max width is greater than the original image, only the original image will be returned.
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="width"></param>
        /// <param name="sizeName"></param>
        /// <returns></returns>
        private HttpResponseMessage GetResized(string imagePath, int width, string sizeName)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var ext = Path.GetExtension(imagePath);

            // we need to check if it is an image by extension
            if (ImageHelper.IsImageFile(ext) == false)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var resizedPath = imagePath.TrimEnd(ext) + "_" + sizeName + ext;
            var generate = fs.FileExists(resizedPath) == false;
            if (generate)
            {
                // we need to generate it - if we have a source
                if (fs.FileExists(imagePath) == false)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                using (var fileStream = fs.OpenFile(imagePath))
                using (var originalImage = Image.FromStream(fileStream))
                {
                    // if original image is bigger than requested size, then resize, else return original image
                    if (originalImage.Width >= width && originalImage.Height >= width)
                        ImageHelper.GenerateResizedAt(fs, originalImage, resizedPath, width);
                    else
                        resizedPath = imagePath;
                }
            }

            var result = Request.CreateResponse(HttpStatusCode.OK);
            //NOTE: That we are not closing this stream as the framework will do that for us, if we try it will
            // fail. See http://stackoverflow.com/questions/9541351/returning-binary-file-from-controller-in-asp-net-web-api
            var stream = fs.OpenFile(resizedPath);
            if (stream.CanSeek) stream.Seek(0, 0);
            result.Content = new StreamContent(stream);
            result.Headers.Date = fs.GetLastModified(imagePath);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(System.Web.MimeMapping.GetMimeMapping(imagePath));
            
            return result;
        }
    }
}