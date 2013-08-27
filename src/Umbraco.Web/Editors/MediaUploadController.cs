using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /*
  * WebApi Implementation: Luke Baughan https://github.com/bUKaneer/jQuery-File-Upload
  * based on HttpHandler Implementation: Max Pavlov https://github.com/maxpavlov/jQuery-File-Upload.MVC3
  */

    [PluginController("UmbracoApi")]
    public class MediaUploadController : UmbracoApiController
        {
            private readonly JavaScriptSerializer _js = new JavaScriptSerializer { MaxJsonLength = 41943040 };
            private readonly string _storageRoot = Core.IO.IOHelper.MapPath( Core.IO.SystemDirectories.Media );
            public bool _isReusable { get { return false; } }


            public string Meh()
            {
                return "wo";
            }


            #region Get
            private HttpResponseMessage DownloadFileContent()
            {
                var filename = HttpContext.Current.Request["f"];
                var filePath = _storageRoot + filename;
                if (File.Exists(filePath))
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
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
                    .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
                    .Select(f => new FilesStatus(f))
                    .ToArray();
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "inline; filename=\"files.json\"");
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, _js.Serialize(files));
            }

            
            public HttpResponseMessage Get()
            {
                if (!string.IsNullOrEmpty(HttpContext.Current.Request["f"]))
                {
                    return DownloadFileContent();
                }
                return DownloadFileList();
            }
            #endregion

            #region Post & Put
            public HttpResponseMessage Post()
            {
                return UploadFile(HttpContext.Current);
            }

            public HttpResponseMessage Put()
            {
                return UploadFile(HttpContext.Current);
            }

            private HttpResponseMessage UploadFile(HttpContext context)
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

            private HttpResponseMessage WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
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
            private void UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses)
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
            private void UploadWholeFile(HttpContext context, List<FilesStatus> statuses)
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

            #region Delete
            public HttpResponseMessage Delete()
            {
                var filePath = _storageRoot + HttpContext.Current.Request["f"];
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, "");
            }
            #endregion
        }

        #region FileStatus
        public class FilesStatus
        {
            public const string HandlerPath = "/";

            public string group { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int size { get; set; }
            public string progress { get; set; }
            public string url { get; set; }
            public string thumbnail_url { get; set; }
            public string delete_url { get; set; }
            public string delete_type { get; set; }
            public string error { get; set; }

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
                name = fileName;
                type = "image/png";
                size = fileLength;
                progress = "1.0";
                url = HandlerPath + "api/Upload?f=" + fileName;
                delete_url = HandlerPath + "api/Upload?f=" + fileName;
                delete_type = "DELETE";
                var ext = Path.GetExtension(fullPath);
                var fileSize = ConvertBytesToMegabytes(new FileInfo(fullPath).Length);
                if (fileSize > 3 || !IsImage(ext))
                {
                    thumbnail_url = "/Content/img/generalFile.png";
                }
                else
                {
                    thumbnail_url = @"data:image/png;base64," + EncodeFile(fullPath);
                }
            }

            private bool IsImage(string ext)
            {
                return ext == ".gif" || ext == ".jpg" || ext == ".png";
            }

            private string EncodeFile(string fileName)
            {
                byte[] bytes;
                using (Image image = Image.FromFile(fileName))
                {
                    var ratioX = (double)80 / image.Width;
                    var ratioY = (double)80 / image.Height;
                    var ratio = Math.Min(ratioX, ratioY);
                    var newWidth = (int)(image.Width * ratio);
                    var newHeight = (int)(image.Height * ratio);
                    var newImage = new Bitmap(newWidth, newHeight);
                    Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
                    ImageConverter converter = new ImageConverter();
                    bytes = (byte[])converter.ConvertTo(newImage, typeof(byte[]));
                    newImage.Dispose();
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
