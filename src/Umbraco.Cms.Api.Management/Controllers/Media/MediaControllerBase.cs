using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Media)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
public class MediaControllerBase : ContentControllerBase
{
    protected IActionResult MediaNotFound()
        => OperationStatusResult(ContentEditingOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The requested Media could not be found")
                .Build()));

    protected IActionResult MediaEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<MediaValueModel, MediaVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, MediaValueModel, MediaVariantRequestModel>(status, requestModel, validationResult);
}
