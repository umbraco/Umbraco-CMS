using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[VersionedApiBackOfficeRoute("preview")]
[ApiExplorerSettings(GroupName = "Preview")]
public class PreviewControllerBase : ManagementApiControllerBase
{
}
