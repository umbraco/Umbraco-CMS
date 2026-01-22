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
using Umbraco.Cms.Core.Hosting;
namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class ConfigurationServerController : ServerControllerBase
{
    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly IBackOfficeExternalLoginProviders _externalLoginProviders;
    private readonly IHostingEnvironment _hostingEnvironment;

    [ActivatorUtilitiesConstructor]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings, IBackOfficeExternalLoginProviders externalLoginProviders, IHostingEnvironment hostingEnvironment)
    {
        _securitySettings = securitySettings.Value;
        _globalSettings = globalSettings.Value;
        _externalLoginProviders = externalLoginProviders;
        _hostingEnvironment = hostingEnvironment;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings, IBackOfficeExternalLoginProviders externalLoginProviders)
        : this(securitySettings, globalSettings, externalLoginProviders, StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>())
    {
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
            UmbracoCssPath = _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoCssPath),
        };

        return Task.FromResult<IActionResult>(Ok(responseModel));
    }
}
