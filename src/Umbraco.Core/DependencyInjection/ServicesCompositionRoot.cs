using System.Collections.Generic;
using LightInject;
using Umbraco.Core.Events;
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
            container.RegisterSingleton<ILocalizedTextService, LocalizedTextService>();
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
            container.RegisterSingleton<INotificationService, NotificationService>();
            container.RegisterSingleton<IExternalLoginService, ExternalLoginService>();
            //TODO: These are replaced in the web project - we need to declare them so that 
            // something is wired up, just not sure this is very nice but will work for now.
            container.RegisterSingleton<IApplicationTreeService, EmptyApplicationTreeService>();
            container.RegisterSingleton<ISectionService, EmptySectionService>();
        }
    }
}