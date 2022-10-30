using Microsoft.AspNetCore.Mvc;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

[ApiController]
[VersionedApiBackOfficeRoute("language")]
[ApiExplorerSettings(GroupName = "Language")]
[ApiVersion("1.0")]
public abstract class LanguageControllerBase : ManagementApiControllerBase
{
}
