using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using File = System.IO.File;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Checks if the parameter is ContentItemSave and then deletes any temporary saved files from file uploads associated with the request
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

        /// <summary>
        /// Returns true so that other filters can execute along with this one
        /// </summary>
        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (_incomingModel)
            {
                if (actionExecutedContext.ActionContext.ActionArguments.Any())
                {
                    var contentItem = actionExecutedContext.ActionContext.ActionArguments.First().Value as IHaveUploadedFiles;
                    if (contentItem != null)
                    {
                        //cleanup any files associated
                        foreach (var f in contentItem.UploadedFiles)
                        {
                            File.Delete(f.TempFilePath);
                        }
                    }
                }
            }
            else
            {
                if (actionExecutedContext == null)
                {
                    LogHelper.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext is null!!??");
                    return;
                }
                if (actionExecutedContext.Request == null)
                {
                    LogHelper.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request is null!!??");
                    return;
                }
                if (actionExecutedContext.Request.Content == null)
                {
                    LogHelper.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content is null!!??");
                    return;
                }

                ObjectContent objectContent;
                
                try
                {
                    objectContent = actionExecutedContext.Response.Content as ObjectContent;
                }
                catch (System.Exception ex)
                {
                    LogHelper.Error<FileUploadCleanupFilterAttribute>("Could not acquire actionExecutedContext.Response.Content", ex);
                    return;
                }

                if (objectContent != null)
                {
                    var uploadedFiles = objectContent.Value as IHaveUploadedFiles;
                    if (uploadedFiles != null)
                    {
                        if (uploadedFiles.UploadedFiles != null)
                        {
                            //cleanup any files associated
                            foreach (var f in uploadedFiles.UploadedFiles)
                            {
                                if (f.TempFilePath.IsNullOrWhiteSpace() == false)
                                {
                                    LogHelper.Debug<FileUploadCleanupFilterAttribute>("Removing temp file " + f.TempFilePath);
                                    File.Delete(f.TempFilePath);
                                    //clear out the temp path so it's not returned in the response
                                    f.TempFilePath = "";
                                }
                                else
                                {
                                    LogHelper.Warn<FileUploadCleanupFilterAttribute>("The f.TempFilePath is null or whitespace!!??");   
                                }
                            }
                        }
                        else
                        {
                            LogHelper.Warn<FileUploadCleanupFilterAttribute>("The uploadedFiles.UploadedFiles is null!!??");   
                        }                        
                    }
                    else
                    {
                        LogHelper.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content.Value is not IHaveUploadedFiles, it is " + objectContent.Value.GetType());
                    }
                }
                else
                {
                    LogHelper.Warn<FileUploadCleanupFilterAttribute>("The actionExecutedContext.Request.Content is not ObjectContent, it is " + actionExecutedContext.Request.Content.GetType());
                }
            }
            
        }
    }
}