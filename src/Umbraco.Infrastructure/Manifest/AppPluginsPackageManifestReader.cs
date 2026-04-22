using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

/// <summary>
///     Reads package manifests from the <see cref="Constants.SystemDirectories.AppPlugins" /> directory.
/// </summary>
internal sealed class AppPluginsPackageManifestReader : PackageManifestReader
{
    private const string ExtensionsFolderName = "extensions";
    private const string JsExtension = ".js";

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

    protected override Task<PackageManifest?> ParsePackageManifestFromDirectoryAsync(IFileProvider fileProvider, IFileInfo directory, IFileInfo[] directoryContents)
    {
        IFileInfo? extensionDirectory = directoryContents
            .FirstOrDefault(f => f.IsDirectory && f.Name.InvariantEquals(ExtensionsFolderName));

        if (extensionDirectory is null)
        {
            return Task.FromResult<PackageManifest?>(null);
        }

        var packageName = directory.Name;
        var extensionsPath = WebPath.Combine(
            Constants.SystemDirectories.AppPlugins,
            packageName,
            extensionDirectory.Name);

        var bundles = fileProvider
            .GetDirectoryContents(extensionsPath)
            .Where(IsJsFile)
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Select(f => CreateBundleExtension(packageName, extensionsPath, f))
            .ToArray();

        return Task.FromResult(
            bundles.Length > 0
                ? new PackageManifest { Name = packageName, Extensions = bundles }
                : null);
    }

    private static bool IsJsFile(IFileInfo file) =>
        !file.IsDirectory && file.Name.EndsWith(JsExtension, StringComparison.OrdinalIgnoreCase);

    private static object CreateBundleExtension(string packageName, string extensionsPath, IFileInfo file)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
        return new
        {
            type = "bundle",
            alias = $"{packageName}.Extensions.Bundle.{fileNameWithoutExtension}",
            name = $"{packageName} Extensions Bundle ({fileNameWithoutExtension})",
            js = WebPath.Combine(extensionsPath, file.Name) + "?v=%CACHE_BUSTER%",
        };
    }}
