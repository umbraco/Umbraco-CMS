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
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Manifest.BackOfficePackageManifestReader"/> class.
    /// </summary>
    /// <param name="packageManifestFileProviderFactory">A factory used to create providers for accessing package manifest files.</param>
    /// <param name="jsonSerializer">The serializer used to deserialize JSON manifest files.</param>
    /// <param name="logger">The logger used for recording diagnostic and operational information.</param>
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
