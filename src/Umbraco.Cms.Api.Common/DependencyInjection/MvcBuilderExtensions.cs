using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     Extension methods for <see cref="IMvcBuilder"/>.
/// </summary>
public static class MvcBuilderExtensions
{
    /// <summary>
    ///     Adds named JSON serialization options to the MVC builder.
    /// </summary>
    /// <param name="builder">The MVC builder.</param>
    /// <param name="settingsName">The name for the JSON options configuration.</param>
    /// <param name="configure">The action to configure the JSON options.</param>
    /// <returns>The MVC builder for method chaining.</returns>
    public static IMvcBuilder AddJsonOptions(this IMvcBuilder builder, string settingsName, Action<JsonOptions> configure)
    {
        builder.Services.Configure(settingsName, configure);
        builder.Services.AddSingleton<IConfigureOptions<MvcOptions>>(provider =>
        {
            IOptionsMonitor<JsonOptions> options = provider.GetRequiredService<IOptionsMonitor<JsonOptions>>();
            ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new ConfigureMvcJsonOptions(settingsName, options, loggerFactory);
        });
        return builder;
    }
}
