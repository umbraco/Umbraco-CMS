using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Configuration.Grid
{
    public class GridConfig : IGridConfig
    {
        public GridConfig(AppCaches appCaches, IManifestParser manifestParser, IJsonSerializer jsonSerializer, IHostingEnvironment hostingEnvironment, ILogger logger)
        {
            EditorsConfig = new GridEditorsConfig(appCaches, hostingEnvironment, manifestParser, jsonSerializer, logger);
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
