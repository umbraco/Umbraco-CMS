using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;

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

        public static IHealthChecks HealthChecks(this Configs configs)
            => configs.GetConfig<IHealthChecks>();

        public static IGridConfig Grids(this Configs configs)
            => configs.GetConfig<IGridConfig>();

        public static CoreDebug CoreDebug(this Configs configs)
            => configs.GetConfig<CoreDebug>();
    }
}
