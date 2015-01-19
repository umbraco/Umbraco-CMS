using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
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
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                if (objectContent != null)
                {
                    var uploadedFiles = objectContent.Value as IHaveUploadedFiles;
                    if (uploadedFiles != null)
                    {
                        //cleanup any files associated
                        foreach (var f in uploadedFiles.UploadedFiles)
                        {
                            File.Delete(f.TempFilePath);
                            //clear out the temp path so it's not returned in the response
                            f.TempFilePath = "";
                        }
                    }
                }
            }
            
        }
    }
}