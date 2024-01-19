using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Media)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMedia)]
public class MediaControllerBase : ContentControllerBase
{
    protected IActionResult MediaNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The requested Media could not be found")
        .Build());

    protected IActionResult MediaEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        IEnumerable<PropertyValidationError> validationErrors)
        where TContentModelBase : ContentModelBase<MediaValueModel, MediaVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, MediaValueModel, MediaVariantRequestModel>(status, requestModel, validationErrors);
}
