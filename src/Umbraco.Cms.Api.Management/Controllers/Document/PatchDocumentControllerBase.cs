using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public abstract class PatchDocumentControllerBase : UpdateDocumentControllerBase
{
    protected PatchDocumentControllerBase(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    /// <summary>
    /// Maps ContentPatchingOperationStatus to appropriate HTTP responses for PATCH operations.
    /// </summary>
    protected IActionResult ContentPatchingOperationStatusResult(ContentPatchingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentPatchingOperationStatus.InvalidOperation => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid operation")
                .WithDetail("One or more PATCH operations were invalid. Check operation structure, path syntax, and operation types.")
                .Build()),
            ContentPatchingOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown error")
                .WithDetail("An unexpected error occurred during the PATCH operation.")
                .Build()),
        });
}
