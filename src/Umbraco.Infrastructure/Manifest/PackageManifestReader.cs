using System.Text;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Manifest;

internal class PackageManifestReader : IPackageManifestReader
{
    private readonly string _appPluginsPath;
    private readonly IPackageManifestFileProviderFactory _packageManifestFileProviderFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<PackageManifestReader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifestReader"/> class.
    /// </summary>
    /// <param name="appPluginsPath">The absolute or relative path to the application's plugins directory.</param>
    /// <param name="packageManifestFileProviderFactory">A factory responsible for creating file providers to access package manifest files.</param>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize manifest files.</param>
    /// <param name="logger">The logger used for logging operations and errors related to package manifest reading.</param>
    public PackageManifestReader(
        string appPluginsPath,
        IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<PackageManifestReader> logger)
    {
        _appPluginsPath = appPluginsPath;
        _packageManifestFileProviderFactory = packageManifestFileProviderFactory;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously reads all package manifests from the configured file provider and returns a collection of <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifest"/> instances.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable of <see cref="Umbraco.Cms.Infrastructure.Manifest.PackageManifest"/> objects.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the file provider cannot be created.</exception>
    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        const string extensionFileName = "umbraco-package.json";

        IFileProvider? fileProvider = _packageManifestFileProviderFactory.Create();
        if (fileProvider is null)
        {
            throw new ArgumentNullException(nameof(fileProvider));
        }

        var packageManifests = new List<PackageManifest>();

        foreach (IFileInfo fileInfo in fileProvider.GetDirectoryContents(_appPluginsPath))
        {
            PackageManifest? packageManifest = null;
            if (fileInfo.IsDirectory)
            {
                // find all extension package configuration files one level deep
                var virtualPath = WebPath.Combine(_appPluginsPath, fileInfo.Name);
                IFileInfo[] subDirectoryContents = fileProvider.GetDirectoryContents(virtualPath).ToArray();
                IFileInfo? subManifest = subDirectoryContents
                    .FirstOrDefault(x => !x.IsDirectory && x.Name.InvariantEquals(extensionFileName));
                if (subManifest is not null)
                {
                    // default package manifests take precedence over other manifests per folder
                    packageManifest = await ParsePackageManifestAsync(subManifest);
                }
                else
                {
                    // let the concrete reader attempt to parse different manifests (e.g. bundles)
                    packageManifest = await ParsePackageManifestFromDirectoryAsync(fileProvider, fileInfo, subDirectoryContents);
                }
            }
            else if (fileInfo.Name.InvariantEquals(extensionFileName))
            {
                packageManifest = await ParsePackageManifestAsync(fileInfo);
            }

            if (packageManifest is not null)
            {
                packageManifests.Add(packageManifest);
            }
        }

        return packageManifests;
    }

    protected virtual Task<PackageManifest?> ParsePackageManifestFromDirectoryAsync(IFileProvider fileProvider, IFileInfo directory, IFileInfo[] directoryContents)
        => Task.FromResult<PackageManifest?>(null);

    private async Task<PackageManifest?> ParsePackageManifestAsync(IFileInfo fileInfo)
    {
        var fileContent = await ReadFileContent(fileInfo);

        if (fileContent.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            return _jsonSerializer.Deserialize<PackageManifest>(fileContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"The package manifest file {fileInfo.PhysicalPath} could not be parsed as it does not contain valid JSON. Please see the inner exception for details.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"The package manifest file {fileInfo.PhysicalPath} could not be parsed due to an unexpected error. Please see the inner exception for details.", ex);
        }
    }

    private static async Task<string> ReadFileContent(IFileInfo fileInfo)
    {
        await using Stream stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
