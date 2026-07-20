using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Manifest;

/// <summary>
///     Reads package manifests from the <see cref="Constants.SystemDirectories.AppPlugins" /> directory.
/// </summary>
internal sealed class AppPluginsPackageManifestReader : PackageManifestReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Manifest.AppPluginsPackageManifestReader"/> class.
    /// </summary>
    /// <param name="packageManifestFileProviderFactory">A factory used to create instances of <see cref="IPackageManifestFileProviderFactory"/>, which provide access to package manifest files.</param>
    /// <param name="jsonSerializer">An <see cref="IJsonSerializer"/> used to deserialize JSON manifest files.</param>
    /// <param name="logger">An <see cref="ILogger{AppPluginsPackageManifestReader}"/> instance used for logging operations and errors.</param>
    public AppPluginsPackageManifestReader(
        IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<AppPluginsPackageManifestReader> logger)
        : base(
            Constants.SystemDirectories.AppPlugins,
            packageManifestFileProviderFactory,
            jsonSerializer,
            logger)
    {
    }
}
