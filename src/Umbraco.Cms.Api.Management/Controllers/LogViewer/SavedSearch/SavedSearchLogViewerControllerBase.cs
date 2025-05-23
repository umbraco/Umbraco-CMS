using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

[VersionedApiBackOfficeRoute("log-viewer/saved-search")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
public class SavedSearchLogViewerControllerBase : LogViewerControllerBase
{
    protected IActionResult SavedSearchNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The saved search could not be found")
        .Build());
}
