using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Middleware;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Imaging;

namespace Umbraco.Cms.Web.Common.DependencyInjection;

/// <summary>
///     Configures the ImageSharp middleware options.
/// </summary>
/// <seealso cref="IConfigureOptions{ImageSharpMiddlewareOptions}" />
public sealed class ConfigureImageSharpMiddlewareOptions : IConfigureOptions<ImageSharpMiddlewareOptions>
{
    private readonly Configuration _configuration;
    private readonly IAdditionalImagingOptions _additionalImagingOptions;
    private readonly ImagingSettings _imagingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="additionalImageOptions">Additional options for context related methods.</param>
    public ConfigureImageSharpMiddlewareOptions(
        Configuration configuration,
        IOptions<ImagingSettings> imagingSettings,
        IAdditionalImagingOptions additionalImageOptions)
    {
        _configuration = configuration;
        _imagingSettings = imagingSettings.Value;
        _additionalImagingOptions = additionalImageOptions;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    [Obsolete("Use ctor with all params")]
    public ConfigureImageSharpMiddlewareOptions(
        Configuration configuration,
        IOptions<ImagingSettings> imagingSettings)
    {
        _configuration = configuration;
        _imagingSettings = imagingSettings.Value;
        _additionalImagingOptions = StaticServiceProvider.Instance.GetRequiredService<IAdditionalImagingOptions>();
    }

    /// <inheritdoc />
    public void Configure(ImageSharpMiddlewareOptions options)
    {
        options.Configuration = _configuration;

        options.BrowserMaxAge = _imagingSettings.Cache.BrowserMaxAge;
        options.CacheMaxAge = _imagingSettings.Cache.CacheMaxAge;
        options.CacheHashLength = _imagingSettings.Cache.CacheHashLength;

        options.HMACSecretKey = _imagingSettings.HMACSecretKey.Length != 0 ? _imagingSettings.HMACSecretKey : options.HMACSecretKey;
        options.UseInvariantParsingCulture = _imagingSettings.UseInvariantParsingCulture.GetValueOrDefault(options.UseInvariantParsingCulture);

        options.MemoryStreamManager = _additionalImagingOptions.MemoryStreamManager(options);
        options.OnComputeHMACAsync = (context, hmac) => _additionalImagingOptions.OnComputeHMACAsync(options, context, hmac);
        options.OnParseCommandsAsync = context => _additionalImagingOptions.OnParseCommandsAsync(options, context);
        options.OnBeforeSaveAsync = image => _additionalImagingOptions.OnBeforeSaveAsync(options, image);
        options.OnProcessedAsync = context => _additionalImagingOptions.OnProcessedAsync(options, context);
        options.OnPrepareResponseAsync = context => _additionalImagingOptions.OnPrepareResponseAsync(options, context);
    }
}
