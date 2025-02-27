using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Manifest;

/// <summary>
///     Reads package manifests from the <see cref="Constants.SystemDirectories.BackOfficePath" /> directory.
/// </summary>
internal sealed class BackOfficePackageManifestReader : PackageManifestReader
{
    public BackOfficePackageManifestReader(
        IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<BackOfficePackageManifestReader> logger)
        : base(
            Constants.SystemDirectories.BackOfficePath,
            packageManifestFileProviderFactory,
            jsonSerializer,
            logger)
    {
    }
}
