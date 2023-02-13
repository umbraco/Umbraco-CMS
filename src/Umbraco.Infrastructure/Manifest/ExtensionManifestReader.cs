using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

internal sealed class ExtensionManifestReader : IExtensionManifestReader
{
    private readonly IManifestFileProviderFactory _manifestFileProviderFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<ExtensionManifestReader> _logger;

    public ExtensionManifestReader(IManifestFileProviderFactory manifestFileProviderFactory, IJsonSerializer jsonSerializer, ILogger<ExtensionManifestReader> logger)
    {
        _manifestFileProviderFactory = manifestFileProviderFactory;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<ExtensionManifest>> GetManifestsAsync()
    {
        var manifests = new List<ExtensionManifest>();
        IFileProvider? manifestFileProvider = _manifestFileProviderFactory.Create();

        if (manifestFileProvider is null)
        {
            throw new ArgumentNullException(nameof(manifestFileProvider));
        }

        IFileInfo[] manifestFiles = GetAllManifestFiles(manifestFileProvider, Constants.SystemDirectories.AppPlugins).ToArray();
        foreach (IFileInfo fileInfo in manifestFiles)
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
                ExtensionManifest? manifest = _jsonSerializer.Deserialize<ExtensionManifest>(fileContent);
                if (manifest != null)
                {
                    manifests.Add(manifest);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load extension manifest file: {FileName}", fileInfo.Name);
            }
        }

        return manifests;
    }

    // get all extension manifest files (recursively)
    private static IEnumerable<IFileInfo> GetAllManifestFiles(IFileProvider fileProvider, string path)
    {
        foreach (IFileInfo fileInfo in fileProvider.GetDirectoryContents(path))
        {
            if (fileInfo.IsDirectory)
            {
                var virtualPath = WebPath.Combine(path, fileInfo.Name);

                // recursively find nested extension manifest files
                foreach (IFileInfo nested in GetAllManifestFiles(fileProvider, virtualPath))
                {
                    yield return nested;
                }
            }
            // TODO: use the correct file name
            else if (fileInfo.Name.InvariantEquals("extension.json") && !string.IsNullOrEmpty(fileInfo.PhysicalPath))
            {
                yield return fileInfo;
            }
        }
    }
}
