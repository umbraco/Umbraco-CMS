using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.BackOffice.Middleware;

/// <summary>
///     Ensures the Keep Alive middleware is part of
/// </summary>
public sealed class ConfigureGlobalOptionsForKeepAliveMiddlware : IPostConfigureOptions<GlobalSettings>
{
    private readonly IOptions<KeepAliveSettings> _keepAliveSettings;

    public ConfigureGlobalOptionsForKeepAliveMiddlware(IOptions<KeepAliveSettings> keepAliveSettings) =>
        _keepAliveSettings = keepAliveSettings;

    /// <summary>
    ///     Append the keep alive ping url to the reserved URLs
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    public void PostConfigure(string name, GlobalSettings options) =>
        options.ReservedUrls += _keepAliveSettings.Value.KeepAlivePingUrl;
}
