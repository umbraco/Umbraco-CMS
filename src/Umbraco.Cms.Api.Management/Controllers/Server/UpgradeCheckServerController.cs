using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.RequireAdminAccess)]
public class UpgradeCheckServerController : ServerControllerBase
{
    private readonly IUpgradeService _upgradeService;
    private readonly IUmbracoVersion _umbracoVersion;

    public UpgradeCheckServerController(IUpgradeService upgradeService, IUmbracoVersion umbracoVersion)
    {
        _upgradeService = upgradeService;
        _umbracoVersion = umbracoVersion;
    }

    [HttpGet("upgrade-check")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UpgradeCheckResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpgradeCheck(CancellationToken cancellationToken)
    {
        UpgradeResult upgradeResult = await _upgradeService.CheckUpgrade(_umbracoVersion.SemanticVersion);

        var responseModel = new UpgradeCheckResponseModel
        {
            Type = upgradeResult.UpgradeType,
            Comment = upgradeResult.Comment,
            Url = upgradeResult.UpgradeUrl.IsNullOrWhiteSpace()
                ? string.Empty
                : $"{upgradeResult.UpgradeUrl}?version={_umbracoVersion.Version.ToString(3)}"
        };

        return Ok(responseModel);
    }
}
