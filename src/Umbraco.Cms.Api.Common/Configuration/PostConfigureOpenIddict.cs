using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Common.Configuration;

internal class PostConfigureOpenIddict : IPostConfigureOptions<OpenIddictServerOptions>
{
    private readonly IOptions<GlobalSettings> _globalSettings;

    public PostConfigureOpenIddict(IOptions<GlobalSettings> globalSettings)
    {
        _globalSettings = globalSettings;
    }

    public void PostConfigure(string? name, OpenIddictServerOptions options)
    {
        EnsureHttpsIsNotRequiredWhenConfigAllowHttp(options);
    }

    /// <summary>
    /// Ensures OpenIddict is configured to allow Http requrest, if and only if, the global settings are configured to allow Http.
    /// </summary>
    /// <remarks>
    /// The logic actually allowing http by removing the ValidateTransportSecurityRequirement Descriptor is borrowed from <see cref="OpenIddictServerBuilder.RemoveEventHandler"/>
    /// </remarks>
    private void EnsureHttpsIsNotRequiredWhenConfigAllowHttp(OpenIddictServerOptions options)
    {
        if (_globalSettings.Value.UseHttps is false)
        {
            OpenIddictServerHandlerDescriptor descriptor = OpenIddictServerAspNetCoreHandlers.ValidateTransportSecurityRequirement.Descriptor;

            for (var index = options.Handlers.Count - 1; index >= 0; index--)
            {
                if (options.Handlers[index].ServiceDescriptor.ServiceType == descriptor.ServiceDescriptor.ServiceType)
                {
                    options.Handlers.RemoveAt(index);
                }
            }
        }
    }
}
