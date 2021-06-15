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

        private static OptionsBuilder<TOptions> AddOptions<TOptions>(IUmbracoBuilder builder, string key)
            where TOptions : class
            => builder.Services.AddOptions<TOptions>()
                .Bind(builder.Config.GetSection(key))
                .ValidateDataAnnotations();

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
            builder.Services.AddSingleton<IValidateOptions<UnattendedSettings>, UnattendedSettingsValidator>();

            // Register configuration sections.

            builder.Services.Configure<ModelsBuilderSettings>(builder.Config.GetSection(Constants.Configuration.ConfigModelsBuilder), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<ConnectionStrings>(builder.Config.GetSection("ConnectionStrings"), o => o.BindNonPublicProperties = true);

            AddOptions<ActiveDirectorySettings>(builder, Constants.Configuration.ConfigActiveDirectory);
            AddOptions<ContentSettings>(builder, Constants.Configuration.ConfigContent);
            AddOptions<CoreDebugSettings>(builder, Constants.Configuration.ConfigCoreDebug);
            AddOptions<ExceptionFilterSettings>(builder, Constants.Configuration.ConfigExceptionFilter);
            AddOptions<GlobalSettings>(builder, Constants.Configuration.ConfigGlobal);
            AddOptions<HealthChecksSettings>(builder, Constants.Configuration.ConfigHealthChecks);
            AddOptions<HostingSettings>(builder, Constants.Configuration.ConfigHosting);
            AddOptions<ImagingSettings>(builder, Constants.Configuration.ConfigImaging);
            AddOptions<IndexCreatorSettings>(builder, Constants.Configuration.ConfigExamine);
            AddOptions<KeepAliveSettings>(builder, Constants.Configuration.ConfigKeepAlive);
            AddOptions<LoggingSettings>(builder, Constants.Configuration.ConfigLogging);
            AddOptions<MemberPasswordConfigurationSettings>(builder, Constants.Configuration.ConfigMemberPassword);
            AddOptions<NuCacheSettings>(builder, Constants.Configuration.ConfigNuCache);
            AddOptions<RequestHandlerSettings>(builder, Constants.Configuration.ConfigRequestHandler);
            AddOptions<RuntimeSettings>(builder, Constants.Configuration.ConfigRuntime);
            AddOptions<SecuritySettings>(builder, Constants.Configuration.ConfigSecurity);
            AddOptions<TourSettings>(builder, Constants.Configuration.ConfigTours);
            AddOptions<TypeFinderSettings>(builder, Constants.Configuration.ConfigTypeFinder);
            AddOptions<UserPasswordConfigurationSettings>(builder, Constants.Configuration.ConfigUserPassword);
            AddOptions<WebRoutingSettings>(builder, Constants.Configuration.ConfigWebRouting);
            AddOptions<UmbracoPluginSettings>(builder, Constants.Configuration.ConfigPlugins);
            AddOptions<UnattendedSettings>(builder, Constants.Configuration.ConfigUnattended);
            AddOptions<RichTextEditorSettings>(builder, Constants.Configuration.ConfigRichTextEditor);
            AddOptions<RuntimeMinificationSettings>(builder, Constants.Configuration.ConfigRuntimeMinification);

            return builder;
        }
    }
}
