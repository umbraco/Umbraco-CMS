using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

/// <summary>
///     Reads package manifests by scanning for JavaScript files in <c>App_Plugins/{PackageName}/extensions/</c> folders.
///     Each discovered JS file is registered as a <c>bundle</c> extension, which the frontend bundle initializer
///     loads and inspects for <c>@umbExtension</c> decorator metadata.
/// </summary>
internal sealed class AppPluginsExtensionsFolderPackageManifestReader : IPackageManifestReader
{
    private const string ExtensionsFolderName = "extensions";

    private readonly IPackageManifestFileProviderFactory _packageManifestFileProviderFactory;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly ILogger<AppPluginsExtensionsFolderPackageManifestReader> _logger;

    public AppPluginsExtensionsFolderPackageManifestReader(
        IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
        IUmbracoVersion umbracoVersion,
        ILogger<AppPluginsExtensionsFolderPackageManifestReader> logger)
    {
        _packageManifestFileProviderFactory = packageManifestFileProviderFactory;
        _umbracoVersion = umbracoVersion;
        _logger = logger;
    }

    public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        IFileProvider? fileProvider = _packageManifestFileProviderFactory.Create();
        if (fileProvider is null)
        {
            return Task.FromResult(Enumerable.Empty<PackageManifest>());
        }

        var manifests = new List<PackageManifest>();

        // Scan each package folder in App_Plugins
        foreach (IFileInfo packageFolder in fileProvider.GetDirectoryContents(Constants.SystemDirectories.AppPlugins))
        {
            if (!packageFolder.IsDirectory)
            {
                continue;
            }

            var extensionsPath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageFolder.Name, ExtensionsFolderName);
            IDirectoryContents extensionsContents = fileProvider.GetDirectoryContents(extensionsPath);

            if (!extensionsContents.Exists)
            {
                continue;
            }

            var bundleExtensions = new List<object>();
            var bundleIndex = 0;

            foreach (IFileInfo file in extensionsContents)
            {
                if (file.IsDirectory || !file.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var jsPath = WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageFolder.Name, ExtensionsFolderName, file.Name)
                    + "?v=%CACHE_BUSTER%";

                bundleExtensions.Add(new
                {
                    type = "bundle",
                    alias = $"{packageFolder.Name}.Extensions.Bundle.{bundleIndex}",
                    name = $"{packageFolder.Name} Extensions Bundle {bundleIndex}",
                    js = jsPath,
                });

                bundleIndex++;
            }

            if (bundleExtensions.Count > 0)
            {
                _logger.LogDebug(
                    "Discovered {Count} extension file(s) in {Path}",
                    bundleExtensions.Count,
                    extensionsPath);

                manifests.Add(new PackageManifest
                {
                    Name = packageFolder.Name,
                    Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                    Extensions = bundleExtensions.ToArray(),
                });
            }
        }

        return Task.FromResult<IEnumerable<PackageManifest>>(manifests);
    }
}
