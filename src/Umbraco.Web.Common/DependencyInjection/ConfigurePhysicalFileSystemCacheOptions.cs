using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Web.Common.DependencyInjection;

/// <summary>
///     Configures the ImageSharp physical file system cache options.
/// </summary>
/// <seealso cref="IConfigureOptions{PhysicalFileSystemCacheOptions}" />
public sealed class ConfigurePhysicalFileSystemCacheOptions : IConfigureOptions<PhysicalFileSystemCacheOptions>
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ImagingSettings _imagingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurePhysicalFileSystemCacheOptions" /> class.
    /// </summary>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    public ConfigurePhysicalFileSystemCacheOptions(
        IOptions<ImagingSettings> imagingSettings,
        IHostEnvironment hostEnvironment)
    {
        _imagingSettings = imagingSettings.Value;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public void Configure(PhysicalFileSystemCacheOptions options)
    {
        options.CacheFolder = _hostEnvironment.MapPathContentRoot(_imagingSettings.Cache.CacheFolder);
        options.CacheFolderDepth = _imagingSettings.Cache.CacheFolderDepth;
    }
}
