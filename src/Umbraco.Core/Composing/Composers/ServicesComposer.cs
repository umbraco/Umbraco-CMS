using System;
using System.IO;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Composing.Composers
{
    public static class ServicesComposer
    {
        public static Composition ComposeServices(this Composition composition)
        {
            // register a transient messages factory, which will be replaced by the web
            // boot manager when running in a web context
            composition.RegisterSingleton<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            composition.RegisterSingleton<ServiceContext>();

            // register the special idk map
            composition.RegisterSingleton<IdkMap>();

            // register the services
            composition.RegisterSingleton<IKeyValueService, KeyValueService>();
            composition.RegisterSingleton<IPublicAccessService, PublicAccessService>();
            composition.RegisterSingleton<IDomainService, DomainService>();
            composition.RegisterSingleton<IAuditService, AuditService>();
            composition.RegisterSingleton<ITagService, TagService>();
            composition.RegisterSingleton<IContentService, ContentService>();
            composition.RegisterSingleton<IUserService, UserService>();
            composition.RegisterSingleton<IMemberService, MemberService>();
            composition.RegisterSingleton<IMediaService, MediaService>();
            composition.RegisterSingleton<IContentTypeService, ContentTypeService>();
            composition.RegisterSingleton<IMediaTypeService, MediaTypeService>();
            composition.RegisterSingleton<IDataTypeService, DataTypeService>();
            composition.RegisterSingleton<IFileService, FileService>();
            composition.RegisterSingleton<ILocalizationService, LocalizationService>();
            composition.RegisterSingleton<IPackagingService, PackagingService>();
            composition.RegisterSingleton<IServerRegistrationService, ServerRegistrationService>();
            composition.RegisterSingleton<IEntityService, EntityService>();
            composition.RegisterSingleton<IRelationService, RelationService>();
            composition.RegisterSingleton<IMacroService, MacroService>();
            composition.RegisterSingleton<IMemberTypeService, MemberTypeService>();
            composition.RegisterSingleton<IMemberGroupService, MemberGroupService>();
            composition.RegisterSingleton<INotificationService, NotificationService>();
            composition.RegisterSingleton<IExternalLoginService, ExternalLoginService>();
            composition.RegisterSingleton<IRedirectUrlService, RedirectUrlService>();
            composition.RegisterSingleton<IConsentService, ConsentService>();
            composition.Register<LocalizedTextServiceFileSources>(SourcesFactory);
            composition.RegisterSingleton<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetInstance<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetInstance<ILogger>()));

            //TODO: These are replaced in the web project - we need to declare them so that
            // something is wired up, just not sure this is very nice but will work for now.
            composition.RegisterSingleton<IApplicationTreeService, EmptyApplicationTreeService>();
            composition.RegisterSingleton<ISectionService, EmptySectionService>();

            return composition;
        }

        private static LocalizedTextServiceFileSources SourcesFactory(IFactory container)
        {
            var mainLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Umbraco + "/config/lang/"));
            var appPlugins = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            var configLangFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config + "/lang/"));

            var pluginLangFolders = appPlugins.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : appPlugins.GetDirectories()
                    .SelectMany(x => x.GetDirectories("Lang"))
                    .SelectMany(x => x.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
                    .Where(x => Path.GetFileNameWithoutExtension(x.FullName).Length == 5)
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, false));

            //user defined langs that overwrite the default, these should not be used by plugin creators
            var userLangFolders = configLangFolder.Exists == false
                ? Enumerable.Empty<LocalizedTextServiceSupplementaryFileSource>()
                : configLangFolder
                    .GetFiles("*.user.xml", SearchOption.TopDirectoryOnly)
                    .Where(x => Path.GetFileNameWithoutExtension(x.FullName).Length == 10)
                    .Select(x => new LocalizedTextServiceSupplementaryFileSource(x, true));

            return new LocalizedTextServiceFileSources(
                container.GetInstance<ILogger>(),
                container.GetInstance<CacheHelper>().RuntimeCache,
                mainLangFolder,
                pluginLangFolders.Concat(userLangFolders));
        }
    }
}
