using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.Models.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder" />
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    internal static IUmbracoBuilder AddUmbracoOptions<TOptions>(this IUmbracoBuilder builder, Action<OptionsBuilder<TOptions>>? configure = null)
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
        // Register configuration validators.
        builder.Services.AddSingleton<IValidateOptions<ContentSettings>, ContentSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, GlobalSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<HealthChecksSettings>, HealthChecksSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<RequestHandlerSettings>, RequestHandlerSettingsValidator>();
        builder.Services.AddSingleton<IValidateOptions<UnattendedSettings>, UnattendedSettingsValidator>();

        // Register configuration sections.
        builder
            .AddUmbracoOptions<ModelsBuilderSettings>()
            .AddUmbracoOptions<ActiveDirectorySettings>()
            .AddUmbracoOptions<IndexCreatorSettings>()
            .AddUmbracoOptions<MarketplaceSettings>()
            .AddUmbracoOptions<ContentSettings>()
            .AddUmbracoOptions<DeliveryApiSettings>()
            .AddUmbracoOptions<CoreDebugSettings>()
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
            .AddUmbracoOptions<LegacyPasswordMigrationSettings>()
            .AddUmbracoOptions<PackageMigrationSettings>()
            .AddUmbracoOptions<ContentDashboardSettings>()
            .AddUmbracoOptions<HelpPageSettings>()
            .AddUmbracoOptions<DataTypesSettings>()
            .AddUmbracoOptions<WebhookSettings>();

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

        // TODO: Remove this in V12
        // This is to make the move of the AllowEditInvariantFromNonDefault setting from SecuritySettings to ContentSettings backwards compatible
        // If there is a value in security settings, but no value in content setting we'll use that value, otherwise content settings always wins.
        builder.Services.Configure<ContentSettings>(settings =>
        {
            var securitySettingsValue = builder.Config.GetSection($"{Constants.Configuration.ConfigSecurity}").GetValue<bool?>(nameof(SecuritySettings.AllowEditInvariantFromNonDefault));
            var contentSettingsValue = builder.Config.GetSection($"{Constants.Configuration.ConfigContent}").GetValue<bool?>(nameof(ContentSettings.AllowEditInvariantFromNonDefault));

            if (securitySettingsValue is not null && contentSettingsValue is null)
            {
                settings.AllowEditInvariantFromNonDefault = securitySettingsValue.Value;
            }
        });

        // TODO: Remove this in V13
        // This is to avoid a breaking change in ContentSettings, if the old AllowedFileUploads has a value, and the new
        // AllowedFileUploadExtensions does not, copy the value over, if the new has a value, use that instead.
        builder.Services.Configure<ContentSettings>(settings =>
        {
            // We have to use Config.GetSection().Get<string[]>, as the GetSection.GetValue<string[]> simply cannot retrieve a string array
            var allowedUploadedFileExtensionsValue = builder.Config.GetSection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.AllowedUploadedFileExtensions)}").Get<string[]>();
            var allowedUploadFilesValue = builder.Config.GetSection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.AllowedUploadFiles)}").Get<string[]>();

            if (allowedUploadedFileExtensionsValue is null && allowedUploadFilesValue is not null)
            {
                settings.AllowedUploadedFileExtensions = allowedUploadFilesValue;
            }
        });

        // TODO: Remove this in V13
        builder.Services.Configure<ContentSettings>(settings =>
        {
            var disallowedUploadedFileExtensionsValue = builder.Config.GetSection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.DisallowedUploadedFileExtensions)}").Get<string[]>();
            var disallowedUploadFilesValue = builder.Config.GetSection($"{Constants.Configuration.ConfigContent}:{nameof(ContentSettings.DisallowedUploadFiles)}").Get<string[]>();

            if (disallowedUploadedFileExtensionsValue is null && disallowedUploadFilesValue is not null)
            {
                settings.DisallowedUploadedFileExtensions = disallowedUploadFilesValue;
            }
        });
        return builder;
    }
}
