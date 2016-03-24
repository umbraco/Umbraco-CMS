using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using LightInject;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.DependencyInjection
{
    public sealed class ServicesCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.RegisterSingleton<IPublishingStrategy, PublishingStrategy>();

            //These will be replaced by the web boot manager when running in a web context
            container.Register<IEventMessagesFactory, TransientMessagesFactory>();
            
            //the context
            container.RegisterSingleton<ServiceContext>();
            
            //now the services...
            container.RegisterSingleton<IMigrationEntryService, MigrationEntryService>();
            container.RegisterSingleton<IPublicAccessService, PublicAccessService>();
            container.RegisterSingleton<ITaskService, TaskService>();
            container.RegisterSingleton<IDomainService, DomainService>();
            container.RegisterSingleton<IAuditService, AuditService>();            
            container.RegisterSingleton<ITagService, TagService>();
            container.RegisterSingleton<IContentService, ContentService>();
            container.RegisterSingleton<IUserService, UserService>();
            container.RegisterSingleton<IMemberService, MemberService>();
            container.RegisterSingleton<IMediaService, MediaService>();
            container.RegisterSingleton<IContentTypeService, ContentTypeService>();
            container.RegisterSingleton<IDataTypeService, DataTypeService>();
            container.RegisterSingleton<IFileService, FileService>();
            container.RegisterSingleton<ILocalizationService, LocalizationService>();
            container.RegisterSingleton<IPackagingService, PackagingService>();
            container.RegisterSingleton<IServerRegistrationService, ServerRegistrationService>();
            container.RegisterSingleton<IEntityService, EntityService>();
            container.RegisterSingleton<IRelationService, RelationService>();            
            container.RegisterSingleton<IMacroService, MacroService>();
            container.RegisterSingleton<IMemberTypeService, MemberTypeService>();
            container.RegisterSingleton<IMemberGroupService, MemberGroupService>();
            container.RegisterSingleton<INotificationService, NotificationService>();
            container.RegisterSingleton<IExternalLoginService, ExternalLoginService>();
            container.Register<LocalizedTextServiceFileSources>(factory =>
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
            container.RegisterSingleton<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetInstance<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetInstance<ILogger>()));

            //TODO: These are replaced in the web project - we need to declare them so that 
            // something is wired up, just not sure this is very nice but will work for now.
            container.RegisterSingleton<IApplicationTreeService, EmptyApplicationTreeService>();
            container.RegisterSingleton<ISectionService, EmptySectionService>();
        }
    }
}