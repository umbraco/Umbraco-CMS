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
