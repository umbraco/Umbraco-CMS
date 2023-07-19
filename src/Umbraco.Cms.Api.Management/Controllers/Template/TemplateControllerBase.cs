using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Template;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Template)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Template))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessTemplates)]
public class TemplateControllerBase : ManagementApiControllerBase
{
    protected IActionResult TemplateOperationStatusResult(TemplateOperationStatus status) =>
        status switch
        {
            TemplateOperationStatus.TemplateNotFound => NotFound("The template could not be found"),
            TemplateOperationStatus.InvalidAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid alias")
                .WithDetail("The template alias is not valid.")
                .Build()),
            TemplateOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the template operation.")
                .Build()),
            TemplateOperationStatus.DuplicateAlias => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate alias")
                .WithDetail("A template with that alias already exists.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown template operation status")
        };
}
