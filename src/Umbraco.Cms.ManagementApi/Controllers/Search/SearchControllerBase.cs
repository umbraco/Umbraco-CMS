using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Search;

[ApiController]
[VersionedApiBackOfficeRoute("search")]
[ApiExplorerSettings(GroupName = "Search")]
public class SearchControllerBase : ManagementApiControllerBase
{
}
