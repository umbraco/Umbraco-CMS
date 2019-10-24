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
    /// Checks if the parameter is IHaveUploadedFiles and then deletes any temporary saved files from file uploads associated with the request.
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
            else
            {
                if (actionExecutedContext.Request == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request is null!!??");
                    return;
                }

                if (actionExecutedContext.Request.Content == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content is null!!??");
                    return;
                }

                ObjectContent objectContent;
                try
                {
                    objectContent = actionExecutedContext.Request.Content as ObjectContent;
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not acquire actionExecutedContext.Request.Content");
                    return;
                }

                if (objectContent == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content is not ObjectContent, it is {RequestObjectType}", actionExecutedContext.Request.Content.GetType());
                }

                var uploadedFiles = objectContent.Value as IHaveUploadedFiles;
                if (uploadedFiles == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content.Value is not IHaveUploadedFiles, it is {ObjectType}", objectContent.Value.GetType());
                    return;
                }

                if (uploadedFiles.UploadedFiles == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The uploadedFiles.UploadedFiles is null!!??");
                    return;
                }

                // Cleanup any files associated
                foreach (var f in uploadedFiles.UploadedFiles)
                {
                    if (f.TempFilePath.IsNullOrWhiteSpace())
                    {
                        Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The f.TempFilePath is null or whitespace!!??");
                        continue;
                    }

                    // Track all temp folders, so we can remove old files afterwards
                    var dir = Path.GetDirectoryName(f.TempFilePath);
                    if (tempFolders.Contains(dir) == false)
                    {
                        tempFolders.Add(dir);
                    }

                    Current.Logger.Debug<FileUploadCleanupFilterAttribute>("Removing temp file {FileName}", f.TempFilePath);

                    try
                    {
                        File.Delete(f.TempFilePath);
                    }
                    catch (Exception ex)
                    {
                        Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                    }

                    // Clear out the temp path so it's not returned in the response
                    f.TempFilePath = "";
                }
            }
        }
    }
}
