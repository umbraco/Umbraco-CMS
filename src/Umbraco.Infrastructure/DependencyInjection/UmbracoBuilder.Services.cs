using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;
using Umbraco.Cms.Infrastructure.Templates.PartialViews;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco services
    /// </summary>
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        // register the service context
        builder.Services.AddSingleton<ServiceContext>();

        // register the special idk map
        builder.Services.AddUnique<IIdKeyMap, IdKeyMap>();
        builder.Services.AddUnique<IUserIdKeyResolver, UserIdKeyResolver>();

        builder.Services.AddUnique<IAuditService, AuditService>();
        builder.Services.AddUnique<ICacheInstructionService, CacheInstructionService>();
        builder.Services.AddUnique<IBasicAuthService, BasicAuthService>();
        builder.Services.AddUnique<IDataTypeService, DataTypeService>();
        builder.Services.AddUnique<IPackagingService, PackagingService>();
        builder.Services.AddUnique<IServerInformationService, ServerInformationService>();
        builder.Services.AddUnique<IServerRegistrationService, ServerRegistrationService>();
        builder.Services.AddTransient(CreateLocalizedTextServiceFileSourcesFactory);
        builder.Services.AddUnique(factory => CreatePackageRepository(factory, "createdPackages.config"));
        builder.Services.AddUnique<ICreatedPackagesRepository>(factory
            => new CreatedPackageSchemaRepository(
                factory.GetRequiredService<IHostingEnvironment>(),
                factory.GetRequiredService<FileSystems>(),
                factory.GetRequiredService<IEntityXmlSerializer>(),
                factory.GetRequiredService<IDataTypeService>(),
                factory.GetRequiredService<IFileService>(),
                factory.GetRequiredService<IMediaService>(),
                factory.GetRequiredService<IMediaTypeService>(),
                factory.GetRequiredService<IContentService>(),
                factory.GetRequiredService<MediaFileManager>(),
                factory.GetRequiredService<IContentTypeService>(),
                factory.GetRequiredService<IScopeAccessor>(),
                factory.GetRequiredService<ITemplateService>(),
                factory.GetRequiredService<IDictionaryItemService>(),
                factory.GetRequiredService<ILanguageService>()));
        builder.Services.AddSingleton(CreatePackageDataInstallation);
        builder.Services.AddUnique<IPackageInstallation, PackageInstallation>();
        builder.Services.AddTransient<IExamineIndexCountService, ExamineIndexCountService>();
        builder.Services.AddUnique<IUserDataService, UserDataService>();
        builder.Services.AddUnique<ISystemTroubleshootingInformationService, SystemTroubleshootingInformationTelemetryProvider>();
        builder.Services.AddTransient<IUsageInformationService, UsageInformationService>();
        builder.Services.AddTransient<IPartialViewPopulator, PartialViewPopulator>();
        builder.Services.AddUnique<IContentListViewService, ContentListViewService>();
        builder.Services.AddUnique<IMediaListViewService, MediaListViewService>();
        builder.Services.AddUnique<IEntitySearchService, EntitySearchService>();
        builder.Services.AddUnique<IContentTypeSearchService, ContentTypeSearchService>();
        builder.Services.AddUnique<IIndexedEntitySearchService, IndexedEntitySearchService>();
        builder.Services.TryAddTransient<IReservedFieldNamesService, ReservedFieldNamesService>();

        return builder;
    }

    private static PackagesRepository CreatePackageRepository(IServiceProvider factory, string packageRepoFileName)
        => new(
            factory.GetRequiredService<IContentService>(),
            factory.GetRequiredService<IContentTypeService>(),
            factory.GetRequiredService<IDataTypeService>(),
            factory.GetRequiredService<IFileService>(),
            factory.GetRequiredService<ILocalizationService>(),
            factory.GetRequiredService<IHostingEnvironment>(),
            factory.GetRequiredService<IEntityXmlSerializer>(),
            factory.GetRequiredService<IOptions<GlobalSettings>>(),
            factory.GetRequiredService<IMediaService>(),
            factory.GetRequiredService<IMediaTypeService>(),
            factory.GetRequiredService<MediaFileManager>(),
            factory.GetRequiredService<FileSystems>(),
            packageRepoFileName);

    // Factory registration is only required because of ambiguous constructor
    private static IPackageDataInstallation CreatePackageDataInstallation(IServiceProvider factory)
        => new PackageDataInstallation(
            factory.GetRequiredService<IDataValueEditorFactory>(),
            factory.GetRequiredService<ILogger<PackageDataInstallation>>(),
            factory.GetRequiredService<IFileService>(),
            factory.GetRequiredService<ILocalizationService>(),
            factory.GetRequiredService<IDataTypeService>(),
            factory.GetRequiredService<IEntityService>(),
            factory.GetRequiredService<IContentTypeService>(),
            factory.GetRequiredService<IContentService>(),
            factory.GetRequiredService<PropertyEditorCollection>(),
            factory.GetRequiredService<IScopeProvider>(),
            factory.GetRequiredService<IShortStringHelper>(),
            factory.GetRequiredService<IConfigurationEditorJsonSerializer>(),
            factory.GetRequiredService<IMediaService>(),
            factory.GetRequiredService<IMediaTypeService>(),
            factory.GetRequiredService<ITemplateContentParserService>(),
            factory.GetRequiredService<ITemplateService>());

    private static LocalizedTextServiceFileSources CreateLocalizedTextServiceFileSourcesFactory(
        IServiceProvider container)
    {
        IHostingEnvironment hostingEnvironment = container.GetRequiredService<IHostingEnvironment>();

        // TODO: (for >= v13) Rethink whether all language files (.xml and .user.xml) should be located in ~/config/lang
        // instead of ~/umbraco/config/lang and ~/config/lang.
        // Currently when extending Umbraco, a new language file that the backoffice will be available in, should be placed
        // in ~/umbraco/config/lang, while 'user' translation files for overrides are in ~/config/lang (according to our docs).
        // Such change will be breaking and we would need to document this clearly.
        var subPath = WebPath.Combine(Constants.SystemDirectories.Umbraco, "config", "lang");

        var mainLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(subPath));

        return new LocalizedTextServiceFileSources(
            container.GetRequiredService<ILogger<LocalizedTextServiceFileSources>>(),
            container.GetRequiredService<AppCaches>(),
            mainLangFolder,
            container.GetServices<LocalizedTextServiceSupplementaryFileSource>(),
            new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Lang")
                .GetDirectoryContents(string.Empty));
    }
}
