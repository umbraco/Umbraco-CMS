using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Security;

public class ConfigureFormOptions : IConfigureOptions<FormOptions>
{
    private readonly IOptions<RuntimeSettings> _runtimeSettings;

    public ConfigureFormOptions(IOptions<RuntimeSettings> runtimeSettings) => _runtimeSettings = runtimeSettings;

    public void Configure(FormOptions options) =>

        // convert from KB to bytes
        options.MultipartBodyLengthLimit = _runtimeSettings.Value.MaxRequestLength.HasValue
            ? _runtimeSettings.Value.MaxRequestLength.Value * 1024
            : long.MaxValue;
}
