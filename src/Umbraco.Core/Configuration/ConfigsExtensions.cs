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


        public static IConnectionStrings ConnectionStrings(this Configs configs)
            => configs.GetConfig<IConnectionStrings>();

        public static ISecuritySettings Security(this Configs configs)
            => configs.GetConfig<ISecuritySettings>();

        public static IWebRoutingSettings WebRouting(this Configs configs)
            => configs.GetConfig<IWebRoutingSettings>();

    }
}
