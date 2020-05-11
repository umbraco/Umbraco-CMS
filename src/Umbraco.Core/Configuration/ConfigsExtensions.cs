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

        public static IImagingSettings Imaging(this Configs configs)
            => configs.GetConfig<IImagingSettings>();
        public static IGlobalSettings Global(this Configs configs)
            => configs.GetConfig<IGlobalSettings>();

        public static IHostingSettings Hosting(this Configs configs)
            => configs.GetConfig<IHostingSettings>();

        public static IConnectionStrings ConnectionStrings(this Configs configs)
            => configs.GetConfig<IConnectionStrings>();

        public static IContentSettings Content(this Configs configs)
            => configs.GetConfig<IContentSettings>();

        public static ISecuritySettings Security(this Configs configs)
            => configs.GetConfig<ISecuritySettings>();

        public static ITypeFinderSettings TypeFinder(this Configs configs)
            => configs.GetConfig<ITypeFinderSettings>();


        public static IUserPasswordConfiguration UserPasswordConfiguration(this Configs configs)
            => configs.GetConfig<IUserPasswordConfiguration>();
        public static IMemberPasswordConfiguration MemberPasswordConfiguration(this Configs configs)
            => configs.GetConfig<IMemberPasswordConfiguration>();

        public static IRequestHandlerSettings RequestHandler(this Configs configs)
            => configs.GetConfig<IRequestHandlerSettings>();

        public static IWebRoutingSettings WebRouting(this Configs configs)
            => configs.GetConfig<IWebRoutingSettings>();

        public static IHealthChecksSettings HealthChecks(this Configs configs)
            => configs.GetConfig<IHealthChecksSettings>();
        public static ICoreDebugSettings CoreDebug(this Configs configs)
            => configs.GetConfig<ICoreDebugSettings>();

    }
}
