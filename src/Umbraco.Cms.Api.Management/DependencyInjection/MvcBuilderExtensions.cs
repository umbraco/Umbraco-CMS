﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Configuration;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddJsonOptions(this IMvcBuilder builder, string settingsName, Action<JsonOptions> configure)
    {
        builder.Services.Configure(settingsName, configure);
        builder.Services.AddSingleton<IConfigureOptions<MvcOptions>>(sp =>
        {
            IOptionsMonitor<JsonOptions> options = sp.GetRequiredService<IOptionsMonitor<JsonOptions>>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new ConfigureMvcJsonOptions(settingsName, options, loggerFactory);
        });
        return builder;
    }
}
