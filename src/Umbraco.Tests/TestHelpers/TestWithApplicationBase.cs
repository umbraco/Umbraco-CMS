using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.DI;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Events;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Plugins;
using Umbraco.Core.Services;
using Umbraco.Web.DependencyInjection;
using Umbraco.Web.Services;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for all Umbraco tests that require the Umbraco application.
    /// </summary>
    /// <remarks>
    /// <para>Sets the Umbraco application DI container.</para>
    /// <para>Defines the Compose method for DI composition.</para>
    /// <para>Sets all sorts of things such as logging, plugin manager, base services, database factory & context...</para>
    /// <para>Does *not* create a database.</para>
    /// </remarks>
    [TestFixture]
    public abstract class TestWithApplicationBase : TestWithSettingsBase
    {
        private static PluginManager _pluginManager;

        // tests shouldn't use IoC, but for all these tests inheriting from this class are integration tests
        // and the number of these will hopefully start getting greatly reduced now that most things are mockable.
        protected IServiceContainer Container { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        protected CacheHelper CacheHelper { get; private set; }

        protected virtual ISqlSyntaxProvider SqlSyntax => new SqlCeSyntaxProvider();

        protected IMapperCollection Mappers => Container.GetInstance<IMapperCollection>();

        /// <summary>
        /// Gets a value indicating whether the plugin manager should be resetted before and after each test.
        /// </summary>
        /// <remarks>
        /// False by default, so the plugin manager does not need to re-scan all of the assemblies and tests run faster.
        /// Can be overriden if the plugin manager does need to reset, usually when SetupPluginManager has been overriden.
        /// </remarks>
        protected virtual bool PluginManagerResetRequired => false;

        [TestFixtureSetUp]
        public void InitializeFixture()
        {
            Logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            Profiler = new LogProfiler(Logger);
            ProfilingLogger = new ProfilingLogger(Logger, Profiler);
        }

        public override void SetUp()
        {
            base.SetUp();

            var container = new ServiceContainer();
            container.ConfigureUmbracoCore();
            Container = container;

            TestHelper.InitializeContentDirectories();
            CacheHelper = CreateCacheHelper();
            InitializeLegacyMappingsForCoreEditors();
            SetupPluginManager();
            Compose();
            InitializeAutoMapper();
            MoreSetUp();
        }

        public override void TearDown()
        {
            base.TearDown();

            TestHelper.CleanContentDirectories();
            TestHelper.CleanUmbracoSettingsConfig();

            ResetPluginManager();
            Container.Dispose();
        }

        protected virtual void Compose()
        {
            var settings = SettingsForTests.GetDefault();

            // basic things
            Container.RegisterSingleton(factory => Logger);
            Container.RegisterSingleton(factory => Profiler);
            Container.RegisterSingleton(factory => ProfilingLogger);

            Container.Register(factory => CacheHelper);
            Container.Register(factory => CacheHelper.RuntimeCache);

            // register mappers
            Container.RegisterFrom<CoreModelMappersCompositionRoot>();
            Container.RegisterFrom<WebModelMappersCompositionRoot>();

            Container.RegisterInstance(_pluginManager);

            // default Datalayer/Repositories/SQL/Database/etc...
            Container.RegisterFrom<RepositoryCompositionRoot>();

            // register basic stuff that might need to be there for some container resolvers to work
            Container.RegisterSingleton(factory => SettingsForTests.GetDefault());
            Container.RegisterSingleton(factory => settings.Content);
            Container.RegisterSingleton(factory => settings.Templates);
            Container.Register<IServiceProvider, ActivatorServiceProvider>();
            Container.Register(factory => new MediaFileSystem(Mock.Of<IFileSystem>()));
            Container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();

            // replace some stuff
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "ScriptFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "PartialViewFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "PartialViewMacroFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "StylesheetFileSystem");

            // need real file systems here as templates content is on-disk only
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "MasterpageFileSystem");
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("Views", "/views"), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("MasterPages", "/masterpages"), "MasterpageFileSystem");

            // no factory (noop)
            Container.RegisterSingleton<IPublishedContentModelFactory, NoopPublishedContentModelFactory>();

            // register application stuff (database factory & context, services...)
            Container.RegisterCollectionBuilder<MapperCollectionBuilder>()
                .Add(f => f.GetInstance<PluginManager>().ResolveAssignedMapperTypes());

            Container.RegisterSingleton<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());
            Container.RegisterSingleton<IUmbracoDatabaseAccessor, TestUmbracoDatabaseAccessor>();
            Container.RegisterSingleton<IDatabaseFactory>(f => new DefaultDatabaseFactory(
                Core.Configuration.GlobalSettings.UmbracoConnectionName,
                TestObjects.GetDefaultSqlSyntaxProviders(Logger),
                Logger, f.GetInstance<IUmbracoDatabaseAccessor>(),
                Mock.Of<IMapperCollection>()));
            Container.RegisterSingleton(f => new DatabaseContext(
                f.GetInstance<IDatabaseFactory>(),
                Logger,
                Mock.Of<IRuntimeState>(),
                Mock.Of<IMigrationEntryService>()));

            Container.RegisterCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty
            Container.Register(factory
               => TestObjects.GetDatabaseUnitOfWorkProvider(factory.GetInstance<ILogger>(), factory.TryGetInstance<IDatabaseFactory>(), factory.TryGetInstance<RepositoryFactory>()));

            Container.RegisterFrom<ServicesCompositionRoot>();
            // composition root is doing weird things, fix
            Container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            Container.RegisterSingleton<ISectionService, SectionService>();
        }

        private static void InitializeLegacyMappingsForCoreEditors()
        {
            // create the legacy prop-eds mapping
            if (LegacyPropertyEditorIdToAliasConverter.Count() == 0)
                LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
        }

        // initialize automapper if required - takes time so don't do it unless required
        private void InitializeAutoMapper()
        {
            if (GetType().GetCustomAttribute<RequiresAutoMapperMappingsAttribute>(false) == null) return;

            Mapper.Initialize(configuration =>
            {
                var mappers = Container.GetAllInstances<ModelMapperConfiguration>();
                foreach (var mapper in mappers)
                    mapper.ConfigureMappings(configuration);
            });
        }

        protected virtual void ResetPluginManager()
        {
            if (PluginManagerResetRequired)
                _pluginManager = null;
        }

        protected virtual CacheHelper CreateCacheHelper()
        {
            return CacheHelper.CreateDisabledCacheHelper();
        }
        
        protected virtual void SetupPluginManager()
        {
            if (_pluginManager == null || PluginManagerResetRequired)
            {
                _pluginManager = new PluginManager(CacheHelper.RuntimeCache, ProfilingLogger, false)
                {
                    AssembliesToScan = new[]
                    {
                        Assembly.Load("Umbraco.Core"),
                        Assembly.Load("umbraco"),
                        Assembly.Load("Umbraco.Tests"),
                        Assembly.Load("cms"),
                        Assembly.Load("controls"),
                    }
                };
            }
        }

        // fixme - rename & refactor
        protected virtual void MoreSetUp()
        { }
    }
}