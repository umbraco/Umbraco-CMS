using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Core.Configuration.Grid;

internal class GridEditorsConfig : IGridEditorsConfig
{
    private readonly AppCaches _appCaches;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<GridEditorsConfig> _logger;
    private readonly IGridEditorsConfigFileProviderFactory _gridEditorsConfigFileProviderFactory;
    private readonly IManifestParser _manifestParser;

    public GridEditorsConfig(
        AppCaches appCaches,
        IHostingEnvironment hostingEnvironment,
        IManifestParser manifestParser,
        IJsonSerializer jsonSerializer,
        ILogger<GridEditorsConfig> logger,
        IGridEditorsConfigFileProviderFactory gridEditorsConfigFileProviderFactory)
    {
        _appCaches = appCaches;
        _hostingEnvironment = hostingEnvironment;
        _manifestParser = manifestParser;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
        _gridEditorsConfigFileProviderFactory = gridEditorsConfigFileProviderFactory;
    }

    [Obsolete("Use other ctor - Will be removed in Umbraco 13")]
    public GridEditorsConfig(
        AppCaches appCaches,
        IHostingEnvironment hostingEnvironment,
        IManifestParser manifestParser,
        IJsonSerializer jsonSerializer,
        ILogger<GridEditorsConfig> logger)
        : this(
              appCaches,
              hostingEnvironment,
              manifestParser,
              jsonSerializer,
              logger,
              StaticServiceProvider.Instance.GetRequiredService<IGridEditorsConfigFileProviderFactory>())
    {
    }

    public IEnumerable<IGridEditorConfig> Editors
    {
        get
        {
            List<IGridEditorConfig> GetResult()
            {
                IFileInfo? gridConfig = null;
                var editors = new List<IGridEditorConfig>();
                var configPath = Constants.SystemDirectories.Config.TrimStart(Constants.CharArrays.Tilde);

                // Get physical file if it exists
                var configPhysicalDirPath = _hostingEnvironment.MapPathContentRoot(configPath);

                if (Directory.Exists(configPhysicalDirPath) == true)
                {
                    var physicalFileProvider = new PhysicalFileProvider(configPhysicalDirPath);
                    gridConfig = GetConfigFile(physicalFileProvider, string.Empty);
                }

                // If there is no physical file, check in RCLs
                if (gridConfig is null)
                {
                    IFileProvider? compositeFileProvider = _gridEditorsConfigFileProviderFactory.Create();

                    if (compositeFileProvider is null)
                    {
                        throw new ArgumentNullException(nameof(compositeFileProvider));
                    }

                    gridConfig = GetConfigFile(compositeFileProvider, configPath);
                }

                if (gridConfig is not null)
                {
                    using Stream stream = gridConfig.CreateReadStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var sourceString = reader.ReadToEnd();

                    try
                    {
                        editors.AddRange(_jsonSerializer.Deserialize<IEnumerable<GridEditor>>(sourceString)!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Could not parse the contents of grid.editors.config.js into a JSON array '{Json}",
                            sourceString);
                    }
                }

                // Read default from embedded file
                else
                {
                    IFileProvider configFileProvider = new EmbeddedFileProvider(GetType().Assembly, "Umbraco.Cms.Core.EmbeddedResources.Grid");
                    IFileInfo embeddedConfig = configFileProvider.GetFileInfo("grid.editors.config.js");

                    using Stream stream = embeddedConfig.CreateReadStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var sourceString = reader.ReadToEnd();
                    editors.AddRange(_jsonSerializer.Deserialize<IEnumerable<GridEditor>>(sourceString)!);
                }

                // Add manifest editors, skip duplicates
                foreach (GridEditor gridEditor in _manifestParser.CombinedManifest.GridEditors)
                {
                    if (editors.Contains(gridEditor) == false)
                    {
                        editors.Add(gridEditor);
                    }
                }

                return editors;
            }

            // cache the result if debugging is disabled
            List<IGridEditorConfig>? result = _hostingEnvironment.IsDebugMode
                ? GetResult()
                : _appCaches.RuntimeCache.GetCacheItem(typeof(GridEditorsConfig) + ".Editors", GetResult, TimeSpan.FromMinutes(10));

            return result!;
        }
    }

    private static IFileInfo? GetConfigFile(IFileProvider fileProvider, string path)
    {
        IFileInfo fileInfo = fileProvider.GetFileInfo($"{path}/grid.editors.config.js");
        return fileInfo.Exists ? fileInfo : null;
    }
}
