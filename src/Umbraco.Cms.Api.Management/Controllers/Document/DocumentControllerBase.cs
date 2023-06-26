using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Document)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
public abstract class DocumentControllerBase : ContentControllerBase
{
    protected IActionResult DocumentNotFound() => NotFound("The requested Document could not be found");

    protected IActionResult ContentTypeOperationStatusResult(ContentTypeOperationStatus contentTypeOperationStatus) =>
        contentTypeOperationStatus switch
        {
            ContentTypeOperationStatus.NotFound => NotFound("The document type with the given key was not found"),
        };
}
