using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using Umbraco.Core.IO;

namespace umbraco.editorControls.UrlPicker.AjaxUpload
{
    /// <summary>
    /// Allows uploading of files to the media folder via a simple http handler.
    /// Indirectly, this allows AJAX uploading of files.
    /// 
    /// Response is a JSON serialized response, e.g:
    /// -----------------------------------
    /// {
    ///     statusCode: 200,
    ///     statusDescription: All posted files were saved successfully,
    ///     filesSaved: {
    ///         0: ~/media/116/myImage.jpg
    ///     }
    /// }
    /// -----------------------------------
    /// 
    /// Other response codes returned are 400 (bad request) &amp; 500 (server file errors).
    /// </summary>
    /// <param>
    /// The integer ID of the upload - i.e, the subfolder of media which the file(s) will be
    /// saved in.
    /// 
    /// Must be nonnegative.
    /// </param>
    /// <param>
    /// Include any files via the "multipart/form-data" encoding type.  They will all be saved with their
    /// original filename.
    /// </param>
    public class AjaxUploadHandler : IHttpHandler
    {
        /// <summary>
        /// Used to filter out form fields which do no apply to this handler
        /// </summary>
        internal const string Guid = "84194AD4-24A9-483A-BA44-8F916E469A9B";

        /// <summary>
        /// An object to temporarily lock writing to disk.
        /// </summary>
        private static readonly object _locker = new object();

        // Constants
        private string SavePath
        {
            get
            {
                // Default media path
                return IOHelper.ResolveUrl(SystemDirectories.Media).TrimEnd('/');
            }
        }

        private static readonly string _idParam = Guid + "_Id";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            Authorize();

            int id;
            if (!int.TryParse(context.Request[_idParam], out id) || id < 0)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = string.Format("You must include a parameter named '{0}', which is a non-negative integer describing the subfolder under '{1}/' to save to.", _idParam, SavePath);
                WriteResponseBody(context);
                return;
            }

            var path = string.Format("{0}/{1}/", SavePath, id.ToString());

            // Save all relevant files which are posted
            var savedFiles = new List<string>();
            foreach (string key in context.Request.Files)
            {
                // Get the HttpPostedFile
                var file = context.Request.Files[key];

                // Check if this param is a file, and that the file is meant for this
                // handler (via the GUID).  Also check it isn't a null entry.
                if (key.Contains(Guid) && !string.IsNullOrEmpty(file.FileName.Trim()) && file.ContentLength > 0)
                {
                    // Create the directory for this transaction
                    var directory = IOHelper.MapPath(path);

                    var shortFileName = Path.GetFileName(file.FileName);
                    var fullFileName = directory + shortFileName;

                    // Save file
                    lock (_locker)
                    {
                        Directory.CreateDirectory(directory);

                        //if (File.Exists(fullFileName))
                        //{
                        //    context.Response.StatusCode = 500;
                        //    context.Response.StatusDescription = string.Format("File '{0}/{1}/{2}' already exists", SavePath, id, shortFileName);
                        //    WriteResponseBody(context);
                        //    return;
                        //}

                        try
                        {
                            file.SaveAs(fullFileName);
                        }
                        catch (Exception)
                        {
                            context.Response.StatusCode = 500;
                            context.Response.StatusDescription = string.Format("File '{0}/{1}/{2}' could not be saved", SavePath, id, shortFileName);
                            WriteResponseBody(context);
                            return;
                        }

                        // Log file
                        savedFiles.Add(string.Format("{0}/{1}/{2}", SavePath.TrimStart('~'), id, shortFileName));
                    }
                }
            }

            if (savedFiles.Count == 0)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "You must post at least one file";
                WriteResponseBody(context);
                return;
            }

            // Log saved files
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "All posted files were saved successfully";
            WriteResponseBody(context, savedFiles);
        }

        private void WriteResponseBody(HttpContext context)
        {
            WriteResponseBody(context, null);
        }

        private void WriteResponseBody(HttpContext context, List<string> filesSaved)
        {
            var jss = new JavaScriptSerializer();

            context.Response.ContentType = "text/plain";
            context.Response.Write(jss.Serialize(new
            {
                statusCode = context.Response.StatusCode,
                statusDescription = context.Response.StatusDescription,
                filesSaved = filesSaved
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        internal static void Authorize()
        {
            if (!umbraco.BasePages.BasePage.ValidateUserContextID(umbraco.BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");
        }

        /// <summary>
        /// Ensures that the handler is available on the file system
        /// </summary>
        internal static void Ensure()
        {
            var handlerFile = Path.Combine(IOHelper.MapPath(SystemDirectories.WebServices), "UrlPickerAjaxUploadHandler.ashx");

            if (!File.Exists(handlerFile))
            {
                lock (_locker)
                {
                    // double check locking
                    if (!File.Exists(handlerFile))
                    {
                        // now create our new local web service
                        var wHandlerFile = new FileInfo(handlerFile);
                        if (!wHandlerFile.Exists)
                        {
                            var wHandlerTxt = AjaxUploadHandlerResource.AjaxUploadHandler_ashx;

                            if (!wHandlerFile.Directory.Exists)
                            {
                                wHandlerFile.Directory.Create();
                            }

                            using (var sw = new StreamWriter(wHandlerFile.Create()))
                            {
                                sw.Write(wHandlerTxt);
                            }
                        }
                    }
                }
            }
        }
    }
}