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
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            var imageProp = media.Properties[Constants.Conventions.Media.File];
            if (imageProp == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

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
            if (string.IsNullOrWhiteSpace(originalImagePath))
                return Request.CreateResponse(HttpStatusCode.OK);

            return GetResized(originalImagePath, 500, "big-thumb");
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
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            var imageProp = media.Properties[Constants.Conventions.Media.File];
            if (imageProp == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

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
        /// <param name="suffix"></param>
        /// <returns></returns>
        private HttpResponseMessage GetResized(string imagePath, int width, string suffix)
        {
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var ext = Path.GetExtension(imagePath);

            //we need to check if it is an image by extension
            if (UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(ext.TrimStart('.')) == false)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var thumbFilePath = imagePath.TrimEnd(ext) + "_" + suffix + ".jpg";
            var fullOrgPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(imagePath));
            var fullNewPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(thumbFilePath));
            var thumbIsNew = mediaFileSystem.FileExists(fullNewPath) == false;
            if (thumbIsNew)
            {
                //we need to generate it
                if (mediaFileSystem.FileExists(fullOrgPath) == false)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                using (var fileStream = mediaFileSystem.OpenFile(fullOrgPath))
                {
                    if (fileStream.CanSeek) fileStream.Seek(0, 0);
                    using (var originalImage = Image.FromStream(fileStream))
                    {
                        //If it is bigger, then do the resize
                        if (originalImage.Width >= width && originalImage.Height >= width)
                        {
                            ImageHelper.GenerateThumbnail(
                                originalImage,
                                width,
                                fullNewPath,
                                "jpg",
                                mediaFileSystem);
                        }
                        else
                        {
                            //just return the original image
                            fullNewPath = fullOrgPath;
                        }
                        
                    }
                }
            }

            var result = Request.CreateResponse(HttpStatusCode.OK);
            //NOTE: That we are not closing this stream as the framework will do that for us, if we try it will
            // fail. See http://stackoverflow.com/questions/9541351/returning-binary-file-from-controller-in-asp-net-web-api
            var stream = mediaFileSystem.OpenFile(fullNewPath);
            if (stream.CanSeek) stream.Seek(0, 0);
            result.Content = new StreamContent(stream);
            result.Headers.Date = mediaFileSystem.GetLastModified(imagePath);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            return result;
        }
    }
}