using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Document.Collection;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Collection}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMedia)]
public abstract class MediaCollectionControllerBase : ManagementApiControllerBase
{
    protected IActionResult CollectionOperationStatusResult(ContentCollectionOperationStatus status)
        => DocumentCollectionControllerBase.ContentCollectionOperationStatusResult(status, "media");
}
