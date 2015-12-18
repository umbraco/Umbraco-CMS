using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration.Grid
{
    class GridConfig : IGridConfig
    {
        public GridConfig(ILogger logger, IRuntimeCacheProvider runtimeCache, DirectoryInfo appPlugins, DirectoryInfo configFolder, bool isDebug)
        {
            EditorsConfig = new GridEditorsConfig(logger, runtimeCache, appPlugins, configFolder, isDebug);
        }

        public IGridEditorsConfig EditorsConfig { get; private set; }
    }
}