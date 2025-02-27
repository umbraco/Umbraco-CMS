using Microsoft.Extensions.Options;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Common.Configuration;

internal class ConfigureOpenIddict : IConfigureOptions<OpenIddictServerAspNetCoreOptions>
{
    private readonly IOptions<GlobalSettings> _globalSettings;

    public ConfigureOpenIddict(IOptions<GlobalSettings> globalSettings) => _globalSettings = globalSettings;

    public void Configure(OpenIddictServerAspNetCoreOptions options)
        => options.DisableTransportSecurityRequirement = _globalSettings.Value.UseHttps is false;
}
