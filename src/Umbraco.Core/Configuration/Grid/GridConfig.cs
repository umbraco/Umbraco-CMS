using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Configuration.Grid
{
    class GridConfig : IGridConfig
    {
        public GridConfig(
            ILogger logger,
            IRuntimeCacheProvider runtimeCache,
            IContentTypeService contentTypeService,
            DirectoryInfo appPlugins,
            DirectoryInfo configFolder,
            bool isDebug)
        {
            EditorsConfig = new GridEditorsConfig(
                logger,
                runtimeCache,
                contentTypeService,
                appPlugins,
                configFolder,
                isDebug);
        }

        public IGridEditorsConfig EditorsConfig { get; private set; }
    }
}
