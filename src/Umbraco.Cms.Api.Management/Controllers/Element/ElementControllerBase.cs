using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Element)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public class ElementControllerBase : ContentControllerBase
{
    protected IActionResult ElementEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<ElementValueModel, ElementVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, ElementValueModel, ElementVariantRequestModel>(status, requestModel, validationResult);
}
