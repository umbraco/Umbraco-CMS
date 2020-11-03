using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
            composition.Services.AddUnique<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            composition.Services.AddUnique<ServiceContext>();

            // register the special idk map
            composition.Services.AddUnique<IIdKeyMap, IdKeyMap>();

            // register the services
            composition.Services.AddUnique<IPropertyValidationService, PropertyValidationService>();
            composition.Services.AddUnique<IKeyValueService, KeyValueService>();
            composition.Services.AddUnique<IPublicAccessService, PublicAccessService>();
            composition.Services.AddUnique<IDomainService, DomainService>();
            composition.Services.AddUnique<IAuditService, AuditService>();
            composition.Services.AddUnique<ITagService, TagService>();
            composition.Services.AddUnique<IContentService, ContentService>();
            composition.Services.AddUnique<IUserService, UserService>();
            composition.Services.AddUnique<IMemberService, MemberService>();
            composition.Services.AddUnique<IMediaService, MediaService>();
            composition.Services.AddUnique<IContentTypeService, ContentTypeService>();
            composition.Services.AddUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            composition.Services.AddUnique<IMediaTypeService, MediaTypeService>();
            composition.Services.AddUnique<IDataTypeService, DataTypeService>();
            composition.Services.AddUnique<IFileService, FileService>();
            composition.Services.AddUnique<ILocalizationService, LocalizationService>();
            composition.Services.AddUnique<IPackagingService, PackagingService>();
            composition.Services.AddUnique<IServerRegistrationService, ServerRegistrationService>();
            composition.Services.AddUnique<IEntityService, EntityService>();
            composition.Services.AddUnique<IRelationService, RelationService>();
            composition.Services.AddUnique<IMacroService, MacroService>();
            composition.Services.AddUnique<IMemberTypeService, MemberTypeService>();
            composition.Services.AddUnique<IMemberGroupService, MemberGroupService>();
            composition.Services.AddUnique<INotificationService, NotificationService>();
            composition.Services.AddUnique<IExternalLoginService, ExternalLoginService>();
            composition.Services.AddUnique<IRedirectUrlService, RedirectUrlService>();
            composition.Services.AddUnique<IConsentService, ConsentService>();
            composition.Services.AddTransient<LocalizedTextServiceFileSources>(SourcesFactory);
            composition.Services.AddUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            composition.Services.AddUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            composition.Services.AddUnique<IPackageActionRunner, PackageActionRunner>();

            composition.Services.AddUnique<ConflictingPackageData>();
            composition.Services.AddUnique<CompiledPackageXmlParser>();
            composition.Services.AddUnique<ICreatedPackagesRepository>(factory => CreatePackageRepository(factory, "createdPackages.config"));
            composition.Services.AddUnique<IInstalledPackagesRepository>(factory => CreatePackageRepository(factory, "installedPackages.config"));
            composition.Services.AddUnique<PackageDataInstallation>();
            composition.Services.AddUnique<PackageFileInstallation>();
            composition.Services.AddUnique<IPackageInstallation, PackageInstallation>();

            return composition;
        }

        /// <summary>
        /// Creates an instance of PackagesRepository for either the ICreatedPackagesRepository or the IInstalledPackagesRepository
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="packageRepoFileName"></param>
        /// <returns></returns>
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
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IUmbracoVersion>(),
                factory.GetRequiredService<IOptions<GlobalSettings>>(),
                packageRepoFileName);

        private static LocalizedTextServiceFileSources SourcesFactory(IServiceProvider container)
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
