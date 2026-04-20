using System.Text;
using System.Text.Json;
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
///     Reads package manifests from <c>App_Plugins/</c>. Walks the directory once and, for each
///     package folder, yields manifests from whichever of the two supported sources are present:
///     <list type="bullet">
///         <item><description>An <c>umbraco-package.json</c> file (parsed verbatim).</description></item>
///         <item>
///             <description>
///                 An <c>extensions/</c> folder — each <c>.js</c> file directly inside becomes a
///                 <c>bundle</c> extension that the frontend bundle initializer loads and inspects
///                 for <c>@umbExtension</c> decorator metadata. Subdirectories are not traversed.
///             </description>
///         </item>
///     </list>
///     Both sources may coexist for the same package — they are additive. Auto-synthesized bundle
///     aliases follow the pattern <c>{package}.Extensions.Bundle.{filename}</c> so they do not
///     collide with hand-authored manifest entries.
/// </summary>
internal sealed class AppPluginsPackageManifestReader : IPackageManifestReader
{
    private const string PackageManifestFileName = "umbraco-package.json";
    private const string ExtensionsFolderName = "extensions";
    private const string JsExtension = ".js";

    private readonly IPackageManifestFileProviderFactory _fileProviderFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<AppPluginsPackageManifestReader> _logger;

    public AppPluginsPackageManifestReader(
        IPackageManifestFileProviderFactory fileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<AppPluginsPackageManifestReader> logger)
    {
        _fileProviderFactory = fileProviderFactory;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        IFileProvider? fileProvider = _fileProviderFactory.Create();
        if (fileProvider is null)
        {
            return [];
        }

        var manifests = new List<PackageManifest>();
        foreach (IFileInfo packageFolder in EnumeratePackageFolders(fileProvider))
        {
            manifests.AddRange(await ReadManifestsForPackageAsync(fileProvider, packageFolder.Name));
        }

        return manifests;
    }

    private static IEnumerable<IFileInfo> EnumeratePackageFolders(IFileProvider fileProvider) =>
        fileProvider
            .GetDirectoryContents(Constants.SystemDirectories.AppPlugins)
            .Where(f => f.IsDirectory);

    private async Task<IEnumerable<PackageManifest>> ReadManifestsForPackageAsync(
        IFileProvider fileProvider,
        string packageName)
    {
        // Materialize the directory contents so each source can inspect it without re-enumerating
        // the underlying provider (which some implementations don't support).
        IFileInfo[] packageContents = fileProvider
            .GetDirectoryContents(WebPath.Combine(Constants.SystemDirectories.AppPlugins, packageName))
            .ToArray();

        var manifests = new List<PackageManifest>();

        PackageManifest? fromJson = await TryReadJsonManifestAsync(packageContents);
        if (fromJson is not null)
        {
            manifests.Add(fromJson);
        }

        PackageManifest? fromFolder = TryBuildExtensionsFolderManifest(fileProvider, packageName, packageContents);
        if (fromFolder is not null)
        {
            manifests.Add(fromFolder);
        }

        return manifests;
    }

    private Task<PackageManifest?> TryReadJsonManifestAsync(IReadOnlyList<IFileInfo> packageContents)
    {
        IFileInfo? manifestFile = packageContents
            .FirstOrDefault(f => !f.IsDirectory && f.Name.InvariantEquals(PackageManifestFileName));

        return manifestFile is null
            ? Task.FromResult<PackageManifest?>(null)
            : ParseManifestFileAsync(manifestFile);
    }

    private async Task<PackageManifest?> ParseManifestFileAsync(IFileInfo fileInfo)
    {
        string content = await ReadAllTextAsync(fileInfo);
        if (content.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            return _jsonSerializer.Deserialize<PackageManifest>(content);
        }
        catch (Exception ex)
        {
            string reason = ex is JsonException ? "it does not contain valid JSON" : "of an unexpected error";
            throw new InvalidOperationException(
                $"The package manifest file {fileInfo.PhysicalPath} could not be parsed because {reason}. See the inner exception for details.",
                ex);
        }
    }

    private PackageManifest? TryBuildExtensionsFolderManifest(
        IFileProvider fileProvider,
        string packageName,
        IReadOnlyList<IFileInfo> packageContents)
    {
        bool hasExtensionsFolder = packageContents
            .Any(f => f.IsDirectory && f.Name.InvariantEquals(ExtensionsFolderName));
        if (!hasExtensionsFolder)
        {
            return null;
        }

        var extensionsPath = WebPath.Combine(
            Constants.SystemDirectories.AppPlugins,
            packageName,
            ExtensionsFolderName);

        object[] bundles = fileProvider
            .GetDirectoryContents(extensionsPath)
            .Where(IsJsFile)
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Select(f => CreateBundleExtension(packageName, extensionsPath, f))
            .ToArray();

        if (bundles.Length == 0)
        {
            return null;
        }

        _logger.LogDebug("Discovered {Count} extension file(s) in {Path}", bundles.Length, extensionsPath);

        return new PackageManifest { Name = packageName, Extensions = bundles };
    }

    private static bool IsJsFile(IFileInfo file) =>
        !file.IsDirectory && file.Name.EndsWith(JsExtension, StringComparison.OrdinalIgnoreCase);

    private static object CreateBundleExtension(string packageName, string extensionsPath, IFileInfo file)
    {
        string stem = Path.GetFileNameWithoutExtension(file.Name);
        return new
        {
            type = "bundle",
            alias = $"{packageName}.Extensions.Bundle.{stem}",
            name = $"{packageName} Extensions Bundle ({stem})",
            js = WebPath.Combine(extensionsPath, file.Name) + "?v=%CACHE_BUSTER%",
        };
    }

    private static async Task<string> ReadAllTextAsync(IFileInfo fileInfo)
    {
        await using Stream stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
