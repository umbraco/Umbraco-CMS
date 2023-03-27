using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiController]
[VersionedApiBackOfficeRoute("dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[ApiVersion("1.0")]
// TODO: Add authentication
public abstract class DictionaryControllerBase : ManagementApiControllerBase
{
}
