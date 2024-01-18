using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.RecycleBin;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.RecycleBin}/{Constants.UdiEntityType.Document}")]
[RequireDocumentTreeRootAccess]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocuments)]
public class DocumentRecycleBinControllerBase : RecycleBinControllerBase<RecycleBinItemResponseModel>
{
    public DocumentRecycleBinControllerBase(IEntityService entityService)
        : base(entityService)
    {
    }

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Document;

    protected override int RecycleBinRootId => Constants.System.RecycleBinContent;

    protected override RecycleBinItemResponseModel MapRecycleBinViewModel(Guid? parentId, IEntitySlim entity)
    {
        RecycleBinItemResponseModel responseModel = base.MapRecycleBinViewModel(parentId, entity);

        if (entity is IDocumentEntitySlim documentEntitySlim)
        {
            responseModel.Icon = documentEntitySlim.ContentTypeIcon ?? responseModel.Icon;
        }

        return responseModel;
    }

    protected IActionResult ContentEditingOperationStatusResult(ContentEditingOperationStatus status) =>
        status switch
        {
            ContentEditingOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the content operation.")
                .Build()),
            ContentEditingOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The content could not be found")
                .Build()),
            ContentEditingOperationStatus.NotAllowed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Operation not permitted")
                .WithDetail("The attempted operation was not permitted, likely due to a permission/configuration mismatch with the operation.")
                .Build()),
            ContentEditingOperationStatus.NotInTrash => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content is not in the recycle bin")
                .WithDetail("The attempted operation requires the targeted content to be in the recycle bin.")
                .Build()),
            ContentEditingOperationStatus.Unknown => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown error. Please see the log for more details.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown content operation status.")
                .Build()),
        };
}
