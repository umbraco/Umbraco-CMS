using System;
using LightInject;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Sets the IoC container for the umbraco data layer/repositories/sql/database/etc...
    /// </summary>
    public sealed class RepositoryCompositionRoot : ICompositionRoot
    {
        public const string DisabledCache = "DisabledCache";

        public void Compose(IServiceRegistry container)
        {
            // register syntax providers
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            container.RegisterSingleton<IScopeContextFactory, DefaultScopeContextFactory>();

            // register database factory
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until the database context configures it properly (eg when installing)
            container.RegisterSingleton<IDatabaseFactory, DefaultDatabaseFactory>();

            // register database context
            container.RegisterSingleton<DatabaseContext>();

            // register IUnitOfWork providers
            // using a factory for NPocoUnitOfWorkProvider because it has another ctor accepting
            // one single parameter - we should get rid of it but it is used hundreds of times in
            // unit tests and I am lazy
            container.RegisterSingleton<IUnitOfWorkProvider, FileUnitOfWorkProvider>();
            container.RegisterSingleton<IDatabaseUnitOfWorkProvider>(factory => new NPocoUnitOfWorkProvider(factory.GetInstance<IDatabaseFactory>()));

            // register mapping resover
            // using a factory because... no time to clean it up at the moment
            container.RegisterSingleton<IMappingResolver>(factory => new MappingResolver(
                factory.GetInstance<IServiceContainer>(),
                factory.GetInstance<ILogger>(),
                () => factory.GetInstance<PluginManager>().ResolveAssignedMapperTypes()));

            // register repository factory
            container.RegisterSingleton<RepositoryFactory>();

            // register file systems
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Scripts), "ScriptFileSystem");
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews + "/Partials/"), "PartialViewFileSystem");
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews + "/MacroPartials/"), "PartialViewMacroFileSystem");
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Css), "StylesheetFileSystem");
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Masterpages), "MasterpageFileSystem");
            container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews), "ViewFileSystem");

            // register cache helpers
            // the main cache helper is registered by CoreBootManager and is used by most repositories
            // the disabled one is used by those repositories that have an annotated ctor parameter
            container.RegisterSingleton(factory => CacheHelper.CreateDisabledCacheHelper(), DisabledCache);

            // register IDatabaseUnitOfWork
            // resolve ctor dependency from GetInstance() runtimeArguments if possible
            container.RegisterConstructorDependency((factory, info, runtimeArguments) =>
                runtimeArguments.Length > 0 ? runtimeArguments[0] as IDatabaseUnitOfWork : null);

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
            container.Register<EntityContainerRepository, EntityContainerRepository>();
            container.Register<ITaskRepository, TaskRepository>();
            container.Register<ITaskTypeRepository,TaskTypeRepository>();
            container.Register<IAuditRepository, AuditRepository>();
            container.Register<IRelationRepository, RelationRepository>();
            container.Register<IRelationTypeRepository, RelationTypeRepository>();
            container.Register<IMigrationEntryRepository, MigrationEntryRepository>();
            container.Register<IServerRegistrationRepository, ServerRegistrationRepository>();

            // repositories that depend on a filesystem
            // these have an annotated ctor parameter to pick the right file system
            container.Register<IScriptRepository, ScriptRepository>();
            container.Register<IPartialViewRepository, PartialViewRepository>("PartialViewRepository");
            container.Register<IPartialViewRepository, PartialViewMacroRepository>("PartialViewMacroRepository");
            container.Register<IStylesheetRepository, StylesheetRepository>();
        }
    }
}