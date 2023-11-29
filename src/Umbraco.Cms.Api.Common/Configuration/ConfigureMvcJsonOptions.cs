using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Json;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureMvcJsonOptions : IConfigureOptions<MvcOptions>
{
    private readonly string _jsonOptionsName;
    private readonly IOptionsMonitor<JsonOptions> _jsonOptions;
    private readonly ILoggerFactory _loggerFactory;

    public ConfigureMvcJsonOptions(
        string jsonOptionsName,
        IOptionsMonitor<JsonOptions> jsonOptions,
        ILoggerFactory loggerFactory)
    {
        _jsonOptionsName = jsonOptionsName;
        _jsonOptions = jsonOptions;
        _loggerFactory = loggerFactory;
    }

    public void Configure(MvcOptions options)
    {
        JsonOptions jsonOptions = _jsonOptions.Get(_jsonOptionsName);
        ILogger<NamedSystemTextJsonInputFormatter> logger = _loggerFactory.CreateLogger<NamedSystemTextJsonInputFormatter>();
        options.InputFormatters.Insert(0, new NamedSystemTextJsonInputFormatter(_jsonOptionsName, jsonOptions, logger));
        options.OutputFormatters.Insert(0, new NamedSystemTextJsonOutputFormatter(_jsonOptionsName, jsonOptions.JsonSerializerOptions));
    }
}
