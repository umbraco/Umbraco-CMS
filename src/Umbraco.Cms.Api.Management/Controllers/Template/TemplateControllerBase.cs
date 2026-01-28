using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Template)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessTemplates)]
public class TemplateControllerBase : ManagementApiControllerBase
{
    protected IActionResult TemplateOperationStatusResult(TemplateOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            TemplateOperationStatus.TemplateNotFound => TemplateNotFound(),
            TemplateOperationStatus.InvalidAlias => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid alias")
                .WithDetail("The template alias is not valid.")
                .Build()),
            TemplateOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the template operation.")
                .Build()),
            TemplateOperationStatus.DuplicateAlias => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate alias")
                .WithDetail("A template with that alias already exists.")
                .Build()),
            TemplateOperationStatus.CircularLayoutReference => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid layout")
                .WithDetail("The layout referenced in the template leads to a circular reference.")
                .Build()),
            TemplateOperationStatus.LayoutNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Layout not found")
                .WithDetail("The layout referenced in the template was not found.")
                .Build()),
            TemplateOperationStatus.LayoutCannotBeDeleted => BadRequest(problemDetailsBuilder
                .WithTitle("Layout cannot be deleted")
                .WithDetail("The layout cannot be deleted. Please ensure the template is not used as a layout before you delete.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown template operation status.")
                .Build()),
        });

    protected IActionResult TemplateNotFound()
        => OperationStatusResult(TemplateOperationStatus.TemplateNotFound, TemplateNotFound);

    protected IActionResult TemplateNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The template could not be found")
        .Build());
}
