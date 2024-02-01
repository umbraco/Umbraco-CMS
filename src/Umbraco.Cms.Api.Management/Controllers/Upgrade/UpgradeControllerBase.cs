using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
[Authorize(Policy = "New" + AuthorizationPolicies.RequireAdminAccess)]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{
    protected IActionResult UpgradeOperationResult(UpgradeOperationStatus status, InstallationResult? result = null) =>
        status switch
        {
            UpgradeOperationStatus.Success => Ok(),
            UpgradeOperationStatus.UpgradeFailed => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Upgrade failed")
                .WithDetail(result?.ErrorMessage ?? "An unknown error occurred.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown upgrade operation status.")
                .Build()),
        };
}
