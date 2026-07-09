using Microsoft.Extensions.Options;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Common.Configuration;

/// <summary>
///     Configures OpenIddict server options for Umbraco authentication.
/// </summary>
/// <remarks>
///     Disables transport security requirement when HTTPS is not configured in global settings.
///     Warning: This should only be used in development environments.
/// </remarks>
internal sealed class ConfigureOpenIddict : IConfigureOptions<OpenIddictServerAspNetCoreOptions>
{
    private readonly IOptions<GlobalSettings> _globalSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureOpenIddict"/> class.
    /// </summary>
    /// <param name="globalSettings">The global settings options.</param>
    public ConfigureOpenIddict(IOptions<GlobalSettings> globalSettings) => _globalSettings = globalSettings;

    /// <inheritdoc/>
    public void Configure(OpenIddictServerAspNetCoreOptions options)
        => options.DisableTransportSecurityRequirement = _globalSettings.Value.UseHttps is false;
}
