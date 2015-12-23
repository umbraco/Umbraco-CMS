using System;
using System.Linq;
using LightInject;
using Umbraco.Core.Cache;
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

            //here we are using some nice IoC magic:
            //https://github.com/seesharper/LightInject/issues/237
            //This tells the container that anytime there is a ctor dependency for IDatabaseUnitOfWork and it's available as the first
            //arg in the runtimeArgs, to use that. This means we donn't have to explicitly define all ctor's for all repositories which
            //saves us a lot of code.
            container.RegisterConstructorDependency<IDatabaseUnitOfWork>((factory, info, runtimeArguments) =>
            {
                var uow = runtimeArguments.Length > 0 ? runtimeArguments[0] as IDatabaseUnitOfWork : null;
                return uow;
            });
            //This ensures that the correct CacheHelper is returned for the right repos
            container.RegisterConstructorDependency<CacheHelper>((factory, info, runtimeArguments) =>
            {
                var declaringType = info.Member.DeclaringType;
                var disabledCacheRepos = new[]
                {
                    typeof (ITaskRepository),
                    typeof (ITaskTypeRepository),
                    typeof (IAuditRepository),
                    typeof (IRelationRepository),
                    typeof (IRelationTypeRepository),
                    typeof (IAuditRepository),
                    typeof (IMigrationEntryRepository)
                };
                return disabledCacheRepos.Any(x => TypeHelper.IsTypeAssignableFrom(x, declaringType))
                    ? factory.GetInstance<CacheHelper>("DisabledCache")
                    : factory.GetInstance<CacheHelper>();
            });

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

            //These repo registrations require custom injections so we need to define them:

            container.Register<IDatabaseUnitOfWork, IServerRegistrationRepository>((factory, work) => new ServerRegistrationRepository(
                work,
                factory.GetInstance<CacheHelper>().StaticCache, //special static cache scenario
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