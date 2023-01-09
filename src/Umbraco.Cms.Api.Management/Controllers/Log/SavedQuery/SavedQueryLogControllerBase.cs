using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.LogViewer;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.Log.SavedQuery;

[ApiController]
[VersionedApiBackOfficeRoute("log/saved-query")]
[ApiExplorerSettings(GroupName = "Log")]
[ApiVersion("1.0")]
public class SavedQueryLogControllerBase : LogControllerBase
{
    public SavedQueryLogControllerBase(ILogViewer logViewer)
        : base(logViewer)
    {
    }
}
