using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileItemControllerBase : ManagementApiControllerBase
{
}
