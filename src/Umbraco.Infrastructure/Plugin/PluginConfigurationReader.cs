using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Plugin;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Plugin;

internal sealed class PluginConfigurationReader : IPluginConfigurationReader
{
    private readonly IPluginConfigurationFileProviderFactory _pluginConfigurationFileProviderFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<PluginConfigurationReader> _logger;

    public PluginConfigurationReader(
        IPluginConfigurationFileProviderFactory pluginConfigurationFileProviderFactory,
        IJsonSerializer jsonSerializer,
        ILogger<PluginConfigurationReader> logger)
    {
        _pluginConfigurationFileProviderFactory = pluginConfigurationFileProviderFactory;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public async Task<IEnumerable<PluginConfiguration>> ReadPluginConfigurationsAsync()
    {
        IFileProvider? fileProvider = _pluginConfigurationFileProviderFactory.Create();
        if (fileProvider is null)
        {
            throw new ArgumentNullException(nameof(fileProvider));
        }

        IFileInfo[] files = GetAllPluginConfigurationFiles(fileProvider, Constants.SystemDirectories.AppPlugins).ToArray();
        return await ParsePluginConfigurationFiles(files);
    }

    private static IEnumerable<IFileInfo> GetAllPluginConfigurationFiles(IFileProvider fileProvider, string path)
    {
        const string extensionFileName = "umbraco-package.json";
        foreach (IFileInfo fileInfo in fileProvider.GetDirectoryContents(path))
        {
            if (fileInfo.IsDirectory)
            {
                var virtualPath = WebPath.Combine(path, fileInfo.Name);

                // find all extension package configuration files recursively
                foreach (IFileInfo nested in GetAllPluginConfigurationFiles(fileProvider, virtualPath))
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

    private async Task<IEnumerable<PluginConfiguration>> ParsePluginConfigurationFiles(IFileInfo[] files)
    {
        var pluginConfigurations = new List<PluginConfiguration>();
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
                PluginConfiguration? pluginConfiguration = _jsonSerializer.Deserialize<PluginConfiguration>(fileContent);
                if (pluginConfiguration != null)
                {
                    pluginConfigurations.Add(pluginConfiguration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load plugin configuration file: {FileName}", fileInfo.Name);
            }
        }

        return pluginConfigurations;
    }
}
