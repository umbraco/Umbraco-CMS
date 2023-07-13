using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

[ApiController]
[VersionedApiBackOfficeRoute("language")]
[ApiExplorerSettings(GroupName = "Language")]
public abstract class LanguageControllerBase : ManagementApiControllerBase
{
    protected IActionResult LanguageOperationStatusResult(LanguageOperationStatus status) =>
        status switch
        {
            LanguageOperationStatus.InvalidFallback => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid fallback language")
                .WithDetail("The fallback language could not be applied. This may be caused if the fallback language causes cyclic fallbacks.")
                .Build()),
            LanguageOperationStatus.NotFound => NotFound("The language could not be found"),
            LanguageOperationStatus.MissingDefault => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("No default language")
                .WithDetail("The attempted operation would result in having no default language defined. This is not allowed.")
                .Build()),
            LanguageOperationStatus.DuplicateIsoCode => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate ISO code")
                .WithDetail("Another language already exists with the attempted ISO code.")
                .Build()),
            LanguageOperationStatus.InvalidIsoCode => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid ISO code")
                .WithDetail("The attempted ISO code does not represent a valid culture.")
                .Build()),
            LanguageOperationStatus.InvalidFallbackIsoCode => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid Fallback ISO code")
                .WithDetail("The attempted fallback ISO code does not represent a valid culture.")
                .Build()),
            LanguageOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the language operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown language operation status")
        };
}
