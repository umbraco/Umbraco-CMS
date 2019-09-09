using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Core.IO;
using System.IO;
using System.Threading.Tasks;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using System.Linq;
using System;

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

            // Backoffice user ID (needed for unique folder path) to help with concurrent users
            // to avoid filename clash along with UTC current time
            var userId = Security.CurrentUser.Id;
            var imageTempPath = IOHelper.MapPath(SystemDirectories.TempImageUploads + "/" + userId + "_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            // Temp folderpath (Files come in as bodypart & will need to move/saved into imgTempPath
            var folderPath = IOHelper.MapPath(SystemDirectories.TempFileUploads);

            // Ensure image temp path exists
            if(Directory.Exists(imageTempPath) == false)
            {
                Directory.CreateDirectory(imageTempPath);
            }

            // File uploaded will be saved as bodypart into TEMP folder
            var provider = new MultipartFormDataStreamProvider(folderPath);
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

            // Really we should only have one file per request to this endpoint
            var file = result.FileData[0];
            var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' }).TrimEnd();
            var safeFileName = fileName.ToSafeFileName();
            var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

            if (Current.Configs.Settings().Content.IsFileAllowedForUpload(ext) == false || Current.Configs.Settings().Content.ImageFileTypes.Contains(ext) == false)
            {
                // Throw some error - to say can't upload this IMG type
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This is not an image filetype extension that is approved");
            }

            //var mediaItemName = fileName.ToFriendlyName();
            var currentFile = file.LocalFileName;
            var newFilePath = imageTempPath +  IOHelper.DirSepChar + safeFileName;
            var relativeNewFilePath = IOHelper.GetRelativePath(newFilePath);

            try
            {
                // Move the file from bodypart to a real filename
                // This is what we return from this API so RTE updates img src path
                // Until we fully persist & save the media item when persisting
                // If we find <img data-temp-img /> data attribute
                File.Move(currentFile, newFilePath);
            }
            catch (Exception ex)
            {
                // Could be a file permission ex
                throw;
            }            

            return Request.CreateResponse(HttpStatusCode.OK, new { tmpLocation = relativeNewFilePath });
        }
    }
}
