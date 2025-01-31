using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class ConfigurationServerController : ServerControllerBase
{
    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly IBackOfficeExternalLoginProviders _externalLoginProviders;

    [Obsolete("Use the constructor that accepts all arguments. Will be removed in V16.")]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings)
        : this(securitySettings, StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>())
    {
    }

    [Obsolete("Use the constructor that accepts all arguments. Will be removed in V16.")]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings)
        : this(securitySettings, globalSettings, StaticServiceProvider.Instance.GetRequiredService<IBackOfficeExternalLoginProviders>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings, IBackOfficeExternalLoginProviders externalLoginProviders)
    {
        _securitySettings = securitySettings.Value;
        _globalSettings = globalSettings.Value;
        _externalLoginProviders = externalLoginProviders;
    }

    [AllowAnonymous]
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerConfigurationResponseModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Configuration(CancellationToken cancellationToken)
    {
        var responseModel = new ServerConfigurationResponseModel
        {
            AllowPasswordReset = _securitySettings.AllowPasswordReset,
            VersionCheckPeriod = _globalSettings.VersionCheckPeriod,
            AllowLocalLogin = _externalLoginProviders.HasDenyLocalLogin() is false,
        };

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
