using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;
using Umbraco.Cms.Infrastructure.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Umbraco services
        /// </summary>
        internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
        {
            // register the service context
            builder.Services.AddSingleton<ServiceContext>();

            // register the special idk map
            builder.Services.AddUnique<IIdKeyMap, IdKeyMap>();

            // register the services
            builder.Services.AddUnique<IPropertyValidationService, PropertyValidationService>();
            builder.Services.AddUnique<IKeyValueService, KeyValueService>();
            builder.Services.AddUnique<IPublicAccessService, PublicAccessService>();
            builder.Services.AddUnique<IDomainService, DomainService>();
            builder.Services.AddUnique<IAuditService, AuditService>();
            builder.Services.AddUnique<ICacheInstructionService, CacheInstructionService>();
            builder.Services.AddUnique<IBasicAuthService, BasicAuthService>();
            builder.Services.AddUnique<ITagService, TagService>();
            builder.Services.AddUnique<IContentService, ContentService>();
            builder.Services.AddUnique<IContentVersionService, ContentVersionService>();
            builder.Services.AddUnique<IContentVersionCleanupPolicy, DefaultContentVersionCleanupPolicy>();
            builder.Services.AddUnique<IUserService, UserService>();
            builder.Services.AddUnique<IMemberService, MemberService>();
            builder.Services.AddUnique<IMediaService, MediaService>();
            builder.Services.AddUnique<IContentTypeService, ContentTypeService>();
            builder.Services.AddUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            builder.Services.AddUnique<IMediaTypeService, MediaTypeService>();
            builder.Services.AddUnique<IDataTypeService, DataTypeService>();
            builder.Services.AddUnique<IFileService, FileService>();
            builder.Services.AddUnique<ILocalizationService, LocalizationService>();
            builder.Services.AddUnique<IPackagingService, PackagingService>();
            builder.Services.AddUnique<IServerRegistrationService, ServerRegistrationService>();
            builder.Services.AddUnique<IEntityService, EntityService>();
            builder.Services.AddUnique<IRelationService, RelationService>();
            builder.Services.AddUnique<ITrackedReferencesService, TrackedReferencesService>();
            builder.Services.AddUnique<IMacroService, MacroService>();
            builder.Services.AddUnique<IMemberTypeService, MemberTypeService>();
            builder.Services.AddUnique<IMemberGroupService, MemberGroupService>();
            builder.Services.AddUnique<INotificationService, NotificationService>();
            builder.Services.AddUnique<ExternalLoginService>(factory => new ExternalLoginService(
                factory.GetRequiredService<IScopeProvider>(),
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IEventMessagesFactory>(),
                factory.GetRequiredService<IExternalLoginWithKeyRepository>()
                ));
            builder.Services.AddUnique<IExternalLoginService>(factory => factory.GetRequiredService<ExternalLoginService>());
            builder.Services.AddUnique<IExternalLoginWithKeyService>(factory => factory.GetRequiredService<ExternalLoginService>());
            builder.Services.AddUnique<ITwoFactorLoginService, TwoFactorLoginService>();
            builder.Services.AddUnique<IRedirectUrlService, RedirectUrlService>();
            builder.Services.AddUnique<IConsentService, ConsentService>();
            builder.Services.AddTransient(SourcesFactory);
            builder.Services.AddUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            builder.Services.AddUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            builder.Services.AddSingleton<ConflictingPackageData>();
            builder.Services.AddSingleton<CompiledPackageXmlParser>();
            builder.Services.AddUnique(factory => CreatePackageRepository(factory, "createdPackages.config"));
            builder.Services.AddUnique<ICreatedPackagesRepository, CreatedPackageSchemaRepository>();
            builder.Services.AddSingleton<PackageDataInstallation>();
            builder.Services.AddUnique<IPackageInstallation, PackageInstallation>();
            builder.Services.AddUnique<IHtmlMacroParameterParser, HtmlMacroParameterParser>();
            builder.Services.AddTransient<IExamineIndexCountService, ExamineIndexCountService>();
            builder.Services.AddUnique<IUserDataService, SystemInformationTelemetryProvider>();
            builder.Services.AddTransient<IUsageInformationService, UsageInformationService>();

            return builder;
        }

        private static PackagesRepository CreatePackageRepository(IServiceProvider factory, string packageRepoFileName)
            => new PackagesRepository(
                factory.GetRequiredService<IContentService>(),
                factory.GetRequiredService<IContentTypeService>(),
                factory.GetRequiredService<IDataTypeService>(),
                factory.GetRequiredService<IFileService>(),
                factory.GetRequiredService<IMacroService>(),
                factory.GetRequiredService<ILocalizationService>(),
                factory.GetRequiredService<IHostingEnvironment>(),
                factory.GetRequiredService<IEntityXmlSerializer>(),
                factory.GetRequiredService<IOptions<GlobalSettings>>(),
                factory.GetRequiredService<IMediaService>(),
                factory.GetRequiredService<IMediaTypeService>(),
                factory.GetRequiredService<MediaFileManager>(),
                factory.GetRequiredService<FileSystems>(),
                packageRepoFileName);

        private static LocalizedTextServiceFileSources SourcesFactory(IServiceProvider container)
        {
            var hostingEnvironment = container.GetRequiredService<IHostingEnvironment>();
            var globalSettings = container.GetRequiredService<IOptions<GlobalSettings>>().Value;
            var mainLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(Constants.SystemDirectories.Umbraco, "config", "lang")));
            var appPlugins = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins));
            var configLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(Constants.SystemDirectories.Config, "lang")));

            var pluginLangFolders = appPlugins.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : appPlugins.GetDirectories()
                    // Check for both Lang & lang to support case sensitive file systems.
                    .SelectMany(x => x.GetDirectories("?ang", SearchOption.AllDirectories).Where(x => x.Name.InvariantEquals("lang")))
                    .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, false));

            // user defined langs that overwrite the default, these should not be used by plugin creators
            var userLangFolders = configLangFolder.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : configLangFolder
                    .GetFiles("*.user.xml", SearchOption.TopDirectoryOnly)
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return new LocalizedTextServiceFileSources(
                container.GetRequiredService<ILogger<LocalizedTextServiceFileSources>>(),
                container.GetRequiredService<AppCaches>(),
                mainLangFolder,
                pluginLangFolders.Concat(userLangFolders));
        }
    }
}
