using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration.Grid
{
    class GridConfig : IGridConfig
    {
        public GridConfig(ILogger logger, AppCaches appCaches, DirectoryInfo configFolder, bool isDebug)
        {
            EditorsConfig = new GridEditorsConfig(logger, appCaches, configFolder, isDebug);
        }

        public IGridEditorsConfig EditorsConfig { get; }
    }
}
