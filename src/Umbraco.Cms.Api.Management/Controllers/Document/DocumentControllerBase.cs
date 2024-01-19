using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing.Validation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Document)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentControllerBase : ContentControllerBase
{
    protected IActionResult DocumentNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The requested Document could not be found")
        .Build());

    protected IActionResult DocumentEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        IEnumerable<PropertyValidationError> validationErrors)
        where TContentModelBase : ContentModelBase<DocumentValueModel, DocumentVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, DocumentValueModel, DocumentVariantRequestModel>(status, requestModel, validationErrors);
}
