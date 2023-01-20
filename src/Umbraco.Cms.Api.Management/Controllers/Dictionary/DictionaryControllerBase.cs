﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

[ApiController]
[VersionedApiBackOfficeRoute("dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[ApiVersion("1.0")]
// TODO: Add authentication
public abstract class DictionaryControllerBase : ManagementApiControllerBase
{
    protected IActionResult DictionaryItemOperationStatusResult(DictionaryItemOperationStatus status) =>
        status switch
        {
            DictionaryItemOperationStatus.DuplicateItemKey => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Duplicate dictionary item name detected")
                .WithDetail("Another dictionary item exists with the same name. Dictionary item names must be unique.")
                .Build()),
            DictionaryItemOperationStatus.ItemNotFound => NotFound("The dictionary item could not be found"),
            DictionaryItemOperationStatus.ParentNotFound => NotFound("The dictionary item parent could not be found"),
            DictionaryItemOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the dictionary item operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown dictionary operation status")
        };
}
