using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Configuration;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

public static class MvcBuilderExtensions
{
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
