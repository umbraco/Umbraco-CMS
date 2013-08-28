using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /*
  * WebApi Implementation: Luke Baughan https://github.com/bUKaneer/jQuery-File-Upload
  * based on HttpHandler Implementation: Max Pavlov https://github.com/maxpavlov/jQuery-File-Upload.MVC3
  */

    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Media)]
    public class MediaUploadController : UmbracoAuthorizedApiController
    {
        private readonly JavaScriptSerializer _js = new JavaScriptSerializer { MaxJsonLength = 41943040 };
        private readonly string _storageRoot = Core.IO.IOHelper.MapPath(Core.IO.SystemDirectories.Media);

        #region Get
        private HttpResponseMessage DownloadFileContent()
        {
            var filename = HttpContext.Current.Request["f"];
            var filePath = _storageRoot + filename;
            if (File.Exists(filePath))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = filename
                };
                return response;
            }
            return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "");
        }

        private HttpResponseMessage DownloadFileList()
        {
            var files =
            new DirectoryInfo(_storageRoot)
                .GetFiles("*", SearchOption.TopDirectoryOnly)
                .Where(f => f.Attributes.HasFlag(FileAttributes.Hidden) == false)
                .Select(f => new FilesStatus(f))
                .ToArray();
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "inline; filename=\"files.json\"");
            return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, _js.Serialize(files));
        }

        public HttpResponseMessage Get()
        {
            var http = TryGetHttpContext();
            if (http.Success == false)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (string.IsNullOrEmpty(http.Result.Request["f"]) == false)
            {
                return DownloadFileContent();
            }
            return DownloadFileList();
        }
        #endregion

        #region Post & Put
        public HttpResponseMessage Post()
        {
            var http = TryGetHttpContext();
            return http.Success == false
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : UploadFile(http.Result);
        }

        public HttpResponseMessage Put()
        {
            var http = TryGetHttpContext();
            return http.Success == false
                ? Request.CreateResponse(HttpStatusCode.NotFound)
                : UploadFile(http.Result);
        }

        private HttpResponseMessage UploadFile(HttpContextBase context)
        {
            var statuses = new List<FilesStatus>();
            var headers = context.Request.Headers;

            if (string.IsNullOrEmpty(headers["X-File-Name"]))
            {
                UploadWholeFile(context, statuses);
            }
            else
            {
                UploadPartialFile(headers["X-File-Name"], context, statuses);
            }

            return WriteJsonIframeSafe(context, statuses);
        }

        private HttpResponseMessage WriteJsonIframeSafe(HttpContextBase context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            var response = new HttpResponseMessage()
            {
                Content = new StringContent(_js.Serialize(statuses.ToArray()))
            };
            if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            }
            return response;
        }

        // Upload partial file
        private void UploadPartialFile(string fileName, HttpContextBase context, List<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var inputStream = context.Request.Files[0].InputStream;
            var fullName = _storageRoot + Path.GetFileName(fileName);

            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(new FilesStatus(new FileInfo(fullName)));
        }

        // Upload entire file
        private void UploadWholeFile(HttpContextBase context, List<FilesStatus> statuses)
        {
            var folderId = context.Request.Form["currentFolder"];


            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                string fullPath = _storageRoot + Path.GetFileName(file.FileName);
                Directory.CreateDirectory(_storageRoot);

                file.SaveAs(fullPath);
                string fullName = Path.GetFileName(file.FileName);
                statuses.Add(new FilesStatus(fullName, file.ContentLength, fullPath));
            }
        }
        #endregion

    }

    #region FileStatus

    [DataContract(Name = "fileStatus", Namespace = "")]
    internal class FilesStatus
    {
        public const string HandlerPath = "/";

        [DataMember(Name = "group")]
        public string Group { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        [DataMember(Name = "progress")]
        public string Progress { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [DataMember(Name = "delete_url")]
        public string DeleteUrl { get; set; }

        [DataMember(Name = "delete_type")]
        public string DeleteType { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        public FilesStatus()
        {
        }

        public FilesStatus(FileInfo fileInfo)
        {
            SetValues(fileInfo.Name, (int)fileInfo.Length, fileInfo.FullName);
        }

        public FilesStatus(string fileName, int fileLength, string fullPath)
        {
            SetValues(fileName, fileLength, fullPath);
        }

        private void SetValues(string fileName, int fileLength, string fullPath)
        {
            Name = fileName;
            Type = "image/png";
            Size = fileLength;
            Progress = "1.0";
            Url = HandlerPath + "api/Upload?f=" + fileName;
            DeleteUrl = HandlerPath + "api/Upload?f=" + fileName;
            DeleteType = "DELETE";
            var ext = Path.GetExtension(fullPath);
            var fileSize = ConvertBytesToMegabytes(new FileInfo(fullPath).Length);
            if (fileSize > 3 || !IsImage(ext))
            {
                ThumbnailUrl = "/Content/img/generalFile.png";
            }
            else
            {
                ThumbnailUrl = @"data:image/png;base64," + EncodeFile(fullPath);
            }
        }

        private static bool IsImage(string ext)
        {
            return ext.InvariantEquals(".gif") || ext.InvariantEquals(".jpg") || ext.InvariantEquals(".png");
        }

        private static string EncodeFile(string fileName)
        {
            byte[] bytes = null;
            using (var image = Image.FromFile(fileName))
            {
                var ratioX = (double)80 / image.Width;
                var ratioY = (double)80 / image.Height;
                var ratio = Math.Min(ratioX, ratioY);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);
                using (var newImage = new Bitmap(newWidth, newHeight))
                {
                    using (var g = Graphics.FromImage(newImage))
                    {
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                        var converter = new ImageConverter();
                        bytes = (byte[])converter.ConvertTo(newImage, typeof(byte[]));
                    }
                }

            }
            if (bytes == null)
            {
                throw new InvalidOperationException("Could not read the bytes from the file as an image");
            }
            return Convert.ToBase64String(bytes);
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
    #endregion

}
