using System;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Plugins;

namespace Umbraco.Core.DI
{
    /// <summary>
    /// Sets the IoC container for the umbraco data layer/repositories/sql/database/etc...
    /// </summary>
    public sealed class RepositoryCompositionRoot : ICompositionRoot
    {
        public const string DisabledCache = "DisabledCache";

        public void Compose(IServiceRegistry container)
        {
            // register database context
            container.RegisterSingleton<DatabaseContext>();

            // register IUnitOfWork providers
            container.RegisterSingleton<IUnitOfWorkProvider, FileUnitOfWorkProvider>();
            container.RegisterSingleton<IDatabaseUnitOfWorkProvider, NPocoUnitOfWorkProvider>();

            // register query factory
            container.RegisterSingleton<IQueryFactory, QueryFactory>();

            // register repository factory
            container.RegisterSingleton<RepositoryFactory>();

            // register cache helpers
            // the main cache helper is registered by CoreBootManager and is used by most repositories
            // the disabled one is used by those repositories that have an annotated ctor parameter
            container.RegisterSingleton(factory => CacheHelper.CreateDisabledCacheHelper(), DisabledCache);

            // resolve ctor dependency from GetInstance() runtimeArguments, if possible - 'factory' is
            // the container, 'info' describes the ctor argument, and 'args' contains the args that
            // were passed to GetInstance() - use first arg if it is the right type,
            //
            // for IDatabaseUnitOfWork
            container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as IDatabaseUnitOfWork : null);
            // for IUnitOfWork
            container.RegisterConstructorDependency((factory, info, args) => args.Length > 0 ? args[0] as IUnitOfWork : null);

            // register repositories
            // repos depend on various things, and a IDatabaseUnitOfWork (registered above)
            // some repositories have an annotated ctor parameter to pick the right cache helper

            // repositories
            container.Register<INotificationsRepository, NotificationsRepository>();
            container.Register<IExternalLoginRepository, ExternalLoginRepository>();
            container.Register<IPublicAccessRepository, PublicAccessRepository>();
            container.Register<ITagRepository, TagRepository>();
            container.Register<IContentRepository, ContentRepository>();
            container.Register<IContentTypeRepository, ContentTypeRepository>();
            container.Register<IDataTypeDefinitionRepository, DataTypeDefinitionRepository>();
            container.Register<IDictionaryRepository, DictionaryRepository>();
            container.Register<ILanguageRepository, LanguageRepository>();
            container.Register<IMediaRepository, MediaRepository>();
            container.Register<IMediaTypeRepository, MediaTypeRepository>();
            container.Register<ITemplateRepository, TemplateRepository>();
            container.Register<IUserTypeRepository, UserTypeRepository>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IMacroRepository, MacroRepository>();
            container.Register<IMemberRepository, MemberRepository>();
            container.Register<IMemberTypeRepository, MemberTypeRepository>();
            container.Register<IMemberGroupRepository, MemberGroupRepository>();
            container.Register<IEntityRepository, EntityRepository>();
            container.Register<IDomainRepository, DomainRepository>();
            container.Register<ITaskRepository, TaskRepository>();
            container.Register<ITaskTypeRepository,TaskTypeRepository>();
            container.Register<IAuditRepository, AuditRepository>();
            container.Register<IRelationRepository, RelationRepository>();
            container.Register<IRelationTypeRepository, RelationTypeRepository>();
            container.Register<IMigrationEntryRepository, MigrationEntryRepository>();
            container.Register<IServerRegistrationRepository, ServerRegistrationRepository>();
            container.Register<IDocumentTypeContainerRepository, DocumentTypeContainerRepository>();
            container.Register<IMediaTypeContainerRepository, MediaTypeContainerRepository>();
            container.Register<IDataTypeContainerRepository, DataTypeContainerRepository>();
            container.Register<IRedirectUrlRepository, RedirectUrlRepository>();

            // repositories that depend on a filesystem
            // these have an annotated ctor parameter to pick the right file system
            container.Register<IScriptRepository, ScriptRepository>();
            container.Register<IPartialViewRepository, PartialViewRepository>();
            container.Register<IPartialViewMacroRepository, PartialViewMacroRepository>();
            container.Register<IStylesheetRepository, StylesheetRepository>();

            // collection builders require a full IServiceContainer because they need to
            // be able to both register stuff, and get stuff - but ICompositionRoot gives
            // us an IServiceRegistry - which *is* a full container - so, casting - bit
            // awkward but it works
            var serviceContainer = container as IServiceContainer;
            if (serviceContainer == null) throw new Exception("Container is not IServiceContainer.");
        }
    }
}