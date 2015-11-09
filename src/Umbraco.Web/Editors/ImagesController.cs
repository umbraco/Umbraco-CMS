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
using Umbraco.Core.Models;
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

            return GetResized(media, 500);
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

            return GetResized(originalImagePath, 500);
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

            return GetResized( media, 500 );
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
            var media = Services.MediaService.GetMediaByPath( imagePath );
            if (media == null)
            {
                return new HttpResponseMessage( HttpStatusCode.NotFound );
            }

            return GetResized( media, 500 );
        }

        /// <summary>
        /// Gets a resized image by redirecting to ImageProcessor
        /// </summary>
        /// <param name="media"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private HttpResponseMessage GetResized(IMedia media, int width)
        {
            var imageProp = media.Properties[Constants.Conventions.Media.File];
            if (imageProp == null)
            {
                return Request.CreateResponse( HttpStatusCode.NotFound );
            }

            var imagePath = imageProp.Value.ToString();
            var response = Request.CreateResponse( HttpStatusCode.Found );
            response.Headers.Location = new Uri( string.Format( "{0}?rnd={1}&width={2}", imagePath, string.Format( "{0:yyyyMMddHHmmss}", media.UpdateDate ), width ), UriKind.Relative );
            return response;
        }
    }
}