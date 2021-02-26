using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.DependencyInjection
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
            builder.Services.Configure<ActiveDirectorySettings>(builder.Config.GetSection(Constants.Configuration.ConfigActiveDirectory));
            builder.Services.Configure<ConnectionStrings>(builder.Config.GetSection("ConnectionStrings"), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<ContentSettings>(builder.Config.GetSection(Constants.Configuration.ConfigContent));
            builder.Services.Configure<CoreDebugSettings>(builder.Config.GetSection(Constants.Configuration.ConfigCoreDebug));
            builder.Services.Configure<ExceptionFilterSettings>(builder.Config.GetSection(Constants.Configuration.ConfigExceptionFilter));
            builder.Services.Configure<GlobalSettings>(builder.Config.GetSection(Constants.Configuration.ConfigGlobal));
            builder.Services.Configure<HealthChecksSettings>(builder.Config.GetSection(Constants.Configuration.ConfigHealthChecks));
            builder.Services.Configure<HostingSettings>(builder.Config.GetSection(Constants.Configuration.ConfigHosting));
            builder.Services.Configure<ImagingSettings>(builder.Config.GetSection(Constants.Configuration.ConfigImaging));
            builder.Services.Configure<IndexCreatorSettings>(builder.Config.GetSection(Constants.Configuration.ConfigExamine));
            builder.Services.Configure<KeepAliveSettings>(builder.Config.GetSection(Constants.Configuration.ConfigKeepAlive));
            builder.Services.Configure<LoggingSettings>(builder.Config.GetSection(Constants.Configuration.ConfigLogging));
            builder.Services.Configure<MemberPasswordConfigurationSettings>(builder.Config.GetSection(Constants.Configuration.ConfigMemberPassword));
            builder.Services.Configure<ModelsBuilderSettings>(builder.Config.GetSection(Constants.Configuration.ConfigModelsBuilder), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<NuCacheSettings>(builder.Config.GetSection(Constants.Configuration.ConfigNuCache));
            builder.Services.Configure<RequestHandlerSettings>(builder.Config.GetSection(Constants.Configuration.ConfigRequestHandler));
            builder.Services.Configure<RuntimeSettings>(builder.Config.GetSection(Constants.Configuration.ConfigRuntime));
            builder.Services.Configure<SecuritySettings>(builder.Config.GetSection(Constants.Configuration.ConfigSecurity));
            builder.Services.Configure<TourSettings>(builder.Config.GetSection(Constants.Configuration.ConfigTours));
            builder.Services.Configure<TypeFinderSettings>(builder.Config.GetSection(Constants.Configuration.ConfigTypeFinder));
            builder.Services.Configure<UserPasswordConfigurationSettings>(builder.Config.GetSection(Constants.Configuration.ConfigUserPassword));
            builder.Services.Configure<WebRoutingSettings>(builder.Config.GetSection(Constants.Configuration.ConfigWebRouting));
            builder.Services.Configure<UmbracoPluginSettings>(builder.Config.GetSection(Constants.Configuration.ConfigPlugins));

            return builder;
        }
    }
}
