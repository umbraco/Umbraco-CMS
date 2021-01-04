using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/>
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Add Umbraco configuration services and options
        /// </summary>
        public static IUmbracoBuilder AddConfiguration(this IUmbracoBuilder builder)
        {
            // Register configuration validators.
            builder.Services.AddSingleton<IValidateOptions<ContentSettings>, ContentSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, GlobalSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<HealthChecksSettings>, HealthChecksSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<RequestHandlerSettings>, RequestHandlerSettingsValidator>();

            // Register configuration sections.
            builder.Services.Configure<ActiveDirectorySettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigActiveDirectory));
            builder.Services.Configure<ConnectionStrings>(builder.Config.GetSection("ConnectionStrings"), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<ContentSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigContent));
            builder.Services.Configure<CoreDebugSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigCoreDebug));
            builder.Services.Configure<ExceptionFilterSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigExceptionFilter));
            builder.Services.Configure<GlobalSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigGlobal));
            builder.Services.Configure<HealthChecksSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigHealthChecks));
            builder.Services.Configure<HostingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigHosting));
            builder.Services.Configure<ImagingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigImaging));
            builder.Services.Configure<IndexCreatorSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigExamine));
            builder.Services.Configure<KeepAliveSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigKeepAlive));
            builder.Services.Configure<LoggingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigLogging));
            builder.Services.Configure<MemberPasswordConfigurationSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigMemberPassword));
            builder.Services.Configure<ModelsBuilderSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigModelsBuilder), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<NuCacheSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigNuCache));
            builder.Services.Configure<RequestHandlerSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigRequestHandler));
            builder.Services.Configure<RuntimeSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigRuntime));
            builder.Services.Configure<SecuritySettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigSecurity));
            builder.Services.Configure<TourSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigTours));
            builder.Services.Configure<TypeFinderSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigTypeFinder));
            builder.Services.Configure<UserPasswordConfigurationSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigUserPassword));
            builder.Services.Configure<WebRoutingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigWebRouting));
            builder.Services.Configure<UmbracoPluginSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigPlugins));

            return builder;
        }
    }
}
