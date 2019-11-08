using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Configs"/> class.
    /// </summary>
    public static class ConfigsExtensions
    {

        public static void AddCoreConfigs(this Configs configs, IIOHelper ioHelper)
        {
            var configDir = new DirectoryInfo(ioHelper.MapPath(SystemDirectories.Config));

            // GridConfig depends on runtime caches, manifest parsers... and cannot be available during composition
            configs.Add<IGridConfig>(factory => new GridConfig(
                factory.GetInstance<ILogger>(),
                factory.GetInstance<AppCaches>(),
                configDir,
                factory.GetInstance<IManifestParser>(),
                factory.GetInstance<IRuntimeState>().Debug));
        }
    }
}
