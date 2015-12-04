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

            //redirect to ImageProcessor thumbnail with rnd generated from last write time of original media file
            var fullOrgPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(imagePath));
            var response = Request.CreateResponse( HttpStatusCode.Found );
            response.Headers.Location = new Uri( string.Format( "{0}?rnd={1}&width={2}", imagePath, string.Format( "{0:yyyyMMddHHmmss}", System.IO.File.GetLastWriteTime( fullOrgPath ) ), width ), UriKind.Relative );
            return response;
        }
    }
}