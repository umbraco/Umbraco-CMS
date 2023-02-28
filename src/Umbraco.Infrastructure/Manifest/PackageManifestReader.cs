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

internal sealed class AppPluginsFileProviderPackageManifestReader : IPackageManifestReader
{
    private readonly IPackageManifestFileProviderFactory _packageManifestFileProviderFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<AppPluginsFileProviderPackageManifestReader> _logger;

    public AppPluginsFileProviderPackageManifestReader(
        IPackageManifestFileProviderFactory packageManifestFileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<AppPluginsFileProviderPackageManifestReader> logger)
    {
        _packageManifestFileProviderFactory = packageManifestFileProviderFactory;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        IFileProvider? fileProvider = _packageManifestFileProviderFactory.Create();
        if (fileProvider is null)
        {
            throw new ArgumentNullException(nameof(fileProvider));
        }

        IFileInfo[] files = GetAllPackageManifestFiles(fileProvider, Constants.SystemDirectories.AppPlugins).ToArray();
        return await ParsePackageManifestFiles(files);
    }

    private static IEnumerable<IFileInfo> GetAllPackageManifestFiles(IFileProvider fileProvider, string path)
    {
        const string extensionFileName = "umbraco-package.json";
        foreach (IFileInfo fileInfo in fileProvider.GetDirectoryContents(path))
        {
            if (fileInfo.IsDirectory)
            {
                var virtualPath = WebPath.Combine(path, fileInfo.Name);

                // find all extension package configuration files recursively
                foreach (IFileInfo nested in GetAllPackageManifestFiles(fileProvider, virtualPath))
                {
                    yield return nested;
                }
            }
            else if (fileInfo.Name.InvariantEquals(extensionFileName))
            {
                yield return fileInfo;
            }
        }
    }

    private async Task<IEnumerable<PackageManifest>> ParsePackageManifestFiles(IFileInfo[] files)
    {
        var packageManifests = new List<PackageManifest>();
        foreach (IFileInfo fileInfo in files)
        {
            string fileContent;
            await using (Stream stream = fileInfo.CreateReadStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    fileContent = await reader.ReadToEndAsync();
                }
            }

            if (fileContent.IsNullOrWhiteSpace())
            {
                continue;
            }

            try
            {
                PackageManifest? packageManifest = _jsonSerializer.Deserialize<PackageManifest>(fileContent);
                if (packageManifest != null)
                {
                    packageManifests.Add(packageManifest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load package manifest file: {FileName}", fileInfo.Name);
                throw;
            }
        }

        return packageManifests;
    }
}
