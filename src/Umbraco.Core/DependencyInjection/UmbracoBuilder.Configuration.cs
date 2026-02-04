using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder" />
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco options of type <typeparamref name="TOptions" /> to the builder.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to add. Must have the <see cref="UmbracoOptionsAttribute" />.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="configure">Optional action to configure the <see cref="OptionsBuilder{TOptions}" />.</param>
    /// <returns>The <see cref="IUmbracoBuilder" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="TOptions" /> does not have the <see cref="UmbracoOptionsAttribute" />.</exception>
    private static IUmbracoBuilder AddUmbracoOptions<TOptions>(this IUmbracoBuilder builder, Action<OptionsBuilder<TOptions>>? configure = null)
        where TOptions : class
    {
        UmbracoOptionsAttribute? umbracoOptionsAttribute = typeof(TOptions).GetCustomAttribute<UmbracoOptionsAttribute>();
        if (umbracoOptionsAttribute is null)
        {
            throw new ArgumentException($"{typeof(TOptions)} do not have the UmbracoOptionsAttribute.");
        }

        OptionsBuilder<TOptions>? optionsBuilder = builder.Services.AddOptions<TOptions>()
            .Bind(
                builder.Config.GetSection(umbracoOptionsAttribute.ConfigurationKey),
                o => o.BindNonPublicProperties = umbracoOptionsAttribute.BindNonPublicProperties)
            .ValidateDataAnnotations();

        configure?.Invoke(optionsBuilder);

        return builder;
    }

    /// <summary>
    /// Add Umbraco configuration services and options
    /// </summary>
    public static IUmbracoBuilder AddConfiguration(this IUmbracoBuilder builder)
    {
        // Idempotency check using a private marker class
        if (builder.Services.Any(s => s.ServiceType == typeof(ConfigurationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<ConfigurationMarker>();

        // Register configuration validators.
        builder.Services.AddSingleton<IValidateOptions<ContentSettings>, ContentSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<DeliveryApiSettings>, DeliveryApiSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, GlobalSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<HealthChecksSettings>, HealthChecksSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<LoggingSettings>, LoggingSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<RequestHandlerSettings>, RequestHandlerSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<UnattendedSettings>, UnattendedSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<SecuritySettings>, SecuritySettingsValidator>();

        // Register configuration sections.
        builder
            .AddUmbracoOptions<ModelsBuilderSettings>()
            .AddUmbracoOptions<IndexCreatorSettings>()
            .AddUmbracoOptions<MarketplaceSettings>()
            .AddUmbracoOptions<ContentSettings>()
            .AddUmbracoOptions<DeliveryApiSettings>()
            .AddUmbracoOptions<CoreDebugSettings>()
            .AddUmbracoOptions<DictionarySettings>()
            .AddUmbracoOptions<ExceptionFilterSettings>()
            .AddUmbracoOptions<GlobalSettings>(optionsBuilder => optionsBuilder.PostConfigure(options =>
            {
                if (string.IsNullOrEmpty(options.UmbracoMediaPhysicalRootPath))
                {
                    options.UmbracoMediaPhysicalRootPath = options.UmbracoMediaPath;
                }
            }))
            .AddUmbracoOptions<HealthChecksSettings>()
            .AddUmbracoOptions<HostingSettings>()
            .AddUmbracoOptions<ImagingSettings>()
            .AddUmbracoOptions<IndexingSettings>()
            .AddUmbracoOptions<LoggingSettings>()
            .AddUmbracoOptions<LongRunningOperationsSettings>()
            .AddUmbracoOptions<MemberPasswordConfigurationSettings>()
            .AddUmbracoOptions<NuCacheSettings>()
            .AddUmbracoOptions<RequestHandlerSettings>()
            .AddUmbracoOptions<RuntimeSettings>()
            .AddUmbracoOptions<SecuritySettings>()
            .AddUmbracoOptions<TypeFinderSettings>()
            .AddUmbracoOptions<UserPasswordConfigurationSettings>()
            .AddUmbracoOptions<WebRoutingSettings>()
            .AddUmbracoOptions<UmbracoPluginSettings>()
            .AddUmbracoOptions<UnattendedSettings>()
            .AddUmbracoOptions<BasicAuthSettings>()
            .AddUmbracoOptions<LegacyPasswordMigrationSettings>()
            .AddUmbracoOptions<PackageMigrationSettings>()
            .AddUmbracoOptions<HelpPageSettings>()
            .AddUmbracoOptions<DataTypesSettings>()
            .AddUmbracoOptions<WebhookSettings>()
            .AddUmbracoOptions<CacheSettings>()
            .AddUmbracoOptions<SystemDateMigrationSettings>()
            .AddUmbracoOptions<DistributedJobSettings>()
            .AddUmbracoOptions<BackOfficeTokenCookieSettings>();

        // Configure connection string and ensure it's updated when the configuration changes
        builder.Services.AddSingleton<IConfigureOptions<ConnectionStrings>, ConfigureConnectionStrings>();
        builder.Services.AddSingleton<IOptionsChangeTokenSource<ConnectionStrings>, ConfigurationChangeTokenSource<ConnectionStrings>>();

        builder.Services.Configure<InstallDefaultDataSettings>(
            Constants.Configuration.NamedOptions.InstallDefaultData.Languages,
            builder.Config.GetSection($"{Constants.Configuration.ConfigInstallDefaultData}:{Constants.Configuration.NamedOptions.InstallDefaultData.Languages}"));
        builder.Services.Configure<InstallDefaultDataSettings>(
            Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes,
            builder.Config.GetSection($"{Constants.Configuration.ConfigInstallDefaultData}:{Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes}"));
        builder.Services.Configure<InstallDefaultDataSettings>(
            Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes,
            builder.Config.GetSection($"{Constants.Configuration.ConfigInstallDefaultData}:{Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes}"));
        builder.Services.Configure<InstallDefaultDataSettings>(
            Constants.Configuration.NamedOptions.InstallDefaultData.MemberTypes,
            builder.Config.GetSection($"{Constants.Configuration.ConfigInstallDefaultData}:{Constants.Configuration.NamedOptions.InstallDefaultData.MemberTypes}"));

        builder.Services.AddOptions<TinyMceToTiptapMigrationSettings>();

        return builder;
    }

    /// <summary>
    /// Marker class to ensure AddConfiguration is only called once.
    /// </summary>
    private sealed class ConfigurationMarker
    {
    }
}
