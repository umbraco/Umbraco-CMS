using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Checks if the parameter is IHaveUploadedFiles and then deletes any temporary saved files from file uploads
///     associated with the request
/// </summary>
public sealed class FileUploadCleanupFilterAttribute : TypeFilterAttribute
{
    /// <summary>
    ///     Constructor specifies if the filter should analyze the incoming or outgoing model
    /// </summary>
    /// <param name="incomingModel"></param>
    public FileUploadCleanupFilterAttribute(bool incomingModel = true) : base(typeof(FileUploadCleanupFilter)) =>
        Arguments = new object[] { incomingModel };

    // We need to use IAsyncActionFilter even that we dont have any async because we need access to
    // context.ActionArguments, and this is only available on ActionExecutingContext and not on
    // ActionExecutedContext

    private class FileUploadCleanupFilter : IAsyncActionFilter
    {
        private readonly bool _incomingModel;
        private readonly ILogger<FileUploadCleanupFilter> _logger;

        public FileUploadCleanupFilter(ILogger<FileUploadCleanupFilter> logger, bool incomingModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _incomingModel = incomingModel;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ActionExecutedContext resultContext = await next(); // We only to do stuff after the action is executed

            var tempFolders = new List<string>();

            if (_incomingModel)
            {
                if (context.ActionArguments.Any())
                {
                    if (context.ActionArguments.First().Value is IHaveUploadedFiles contentItem)
                    {
                        //cleanup any files associated
                        foreach (ContentPropertyFile f in contentItem.UploadedFiles)
                        {
                            //track all temp folders so we can remove old files afterwards
                            var dir = Path.GetDirectoryName(f.TempFilePath);
                            if (dir is not null && tempFolders.Contains(dir) == false)
                            {
                                tempFolders.Add(dir);
                            }

                            try
                            {
                                File.Delete(f.TempFilePath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                            }
                        }
                    }
                }
            }
            else
            {
                if (resultContext == null)
                {
                    _logger.LogWarning("The result context is null.");
                    return;
                }

                if (resultContext.Result == null)
                {
                    _logger.LogWarning("The result context's result is null");
                    return;
                }

                if (!(resultContext.Result is ObjectResult objectResult))
                {
                    _logger.LogWarning("Could not acquire the result context's result as ObjectResult");
                    return;
                }

                if (objectResult.Value is IHaveUploadedFiles uploadedFiles)
                {
                    if (uploadedFiles.UploadedFiles != null)
                    {
                        //cleanup any files associated
                        foreach (ContentPropertyFile f in uploadedFiles.UploadedFiles)
                        {
                            if (f.TempFilePath.IsNullOrWhiteSpace() == false)
                            {
                                //track all temp folders so we can remove old files afterwards
                                var dir = Path.GetDirectoryName(f.TempFilePath);
                                if (dir is not null && tempFolders.Contains(dir) == false)
                                {
                                    tempFolders.Add(dir);
                                }

                                _logger.LogDebug("Removing temp file {FileName}", f.TempFilePath);

                                try
                                {
                                    File.Delete(f.TempFilePath);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Could not delete temp file {FileName}", f.TempFilePath);
                                }

                                //clear out the temp path so it's not returned in the response
                                f.TempFilePath = string.Empty;
                            }
                            else
                            {
                                _logger.LogWarning("The f.TempFilePath is null or whitespace!!??");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("The uploadedFiles.UploadedFiles is null!!??");
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "The actionExecutedContext.Request.Content.Value is not IHaveUploadedFiles, it is {ObjectType}",
                        objectResult.Value?.GetType());
                }
            }
        }
    }
}
