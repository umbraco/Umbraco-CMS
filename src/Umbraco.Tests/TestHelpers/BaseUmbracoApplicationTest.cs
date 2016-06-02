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
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Web;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Events;
using Umbraco.Core.Plugins;
using Umbraco.Web.DependencyInjection;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// A base test class used for umbraco tests whcih sets up the logging, plugin manager any base resolvers, etc... and
    /// ensures everything is torn down properly.
    /// </summary>
    [TestFixture]
    public abstract class BaseUmbracoApplicationTest : BaseUmbracoConfigurationTest
    {

        [TestFixtureSetUp]
        public void InitializeFixture()
        {
            var logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            ProfilingLogger = new ProfilingLogger(logger, new LogProfiler(logger));
        }

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            var container = new ServiceContainer();
            container.EnableAnnotatedConstructorInjection();
            Container = container;

            TestHelper.InitializeContentDirectories();

            SetupCacheHelper();

            InitializeLegacyMappingsForCoreEditors();

            SetupPluginManager();

            ConfigureContainer();

            SetupApplicationContext();

            InitializeMappers();

            FreezeResolution();

        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            //reset settings
            SettingsForTests.Reset();
            UmbracoContext.Current = null;
            TestHelper.CleanContentDirectories();
            TestHelper.CleanUmbracoSettingsConfig();
            //reset the app context, this should reset most things that require resetting like ALL resolvers
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;
            ResetPluginManager();

            Container.Dispose();

        }

        protected virtual void ConfigureContainer()
        {
            // oh no! should not use a container in unit tests?
            var settings = SettingsForTests.GetDefault();

            //register mappers
            Container.RegisterFrom<CoreModelMappersCompositionRoot>();
            Container.RegisterFrom<WebModelMappersCompositionRoot>();

            Container.Register<IServiceContainer>(factory => Container);
            Container.Register<PluginManager>(factory => PluginManager.Current);

            //Default Datalayer/Repositories/SQL/Database/etc...
            Container.RegisterFrom<RepositoryCompositionRoot>();

            //register basic stuff that might need to be there for some container resolvers to work,  we can
            // add more to this in base classes in resolution freezing
            Container.RegisterSingleton<ILogger>(factory => Logger);
            Container.Register<CacheHelper>(factory => CacheHelper);
            Container.Register<ProfilingLogger>(factory => ProfilingLogger);
            Container.RegisterSingleton<IUmbracoSettingsSection>(factory => SettingsForTests.GetDefault());
            Container.RegisterSingleton<IContentSection>(factory => settings.Content);
            Container.RegisterSingleton<ITemplatesSection>(factory => settings.Templates);
            Container.Register<IRuntimeCacheProvider>(factory => CacheHelper.RuntimeCache);
            Container.Register<IServiceProvider, ActivatorServiceProvider>();
            Container.Register<MediaFileSystem>(factory => new MediaFileSystem(Mock.Of<IFileSystem>()));
            Container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();

            //replace some stuff
            Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "ScriptFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "PartialViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "PartialViewMacroFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "StylesheetFileSystem");

            // need real file systems here as templates content is on-disk only
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "MasterpageFileSystem");
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("Views", "/views"), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("MasterPages", "/masterpages"), "MasterpageFileSystem");
        }

        private static readonly object Locker = new object();

        protected IMappingResolver MappingResolver
        {
            get { return Container.GetInstance<IMappingResolver>(); }
        }

        private static void InitializeLegacyMappingsForCoreEditors()
        {
            lock (Locker)
            {
                if (LegacyPropertyEditorIdToAliasConverter.Count() == 0)
                {
                    //Create the legacy prop-eds mapping
                    LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
                }
            }
        }

        /// <summary>
        /// If this class requires auto-mapper mapping initialization then init them
        /// </summary>
        /// <remarks>
        /// This is an opt-in option because initializing the mappers takes about 500ms which equates to quite a lot
        /// of time with every test.
        /// </remarks>
        private void InitializeMappers()
        {
            if (this.GetType().GetCustomAttribute<RequiresAutoMapperMappingsAttribute>(false) != null)
            {
                Mapper.Initialize(configuration =>
                {
                    var mappers = Container.GetAllInstances<ModelMapperConfiguration>();
                    foreach (var mapper in mappers)
                    {
                        mapper.ConfigureMappings(configuration, ApplicationContext);
                    }
                });
            }
        }

        /// <summary>
        /// By default this returns false which means the plugin manager will not be reset so it doesn't need to re-scan
        /// all of the assemblies. Inheritors can override this if plugin manager resetting is required, generally needs
        /// to be set to true if the  SetupPluginManager has been overridden.
        /// </summary>
        protected virtual bool PluginManagerResetRequired
        {
            get { return false; }
        }

        /// <summary>
        /// Inheritors can resset the plugin manager if they choose to on teardown
        /// </summary>
        protected virtual void ResetPluginManager()
        {
            if (PluginManagerResetRequired)
            {
                PluginManager.Current = null;
            }
        }

        private void SetupCacheHelper()
        {
            CacheHelper = CreateCacheHelper();
        }

        protected virtual CacheHelper CreateCacheHelper()
        {
            return CacheHelper.CreateDisabledCacheHelper();
        }
        
        private void SetupApplicationContext()
        {
            var applicationContext = CreateApplicationContext();
            ApplicationContext.Current = applicationContext;
        }

        /// <summary>
        /// Inheritors can override this if they wish to create a custom application context
        /// </summary>
        protected virtual ApplicationContext CreateApplicationContext()
        {
            var evtMsgs = new TransientEventMessagesFactory();
            var applicationContext = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory(
                    Core.Configuration.GlobalSettings.UmbracoConnectionName,
                    TestObjects.GetDefaultSqlSyntaxProviders(Logger),
                    Logger, new TestScopeContextAdapter(),
                    Mock.Of<IMappingResolver>()), Logger),
                //assign the service context
                TestObjects.GetServiceContext(
                    Container.GetInstance<RepositoryFactory>(),
                    TestObjects.GetDatabaseUnitOfWorkProvider(Logger),
                    new FileUnitOfWorkProvider(),
                    CacheHelper,
                    Logger,
                    evtMsgs,
                    Enumerable.Empty<IUrlSegmentProvider>()),
                CacheHelper,
                ProfilingLogger)
            {
                IsReady = true
            };
            return applicationContext;
        }

        /// <summary>
        /// Inheritors can override this if they wish to setup the plugin manager differenty (i.e. specify certain assemblies to load)
        /// </summary>
        protected virtual void SetupPluginManager()
        {
            if (PluginManager.Current == null || PluginManagerResetRequired)
            {
                PluginManager.Current = new PluginManager(
                    new ActivatorServiceProvider(),
                    CacheHelper.RuntimeCache, ProfilingLogger, false)
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

        /// <summary>
        /// Inheritors can override this to setup any resolvers before resolution is frozen
        /// </summary>
        protected virtual void FreezeResolution()
        {
            Resolution.Freeze();
        }

        protected ApplicationContext ApplicationContext => ApplicationContext.Current;

        protected ILogger Logger => ProfilingLogger.Logger;
        protected ProfilingLogger ProfilingLogger { get; private set; }
        protected CacheHelper CacheHelper { get; private set; }

        //I know tests shouldn't use IoC, but for all these tests inheriting from this class are integration tests
        // and the number of these will hopefully start getting greatly reduced now that most things are mockable.
        internal IServiceContainer Container { get; private set; }

        protected virtual ISqlSyntaxProvider SqlSyntax => new SqlCeSyntaxProvider();
    }
}