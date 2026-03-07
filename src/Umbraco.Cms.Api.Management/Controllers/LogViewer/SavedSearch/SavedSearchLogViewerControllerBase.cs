using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

    /// <summary>
    /// Serves as the base controller for operations related to saved search log viewers in the management API.
    /// </summary>
[VersionedApiBackOfficeRoute("log-viewer/saved-search")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
public class SavedSearchLogViewerControllerBase : LogViewerControllerBase
{
    protected IActionResult SavedSearchNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The saved search could not be found")
        .Build());
}
