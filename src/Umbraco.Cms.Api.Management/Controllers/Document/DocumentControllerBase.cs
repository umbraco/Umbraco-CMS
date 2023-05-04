using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Document)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
public abstract class DocumentControllerBase : ContentControllerBase
{
    protected IActionResult DocumentNotFound() => NotFound("The requested Document could not be found");
}
