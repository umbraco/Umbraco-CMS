using System;
using System.IO;
using System.Linq;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Composing.Composers
{
    public static class ServicesComposer
    {
        public static IServiceRegistry ComposeServices(this IServiceRegistry registry)
        {
            // register a transient messages factory, which will be replaced by the web
            // boot manager when running in a web context
            registry.RegisterSingleton<IEventMessagesFactory, TransientEventMessagesFactory>();

            // register the service context
            registry.RegisterSingleton<ServiceContext>();

            // register the special idk map
            registry.RegisterSingleton<IdkMap>();

            // register the services
            registry.RegisterSingleton<IKeyValueService, KeyValueService>();
            registry.RegisterSingleton<IPublicAccessService, PublicAccessService>();
            registry.RegisterSingleton<ITaskService, TaskService>();
            registry.RegisterSingleton<IDomainService, DomainService>();
            registry.RegisterSingleton<IAuditService, AuditService>();
            registry.RegisterSingleton<ITagService, TagService>();
            registry.RegisterSingleton<IContentService, ContentService>();
            registry.RegisterSingleton<IUserService, UserService>();
            registry.RegisterSingleton<IMemberService, MemberService>();
            registry.RegisterSingleton<IMediaService, MediaService>();
            registry.RegisterSingleton<IContentTypeService, ContentTypeService>();
            registry.RegisterSingleton<IMediaTypeService, MediaTypeService>();
            registry.RegisterSingleton<IDataTypeService, DataTypeService>();
            registry.RegisterSingleton<IFileService, FileService>();
            registry.RegisterSingleton<ILocalizationService, LocalizationService>();
            registry.RegisterSingleton<IPackagingService, PackagingService>();
            registry.RegisterSingleton<IServerRegistrationService, ServerRegistrationService>();
            registry.RegisterSingleton<IEntityService, EntityService>();
            registry.RegisterSingleton<IRelationService, RelationService>();
            registry.RegisterSingleton<IMacroService, MacroService>();
            registry.RegisterSingleton<IMemberTypeService, MemberTypeService>();
            registry.RegisterSingleton<IMemberGroupService, MemberGroupService>();
            registry.RegisterSingleton<INotificationService, NotificationService>();
            registry.RegisterSingleton<IExternalLoginService, ExternalLoginService>();
            registry.RegisterSingleton<IRedirectUrlService, RedirectUrlService>();
            registry.RegisterSingleton<IConsentService, ConsentService>();
            registry.Register<LocalizedTextServiceFileSources>(factory =>
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
                    factory.GetInstance<ILogger>(),
                    factory.GetInstance<CacheHelper>().RuntimeCache,
                    mainLangFolder,
                    pluginLangFolders.Concat(userLangFolders));
            });
            registry.RegisterSingleton<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetInstance<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetInstance<ILogger>()));

            //TODO: These are replaced in the web project - we need to declare them so that
            // something is wired up, just not sure this is very nice but will work for now.
            registry.RegisterSingleton<IApplicationTreeService, EmptyApplicationTreeService>();
            registry.RegisterSingleton<ISectionService, EmptySectionService>();

            return registry;
        }
    }
}
