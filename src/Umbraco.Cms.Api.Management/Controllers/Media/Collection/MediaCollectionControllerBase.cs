using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Collection}/{Constants.UdiEntityType.Media}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
public abstract class MediaCollectionControllerBase : ContentCollectionControllerBase<IMedia, MediaCollectionResponseModel, MediaValueResponseModel, MediaVariantResponseModel>
{
    protected MediaCollectionControllerBase(IUmbracoMapper mapper)
        : base(mapper)
    {
    }

    protected IActionResult CollectionOperationStatusResult(ContentCollectionOperationStatus status)
        => ContentCollectionOperationStatusResult(status, "media");
}
