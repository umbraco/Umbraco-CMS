using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        public static IGlobalSettings Global(this Configs configs)
            => configs.GetConfig<IGlobalSettings>();

        public static IConnectionStrings ConnectionStrings(this Configs configs)
            => configs.GetConfig<IConnectionStrings>();

        public static IUmbracoSettingsSection Settings(this Configs configs)
            => configs.GetConfig<IUmbracoSettingsSection>();

        public static IHealthChecks HealthChecks(this Configs configs)
            => configs.GetConfig<IHealthChecks>();

        public static IGridConfig Grids(this Configs configs)
            => configs.GetConfig<IGridConfig>();

        public static ICoreDebug CoreDebug(this Configs configs)
            => configs.GetConfig<ICoreDebug>();

        public static void AddCoreConfigs(this Configs configs, IIOHelper ioHelper)
        {
            var configDir = new DirectoryInfo(ioHelper.MapPath(Constants.SystemDirectories.Config));


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
