using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Core.IO;
using System.IO;
using System.Threading.Tasks;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using System.Linq;
using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class TinyMceController : UmbracoAuthorizedApiController
    {
        private IMediaService _mediaService;
        private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

        public TinyMceController(IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadImage()
        {

            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath(SystemDirectories.TempFileUploads);

            // Ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            // Must have a file
            if (result.FileData.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            // Should only have one file
            if (result.FileData.Count > 1)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Only one file can be uploaded at a time");
            }

            // Check we have mediaParent as posted data
            if (string.IsNullOrEmpty(result.FormData["mediaParent"]))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing the Media Parent folder to save this image");
            }

            // Really we should only have one file per request to this endpoint
            var file = result.FileData[0];

            var parentFolder = Convert.ToInt32(result.FormData["mediaParent"]);

            var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' }).TrimEnd();
            var safeFileName = fileName.ToSafeFileName();
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (Current.Configs.Settings().Content.IsFileAllowedForUpload(ext) == false || Current.Configs.Settings().Content.ImageFileTypes.Contains(ext) == false)
            {
                // Throw some error - to say can't upload this IMG type
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This is not an image filetype extension that is approved");
            }

            var mediaItemName = fileName.ToFriendlyName();
            var f = _mediaService.CreateMedia(mediaItemName, parentFolder, Constants.Conventions.MediaTypes.Image, Security.CurrentUser.Id);
            var fileInfo = new FileInfo(file.LocalFileName);
            var fs = fileInfo.OpenReadWithRetry();
            if (fs == null) throw new InvalidOperationException("Could not acquire file stream");
            using (fs)
            {
                f.SetValue(Services.ContentTypeBaseServices, Constants.Conventions.Media.File, fileName, fs);
            }

            _mediaService.Save(f, Security.CurrentUser.Id);

            // Need to get URL to the media item and its UDI
            var udi = f.GetUdi();


            // TODO: Check this is the BEST way to get the URL
            // Ensuring if they use some CDN & blob storage?!
            var mediaTyped = Umbraco.Media(f.Id);
            var location = mediaTyped.Url;

            return Request.CreateResponse(HttpStatusCode.OK, new { location = location, udi = udi });
        }
    }
}
