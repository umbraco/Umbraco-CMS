using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
[Authorize(Policy = AuthorizationPolicies.RequireAdminAccess)]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{
    protected IActionResult UpgradeOperationResult(UpgradeOperationStatus status, InstallationResult? result = null) =>
        status is UpgradeOperationStatus.Success
            ? Ok()
            : OperationStatusResult(status, problemDetailsBuilder => status switch
            {
                UpgradeOperationStatus.UpgradeFailed => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    problemDetailsBuilder
                        .WithTitle("Upgrade failed")
                        .WithDetail(result?.ErrorMessage ?? "An unknown error occurred.")
                        .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown upgrade operation status.")
                    .Build()),
            });
}
