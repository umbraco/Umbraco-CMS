using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Configuration.Grid
{
    class GridConfig : IGridConfig
    {
        public GridConfig(ILogger logger, AppCaches appCaches, DirectoryInfo configFolder, ManifestParser manifestParser, bool isDebug)
        {
            EditorsConfig = new GridEditorsConfig(logger, appCaches, configFolder, manifestParser, isDebug);
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
