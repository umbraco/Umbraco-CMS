using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

[VersionedApiBackOfficeRoute("dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDictionary)]
public abstract class DictionaryControllerBase : ManagementApiControllerBase
{
    protected IActionResult DictionaryItemOperationStatusResult(DictionaryItemOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            DictionaryItemOperationStatus.DuplicateItemKey => Conflict(problemDetailsBuilder
                .WithTitle("Duplicate dictionary item name detected")
                .WithDetail("Another dictionary item exists with the same name. Dictionary item names must be unique.")
                .Build()),
            DictionaryItemOperationStatus.ItemNotFound => DictionaryItemNotFound(problemDetailsBuilder),
            DictionaryItemOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The dictionary item parent could not be found")
                .Build()),
            DictionaryItemOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the dictionary item operation.")
                .Build()),
            DictionaryItemOperationStatus.InvalidParent => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid parent")
                .WithDetail("The targeted parent dictionary item is not valid for this dictionary item operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown dictionary operation status.")
                .Build()),
        });

    protected IActionResult DictionaryItemNotFound() => OperationStatusResult(DictionaryItemOperationStatus.ItemNotFound, DictionaryItemNotFound);

    private IActionResult DictionaryItemNotFound(ProblemDetailsBuilder problemDetailsBuilder)
        => NotFound(problemDetailsBuilder
            .WithTitle("The dictionary item could not be found")
            .Build());
}
