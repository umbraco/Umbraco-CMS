using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using File = System.IO.File;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Checks if the parameter is IHaveUploadedFiles and then deletes any temporary saved files from file uploads associated with the Response.
    /// </summary>
    /// <seealso cref="System.Web.Http.Filters.ActionFilterAttribute" />
    internal sealed class FileUploadCleanupFilterAttribute : ActionFilterAttribute
    {
        private readonly bool _incomingModel;

        /// <summary>
        /// Constructor specifies if the filter should analyze the incoming or outgoing model.
        /// </summary>
        /// <param name="incomingModel">If set to <c>true</c>, the incoming model is analyzed.</param>
        public FileUploadCleanupFilterAttribute(bool incomingModel = true)
        {
            _incomingModel = incomingModel;
        }

        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            var tempFolders = new List<string>();

            if (_incomingModel)
            {
                if (actionExecutedContext.ActionContext.ActionArguments.Any())
                {
                    if (actionExecutedContext.ActionContext.ActionArguments.First().Value is IHaveUploadedFiles contentItem)
                    {
                        // Cleanup any files associated
                        foreach (var f in contentItem.UploadedFiles)
                        {
                            // Track all temp folders, so we can remove old files afterwards
                            var dir = Path.GetDirectoryName(f.TempFilePath);
                            if (tempFolders.Contains(dir) == false)
                            {
                                tempFolders.Add(dir);
                            }

                            try
                            {
                                File.Delete(f.TempFilePath);
                            }
                            catch (Exception ex)
                            {
                                Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                            }
                        }
                    }
                }
            }
            else if (actionExecutedContext.Response?.Content is ObjectContent objectContent &&
                objectContent.Value is IHaveUploadedFiles haveUploadedFiles &&
                haveUploadedFiles.UploadedFiles is var uploadedFiles && uploadedFiles != null)
            {
                // Cleanup any files associated
                foreach (var f in uploadedFiles)
                {
                    if (string.IsNullOrWhiteSpace(f.TempFilePath))
                    {
                        continue;
                    }

                    // Track all temp folders, so we can remove old files afterwards
                    var dir = Path.GetDirectoryName(f.TempFilePath);
                    if (tempFolders.Contains(dir) == false)
                    {
                        tempFolders.Add(dir);
                    }

                    try
                    {
                        File.Delete(f.TempFilePath);
                    }
                    catch (Exception ex)
                    {
                        Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                    }

                    // Clear out the temp path, so it's not returned in the response
                    f.TempFilePath = null;
                }
            }
        }
    }
}
