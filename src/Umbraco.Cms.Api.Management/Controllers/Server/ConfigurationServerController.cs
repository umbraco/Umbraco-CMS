using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class ConfigurationServerController : ServerControllerBase
{
    private readonly SecuritySettings _securitySettings;

    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings) => _securitySettings = securitySettings.Value;

    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var responseModel = new ServerConfigurationResponseModel
        {
            AllowPasswordReset = _securitySettings.AllowPasswordReset,
        };

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
