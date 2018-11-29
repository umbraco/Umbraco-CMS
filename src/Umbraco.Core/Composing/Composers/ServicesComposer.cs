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
            composition.RegisterUnique<IContentService, ContentService>();
            composition.RegisterUnique<IUserService, UserService>();
            composition.RegisterUnique<IMemberService, MemberService>();
            composition.RegisterUnique<IMediaService, MediaService>();
            composition.RegisterUnique<IContentTypeService, ContentTypeService>();
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
                factory.GetInstance<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetInstance<ILogger>()));

            //TODO: These are replaced in the web project - we need to declare them so that
            // something is wired up, just not sure this is very nice but will work for now.
            composition.RegisterUnique<IApplicationTreeService, EmptyApplicationTreeService>();
            composition.RegisterUnique<ISectionService, EmptySectionService>();

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
