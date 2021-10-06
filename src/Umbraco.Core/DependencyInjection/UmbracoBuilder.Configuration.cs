using System;
using System.Reflection;
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

        private static IUmbracoBuilder AddUmbracoOptions<TOptions>(this IUmbracoBuilder builder)
            where TOptions : class
        {
            var umbracoOptionsAttribute = typeof(TOptions).GetCustomAttribute<UmbracoOptionsAttribute>();

            if (umbracoOptionsAttribute is null)
            {
                throw new ArgumentException("typeof(TOptions) do not have the UmbracoOptionsAttribute");
            }


            builder.Services.AddOptions<TOptions>()
                .Bind(builder.Config.GetSection(umbracoOptionsAttribute.ConfigurationKey),
                    o => o.BindNonPublicProperties = umbracoOptionsAttribute.BindNonPublicProperties)
                .ValidateDataAnnotations();

             return builder;
        }

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
            builder
                .AddUmbracoOptions<ModelsBuilderSettings>()
                .AddUmbracoOptions<ConnectionStrings>()
                .AddUmbracoOptions<ActiveDirectorySettings>()
                .AddUmbracoOptions<ContentSettings>()
                .AddUmbracoOptions<CoreDebugSettings>()
                .AddUmbracoOptions<ExceptionFilterSettings>()
                .AddUmbracoOptions<GlobalSettings>()
                .AddUmbracoOptions<HealthChecksSettings>()
                .AddUmbracoOptions<HostingSettings>()
                .AddUmbracoOptions<ImagingSettings>()
                .AddUmbracoOptions<IndexCreatorSettings>()
                .AddUmbracoOptions<KeepAliveSettings>()
                .AddUmbracoOptions<LoggingSettings>()
                .AddUmbracoOptions<MemberPasswordConfigurationSettings>()
                .AddUmbracoOptions<NuCacheSettings>()
                .AddUmbracoOptions<RequestHandlerSettings>()
                .AddUmbracoOptions<RuntimeSettings>()
                .AddUmbracoOptions<SecuritySettings>()
                .AddUmbracoOptions<TourSettings>()
                .AddUmbracoOptions<TypeFinderSettings>()
                .AddUmbracoOptions<UserPasswordConfigurationSettings>()
                .AddUmbracoOptions<WebRoutingSettings>()
                .AddUmbracoOptions<UmbracoPluginSettings>()
                .AddUmbracoOptions<UnattendedSettings>()
                .AddUmbracoOptions<RichTextEditorSettings>()
                .AddUmbracoOptions<BasicAuthSettings>()
                .AddUmbracoOptions<RuntimeMinificationSettings>()
                .AddUmbracoOptions<PackageMigrationSettings>();

            return builder;
        }
    }
}
