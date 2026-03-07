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

/// <summary>
/// API controller that provides endpoints for retrieving and managing server configuration settings.
/// </summary>
[ApiVersion("1.0")]
public class ConfigurationServerController : ServerControllerBase
{
    private readonly SecuritySettings _securitySettings;
    private readonly GlobalSettings _globalSettings;
    private readonly IBackOfficeExternalLoginProviders _externalLoginProviders;
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationServerController"/> class.
    /// </summary>
    /// <param name="securitySettings">The security settings options.</param>
    /// <param name="globalSettings">The global settings options.</param>
    /// <param name="externalLoginProviders">The external login providers for back office.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    [ActivatorUtilitiesConstructor]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings, IBackOfficeExternalLoginProviders externalLoginProviders, IHostingEnvironment hostingEnvironment)
    {
        _securitySettings = securitySettings.Value;
        _globalSettings = globalSettings.Value;
        _externalLoginProviders = externalLoginProviders;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Server.ConfigurationServerController"/> class.
    /// </summary>
    /// <param name="securitySettings">The <see cref="SecuritySettings"/> options.</param>
    /// <param name="globalSettings">The <see cref="GlobalSettings"/> options.</param>
    /// <param name="externalLoginProviders">The external login providers used for back office authentication.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ConfigurationServerController(IOptions<SecuritySettings> securitySettings, IOptions<GlobalSettings> globalSettings, IBackOfficeExternalLoginProviders externalLoginProviders)
        : this(securitySettings, globalSettings, externalLoginProviders, StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>())
    {
    }

    /// <summary>
    /// Retrieves the current server configuration settings.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ServerConfigurationResponseModel"/> with the server's configuration settings, such as password reset allowance, version check period, local login permission, and CSS path.
    /// </returns>
    [AllowAnonymous]
    [HttpGet("configuration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerConfigurationResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the server configuration.")]
    [EndpointDescription("Gets the configuration settings for servers.")]
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
