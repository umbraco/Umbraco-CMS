using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration.Grid
{
    class GridConfig : IGridConfig
    {
        public GridConfig(ILogger logger, IAppPolicedCache runtimeCache, DirectoryInfo configFolder, bool isDebug)
        {
            EditorsConfig = new GridEditorsConfig(logger, runtimeCache, configFolder, isDebug);
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
