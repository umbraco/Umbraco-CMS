using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal sealed class BackOfficePackageManifestReader(
    IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
    IJsonSerializer jsonSerializer,
    ILogger<BackOfficePackageManifestReader> logger)
    : PackageManifestReader(Constants.SystemDirectories.BackOfficePath, packageManifestFileProviderFactory,
        jsonSerializer, logger);
