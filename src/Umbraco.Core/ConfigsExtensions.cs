using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Configs"/> class.
    /// </summary>
    public static class ConfigsExtensions
    {
        public static IGlobalSettings Global(this Configs configs)
            => configs.GetConfig<IGlobalSettings>();

        public static IUmbracoSettingsSection Settings(this Configs configs)
            => configs.GetConfig<IUmbracoSettingsSection>();

        public static IDashboardSection Dashboards(this Configs configs)
            => configs.GetConfig<IDashboardSection>();

        public static IHealthChecks HealthChecks(this Configs configs)
            => configs.GetConfig<IHealthChecks>();

        public static IGridConfig Grids(this Configs configs)
            => configs.GetConfig<IGridConfig>();

        internal static CoreDebug CoreDebug(this Configs configs)
            => configs.GetConfig<CoreDebug>();

        public static void AddCoreConfigs(this Configs configs)
        {
            var configDir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config));

            configs.Add<IGlobalSettings>(() => new GlobalSettings());
            configs.Add<IUmbracoSettingsSection>("umbracoConfiguration/settings");
            configs.Add<IDashboardSection>("umbracoConfiguration/dashBoard");
            configs.Add<IHealthChecks>("umbracoConfiguration/HealthChecks");

            configs.Add(() => new CoreDebug());

            // GridConfig depends on runtime caches, manifest parsers... and cannot be available during composition
            configs.Add<IGridConfig>(factory => new GridConfig(factory.GetInstance<ILogger>(), factory.GetInstance<IRuntimeCacheProvider>(), configDir, factory.GetInstance<IRuntimeState>().Debug));
        }
    }
}