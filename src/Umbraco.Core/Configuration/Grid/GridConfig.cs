using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Hosting;
using Umbraco.Core.Manifest;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Configuration.Grid
{
    [UmbracoVolatile]
    public class GridConfig : IGridConfig
    {
        public GridConfig(AppCaches appCaches, IManifestParser manifestParser, IJsonSerializer jsonSerializer, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        {
            EditorsConfig = new GridEditorsConfig(appCaches, hostingEnvironment, manifestParser, jsonSerializer, loggerFactory.CreateLogger<GridEditorsConfig>());
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
