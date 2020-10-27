using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Packaging;
using Umbraco.Core.Routing;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Composing.CompositionExtensions
{
    internal static class Services
    {
        public static Composition ComposeServices(this Composition composition)
        {
            // register a transient messages factory, which will be replaced by the web
            // boot manager when running in a web context
            composition.RegisterUnique<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            composition.RegisterUnique<ServiceContext>();

            // register the special idk map
            composition.RegisterUnique<IIdKeyMap, IdKeyMap>();

            // register the services
            composition.RegisterUnique<IPropertyValidationService, PropertyValidationService>();
            composition.RegisterUnique<IKeyValueService, KeyValueService>();
            composition.RegisterUnique<IPublicAccessService, PublicAccessService>();
            composition.RegisterUnique<IDomainService, DomainService>();
            composition.RegisterUnique<IAuditService, AuditService>();
            composition.RegisterUnique<ITagService, TagService>();
            composition.RegisterUnique<IContentService, ContentService>();
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
            composition.Register<LocalizedTextServiceFileSources>(SourcesFactory);
            composition.RegisterUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            composition.RegisterUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            composition.RegisterUnique<IPackageActionRunner, PackageActionRunner>();

            composition.RegisterUnique<ConflictingPackageData>();
            composition.RegisterUnique<CompiledPackageXmlParser>();
            composition.RegisterUnique<ICreatedPackagesRepository>(factory => CreatePackageRepository(factory, "createdPackages.config"));
            composition.RegisterUnique<IInstalledPackagesRepository>(factory => CreatePackageRepository(factory, "installedPackages.config"));
            composition.RegisterUnique<PackageDataInstallation>();
            composition.RegisterUnique<PackageFileInstallation>();
            composition.RegisterUnique<IPackageInstallation, PackageInstallation>();

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
                factory.GetRequiredService<IContentService>(),
                factory.GetRequiredService<IContentTypeService>(),
                factory.GetRequiredService<IDataTypeService>(),
                factory.GetRequiredService<IFileService>(),
                factory.GetRequiredService<IMacroService>(),
                factory.GetRequiredService<ILocalizationService>(),
                factory.GetRequiredService<IHostingEnvironment>(),
                factory.GetRequiredService<IEntityXmlSerializer>(),
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IUmbracoVersion>(),
                factory.GetRequiredService<IOptions<GlobalSettings>>(),
                packageRepoFileName);

        private static LocalizedTextServiceFileSources SourcesFactory(IFactory container)
        {
            var hostingEnvironment = container.GetRequiredService<IHostingEnvironment>();
            var globalSettings = container.GetRequiredService<IOptions<GlobalSettings>>().Value;
            var mainLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(globalSettings.UmbracoPath , "config","lang")));
            var appPlugins = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins));
            var configLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(WebPath.Combine(Constants.SystemDirectories.Config  ,"lang")));

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
                container.GetRequiredService<ILogger<LocalizedTextServiceFileSources>>(),
                container.GetRequiredService<AppCaches>(),
                mainLangFolder,
                pluginLangFolders.Concat(userLangFolders));
        }
    }
}
