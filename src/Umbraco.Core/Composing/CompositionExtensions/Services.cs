using System;
using System.IO;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Telemetry;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    public static class Services
    {
        public static Composition ComposeServices(this Composition composition)
        {
            // register a transient messages factory, which will be replaced by the web
            // boot manager when running in a web context
            composition.RegisterUnique<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            composition.RegisterUnique<ServiceContext>();

            // register the special idk map
            composition.RegisterUnique<IdkMap>();

            // register the services
            composition.RegisterUnique<IKeyValueService, KeyValueService>();
            composition.RegisterUnique<IPublicAccessService, PublicAccessService>();
            composition.RegisterUnique<IDomainService, DomainService>();
            composition.RegisterUnique<IAuditService, AuditService>();
            composition.RegisterUnique<ITagService, TagService>();

            composition.RegisterUnique<ContentService>();
            composition.RegisterUnique<IContentService>(factory => factory.GetInstance<ContentService>());
            composition.RegisterUnique<IContentVersionService>(factory => factory.GetInstance<ContentService>());
            composition.RegisterUnique<IContentVersionCleanupPolicy, DefaultContentVersionCleanupPolicy>();

            composition.RegisterUnique<IUserService, UserService>();
            composition.RegisterUnique<IMemberService, MemberService>();
            composition.RegisterUnique<IMediaService, MediaService>();
            composition.RegisterUnique<IContentTypeService, ContentTypeService>();
            composition.RegisterUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            composition.RegisterUnique<IMediaTypeService, MediaTypeService>();
            composition.RegisterUnique<IDataTypeService, DataTypeService>();
            composition.RegisterUnique<IFileService, FileService>();
            composition.RegisterUnique<ILocalizationService, LocalizationService>();
            composition.RegisterUnique<IPackagingService, PackagingService>();
            composition.RegisterUnique<IServerRegistrationService, ServerRegistrationService>();
            composition.RegisterUnique<IEntityService, EntityService>();
            composition.RegisterUnique<IRelationService, RelationService>();
            composition.RegisterUnique<IMacroService, MacroService>();
            composition.RegisterUnique<IMemberTypeService, MemberTypeService>();
            composition.RegisterUnique<IMemberGroupService, MemberGroupService>();
            composition.RegisterUnique<INotificationService, NotificationService>();
            composition.RegisterUnique<IExternalLoginService, ExternalLoginService>();
            composition.RegisterUnique<IRedirectUrlService, RedirectUrlService>();
            composition.RegisterUnique<IConsentService, ConsentService>();
            composition.RegisterUnique<IEmailSender, EmailSender>();
            composition.Register<LocalizedTextServiceFileSources>(SourcesFactory);
            composition.RegisterUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetInstance<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetInstance<ILogger>()));

            composition.RegisterUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            composition.RegisterUnique<IPackageActionRunner, PackageActionRunner>();

            composition.RegisterUnique<ConflictingPackageData>();
            composition.RegisterUnique<CompiledPackageXmlParser>();
            composition.RegisterUnique<ICreatedPackagesRepository>(factory => CreatePackageRepository(factory, "createdPackages.config"));
            composition.RegisterUnique<IInstalledPackagesRepository>(factory => CreatePackageRepository(factory, "installedPackages.config"));
            composition.RegisterUnique<PackageDataInstallation>();
            composition.RegisterUnique<PackageFileInstallation>();
            composition.RegisterUnique<IPackageInstallation>(factory => //factory required because we need to pass in a string path
                new PackageInstallation(
                    factory.GetInstance<PackageDataInstallation>(), factory.GetInstance<PackageFileInstallation>(),
                    factory.GetInstance<CompiledPackageXmlParser>(), factory.GetInstance<IPackageActionRunner>(),
                    new DirectoryInfo(IOHelper.GetRootDirectorySafe())));

            composition.RegisterUnique<ITelemetryService, TelemetryService>();
            composition.RegisterUnique<IHtmlSanitizer, NoopHtmlSanitizer>();

            return composition;
        }

        /// <summary>
        /// Creates an instance of PackagesRepository for either the ICreatedPackagesRepository or the IInstalledPackagesRepository
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="packageRepoFileName"></param>
        /// <returns></returns>
        private static PackagesRepository CreatePackageRepository(IFactory factory, string packageRepoFileName)
            => new PackagesRepository(
                factory.GetInstance<IContentService>(), factory.GetInstance<IContentTypeService>(), factory.GetInstance<IDataTypeService>(), factory.GetInstance<IFileService>(), factory.GetInstance<IMacroService>(), factory.GetInstance<ILocalizationService>(), factory.GetInstance<IEntityXmlSerializer>(), factory.GetInstance<ILogger>(),
                packageRepoFileName);

        private static LocalizedTextServiceFileSources SourcesFactory(IFactory container)
        {
            var mainLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/config/lang/"));
            var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            var configLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config + "/lang/"));

            var pluginLangFolders = appPlugins.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : appPlugins.GetDirectories()
                    .SelectMany(x => x.GetDirectories("Lang", SearchOption.AllDirectories))
                    .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, false));

            //user defined langs that overwrite the default, these should not be used by plugin creators
            var userLangFolders = configLangFolder.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : configLangFolder
                    .GetFiles("*.user.xml", SearchOption.TopDirectoryOnly)
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return new LocalizedTextServiceFileSources(
                container.GetInstance<ILogger>(),
                container.GetInstance<AppCaches>(),
                mainLangFolder,
                pluginLangFolders.Concat(userLangFolders));
        }
    }
}
