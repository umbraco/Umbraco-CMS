using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Configuration.Grid;

[Obsolete("The grid is obsolete, will be removed in V13")]
public class GridConfig : IGridConfig
{
    public GridConfig(
        AppCaches appCaches,
        IManifestParser manifestParser,
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        ILoggerFactory loggerFactory,
        IGridEditorsConfigFileProviderFactory gridEditorsConfigFileProviderFactory)
        => EditorsConfig =
            new GridEditorsConfig(appCaches, hostingEnvironment, manifestParser, jsonSerializer, loggerFactory.CreateLogger<GridEditorsConfig>(), gridEditorsConfigFileProviderFactory);

    [Obsolete("Use other ctor - Will be removed in Umbraco 13")]
    public GridConfig(
        AppCaches appCaches,
        IManifestParser manifestParser,
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        ILoggerFactory loggerFactory)
        : this(
              appCaches,
              manifestParser,
              jsonSerializer,
              hostingEnvironment,
              loggerFactory,
              StaticServiceProvider.Instance.GetRequiredService<IGridEditorsConfigFileProviderFactory>())
    {
    }

    public IGridEditorsConfig EditorsConfig { get; }
}
