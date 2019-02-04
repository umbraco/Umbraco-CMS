using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using File = System.IO.File;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Checks if the parameter is IHaveUploadedFiles and then deletes any temporary saved files from file uploads associated with the request
    /// </summary>
    internal sealed class FileUploadCleanupFilterAttribute : ActionFilterAttribute
    {
        private readonly bool _incomingModel;

        /// <summary>
        /// Constructor specifies if the filter should analyze the incoming or outgoing model
        /// </summary>
        /// <param name="incomingModel"></param>
        public FileUploadCleanupFilterAttribute(bool incomingModel = true)
        {
            _incomingModel = incomingModel;
        }

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
                        //cleanup any files associated
                        foreach (var f in contentItem.UploadedFiles)
                        {
                            //track all temp folders so we can remove old files afterwards
                            var dir = Path.GetDirectoryName(f.TempFilePath);
                            if (tempFolders.Contains(dir) == false)
                            {
                                tempFolders.Add(dir);
                            }

                            try
                            {
                                File.Delete(f.TempFilePath);
                            }
                            catch (System.Exception ex)
                            {
                                Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                            }
                        }
                    }
                }
            }
            else
            {
                if (actionExecutedContext == null)
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext is null!!??");
                    return;
                }
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
                    objectContent = actionExecutedContext.Response.Content as ObjectContent;
                }
                catch (System.Exception ex)
                {
                    Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not acquire actionExecutedContext.Response.Content");
                    return;
                }

                if (objectContent != null)
                {
                    if (objectContent.Value is IHaveUploadedFiles uploadedFiles)
                    {
                        if (uploadedFiles.UploadedFiles != null)
                        {
                            //cleanup any files associated
                            foreach (var f in uploadedFiles.UploadedFiles)
                            {
                                if (f.TempFilePath.IsNullOrWhiteSpace() == false)
                                {
                                    //track all temp folders so we can remove old files afterwards
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
                                    catch (System.Exception ex)
                                    {
                                        Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                                    }

                                    //clear out the temp path so it's not returned in the response
                                    f.TempFilePath = "";
                                }
                                else
                                {
                                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The f.TempFilePath is null or whitespace!!??");
                                }
                            }
                        }
                        else
                        {
                            Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The uploadedFiles.UploadedFiles is null!!??");
                        }
                    }
                    else
                    {
                        Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content.Value is not IHaveUploadedFiles, it is {ObjectType}", objectContent.Value.GetType());
                    }
                }
                else
                {
                    Current.Logger.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content is not ObjectContent, it is {RequestObjectType}", actionExecutedContext.Request.Content.GetType());
                }
            }

            //Now remove all old files so that the temp folder(s) never grow
            foreach (var tempFolder in tempFolders.Distinct())
            {
                var files = Directory.GetFiles(tempFolder);
                foreach (var file in files)
                {
                    if (DateTime.UtcNow - File.GetLastWriteTimeUtc(file) > TimeSpan.FromDays(1))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (System.Exception ex)
                        {
                            Current.Logger.Error<FileUploadCleanupFilterAttribute>(ex, "Could not delete temp file {FileName}", file);
                        }
                    }
                }

            }

        }
    }
}
