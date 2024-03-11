using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Language)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Language))]
public abstract class LanguageControllerBase : ManagementApiControllerBase
{
    protected IActionResult LanguageOperationStatusResult(LanguageOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            LanguageOperationStatus.InvalidFallback => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid fallback language")
                .WithDetail("The fallback language could not be applied. This may be caused if the fallback language causes cyclic fallbacks.")
                .Build()),
            LanguageOperationStatus.NotFound => LanguageNotFound(problemDetailsBuilder),
            LanguageOperationStatus.MissingDefault => BadRequest(problemDetailsBuilder
                .WithTitle("No default language")
                .WithDetail("The attempted operation would result in having no default language defined. This is not allowed.")
                .Build()),
            LanguageOperationStatus.DuplicateIsoCode => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate ISO code")
                .WithDetail("Another language already exists with the attempted ISO code.")
                .Build()),
            LanguageOperationStatus.InvalidIsoCode => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid ISO code")
                .WithDetail("The attempted ISO code does not represent a valid culture.")
                .Build()),
            LanguageOperationStatus.InvalidFallbackIsoCode => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Fallback ISO code")
                .WithDetail("The attempted fallback ISO code does not represent a valid culture.")
                .Build()),
            LanguageOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the language operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown language operation status.")
                .Build()),
        });

    protected IActionResult LanguageNotFound() => OperationStatusResult(LanguageOperationStatus.NotFound, LanguageNotFound);

    private IActionResult LanguageNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The language could not be found")
        .Build());
}
