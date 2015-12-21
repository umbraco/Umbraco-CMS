using System;
using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        public void Compose(IServiceRegistry container)
        {
            container.Register<IDatabaseFactory>(factory => new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, factory.GetInstance<ILogger>()), new PerContainerLifetime());
            container.Register<DatabaseContext>(factory => GetDbContext(factory), new PerContainerLifetime());
            container.Register<SqlSyntaxProviders>(factory => SqlSyntaxProviders.CreateDefault(factory.GetInstance<ILogger>()), new PerContainerLifetime());
            container.Register<IUnitOfWorkProvider, FileUnitOfWorkProvider>(new PerContainerLifetime());
            container.Register<IDatabaseUnitOfWorkProvider>(factory => new PetaPocoUnitOfWorkProvider(factory.GetInstance<ILogger>()), new PerContainerLifetime());
            container.Register<IMappingResolver>(factory => new MappingResolver(
                factory.GetInstance<IServiceContainer>(),
                factory.GetInstance<ILogger>(),
                () => factory.GetInstance<PluginManager>().ResolveAssignedMapperTypes()),
                new PerContainerLifetime());
            container.Register<RepositoryFactory>();
            container.Register<ISqlSyntaxProvider>(factory => factory.GetInstance<DatabaseContext>().SqlSyntax);
            container.Register<CacheHelper>(factory => CacheHelper.CreateDisabledCacheHelper(), "DisabledCache", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Scripts), "ScriptFileSystem", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews + "/Partials/"), "PartialViewFileSystem", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews + "/MacroPartials/"), "PartialViewMacroFileSystem", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Css), "StylesheetFileSystem", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.Masterpages), "MasterpageFileSystem", new PerContainerLifetime());
            container.Register<IFileSystem>(factory => new PhysicalFileSystem(SystemDirectories.MvcViews), "ViewFileSystem", new PerContainerLifetime());

            //Repository factories:
            //NOTE: Wondering if we can pass in parameters at resolution time with LightInject 
            // without having to manually specify the ctor for each one, have asked here: https://github.com/seesharper/LightInject/issues/237
            container.Register<IDatabaseUnitOfWork, INotificationsRepository>((factory, work) => new NotificationsRepository(work, factory.GetInstance<ISqlSyntaxProvider>()));
            container.Register<IDatabaseUnitOfWork, IExternalLoginRepository>((factory, work) => new ExternalLoginRepository(
                work,   
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IPublicAccessRepository>((factory, work) => new PublicAccessRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, ITaskRepository>((factory, work) => new TaskRepository(
                work,
                factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IAuditRepository>((factory, work) => new AuditRepository(
                work,
                factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, ITagRepository>((factory, work) => new TagRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IContentRepository>((factory, work) => new ContentRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IContentTypeRepository>>()(work),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ITemplateRepository>>()(work),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ITagRepository>>()(work),
                factory.GetInstance<IContentSection>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IContentTypeRepository>((factory, work) => new ContentTypeRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ITemplateRepository>>()(work),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IDataTypeDefinitionRepository>((factory, work) => new DataTypeDefinitionRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IContentTypeRepository>>()(work),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IDictionaryRepository>((factory, work) => new DictionaryRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ILanguageRepository>>()(work),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, ILanguageRepository>((factory, work) => new LanguageRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMediaRepository>((factory, work) => new MediaRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IMediaTypeRepository>>()(work),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ITagRepository>>()(work),
                factory.GetInstance<IContentSection>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMediaTypeRepository>((factory, work) => new MediaTypeRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IRelationRepository>((factory, work) => new RelationRepository(
                work,
                factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IRelationTypeRepository>>()(work),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IRelationTypeRepository>((factory, work) => new RelationTypeRepository(
                work,
                factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IUnitOfWork, IScriptRepository>((factory, work) => new ScriptRepository(                
                work,
                factory.GetInstance<IFileSystem>("ScriptFileSystem"),
                factory.GetInstance<IContentSection>()));
            container.Register<IUnitOfWork, IPartialViewRepository>((factory, work) => new PartialViewRepository(
                work,
                factory.GetInstance<IFileSystem>("ScriptFileSystem")),
                serviceName: "PartialViewFileSystem");
            container.Register<IUnitOfWork, IPartialViewRepository>((factory, work) => new PartialViewMacroRepository(
                work,
                factory.GetInstance<IFileSystem>("PartialViewMacroFileSystem")),
                serviceName: "PartialViewMacroRepository");
            container.Register<IUnitOfWork, IStylesheetRepository>((factory, work) => new StylesheetRepository(
                work,
                factory.GetInstance<IFileSystem>("StylesheetFileSystem")));
            container.Register<IDatabaseUnitOfWork, ITemplateRepository>((factory, work) => new TemplateRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IFileSystem>("MasterpageFileSystem"),
                factory.GetInstance<IFileSystem>("ViewFileSystem"),
                factory.GetInstance<ITemplatesSection>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMigrationEntryRepository>((factory, work) => new MigrationEntryRepository(
                work,
                factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IServerRegistrationRepository>((factory, work) => new ServerRegistrationRepository(
                work,
                factory.GetInstance<CacheHelper>().StaticCache, //special static cache scenario
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IUserTypeRepository>((factory, work) => new UserTypeRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IUserRepository>((factory, work) => new UserRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IUserTypeRepository>>()(work),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMacroRepository>((factory, work) => new MacroRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMemberRepository>((factory, work) => new MemberRepository(
                work,
                factory.GetInstance<CacheHelper>(),
                factory.GetInstance<ILogger>(),
                factory.GetInstance<ISqlSyntaxProvider>(),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IMemberTypeRepository>>()(work),
                factory.GetInstance<Func<IDatabaseUnitOfWork, IMemberGroupRepository>>()(work),
                factory.GetInstance<Func<IDatabaseUnitOfWork, ITagRepository>>()(work),
                factory.GetInstance<IContentSection>(),
                factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMemberTypeRepository>((factory, work) => new MemberTypeRepository(
               work,
               factory.GetInstance<CacheHelper>(),
               factory.GetInstance<ILogger>(),
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IMemberGroupRepository>((factory, work) => new MemberGroupRepository(
               work,
               factory.GetInstance<CacheHelper>(),
               factory.GetInstance<ILogger>(),
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IEntityRepository>((factory, work) => new EntityRepository(
               work,               
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, IDomainRepository>((factory, work) => new DomainRepository(
               work,
               factory.GetInstance<CacheHelper>(),
               factory.GetInstance<ILogger>(),
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, ITaskTypeRepository>((factory, work) => new TaskTypeRepository(
               work,
               factory.GetInstance<CacheHelper>("DisabledCache"), //never cache
               factory.GetInstance<ILogger>(),
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
            container.Register<IDatabaseUnitOfWork, EntityContainerRepository>((factory, work) => new EntityContainerRepository(
               work,
               factory.GetInstance<CacheHelper>(),
               factory.GetInstance<ILogger>(),
               factory.GetInstance<ISqlSyntaxProvider>(),
               factory.GetInstance<IMappingResolver>()));
        }

        /// <summary>
        /// Creates and initializes the db context when IoC requests it
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private DatabaseContext GetDbContext(IServiceFactory container)
        {
            var dbCtx = new DatabaseContext(
                container.GetInstance<IDatabaseFactory>(),
                container.GetInstance<ILogger>(),
                container.GetInstance<SqlSyntaxProviders>());

            //when it's first created we need to initialize it
            dbCtx.Initialize();
            return dbCtx;
        }
    }
}