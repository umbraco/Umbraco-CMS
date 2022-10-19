using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Configuration.Grid;

public class GridConfig : IGridConfig
{
    public GridConfig(
        AppCaches appCaches,
        IManifestParser manifestParser,
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        ILoggerFactory loggerFactory)
        => EditorsConfig =
        new GridEditorsConfig(appCaches, hostingEnvironment, manifestParser, jsonSerializer, loggerFactory.CreateLogger<GridEditorsConfig>());

    public IGridEditorsConfig EditorsConfig { get; }
}
