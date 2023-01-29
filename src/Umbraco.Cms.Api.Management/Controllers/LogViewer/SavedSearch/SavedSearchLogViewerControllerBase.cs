using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

[ApiController]
[VersionedApiBackOfficeRoute("log-viewer/saved-search")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
[ApiVersion("1.0")]
public class SavedSearchLogViewerControllerBase : LogViewerControllerBase
{
}
