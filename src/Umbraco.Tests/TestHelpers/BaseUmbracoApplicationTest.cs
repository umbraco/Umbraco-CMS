using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Models.Mapping;
using umbraco.BusinessLogic;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for Umbraco application tests.
    /// </summary>
    /// <remarks>Sets logging, pluging manager, application context, base resolvers...</remarks>
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

            TestHelper.InitializeContentDirectories();

            CacheHelper = CreateCacheHelper();

            InitializeLegacyMappingsForCoreEditors();

            SetupPluginManager();

            SetupApplicationContext();

            if (GetType().GetCustomAttribute<RequiresAutoMapperMappingsAttribute>(false) != null)
            {
                InitializeMappers(ApplicationContext);
            }            

            FreezeResolution();

        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            // reset settings
            SettingsForTests.Reset();
            UmbracoContext.Current = null;
            TestHelper.CleanContentDirectories();
            TestHelper.CleanUmbracoSettingsConfig();

            // reset the app context, this should reset most things that require resetting like ALL resolvers
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;

            // reset plugin manager
            ResetPluginManager();
        }

        private static readonly object Locker = new object();

        private static void InitializeLegacyMappingsForCoreEditors()
        {
            lock (Locker)
            {
                if (LegacyPropertyEditorIdToAliasConverter.Count() == 0)
                {
                    // create the legacy prop-eds mapping
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
        protected virtual void InitializeMappers(ApplicationContext applicationContext)
        {
            Mapper.Initialize(configuration =>
            {
                var mappers = PluginManager.Current.FindAndCreateInstances<IMapperConfiguration>(
                    specificAssemblies: new[]
                    {
                        typeof(ContentModelMapper).Assembly,
                        typeof(ApplicationRegistrar).Assembly
                    });
                foreach (var mapper in mappers)
                {
                    mapper.ConfigureMappings(configuration, applicationContext);
                }
            });            
        }

        /// <summary>
        /// Inheritors can resset the plugin manager if they choose to on teardown
        /// </summary>
        protected virtual void ResetPluginManager()
        {
            PluginManager.Current = null;
        }

        protected virtual CacheHelper CreateCacheHelper()
        {
            return CacheHelper.CreateDisabledCacheHelper();
        }

        /// <summary>
        /// Inheritors can override this if they wish to create a custom application context
        /// </summary>
        protected virtual void SetupApplicationContext()
        {
            var applicationContext = CreateApplicationContext();
            ApplicationContext.Current = applicationContext;

            // FileSystemProviderManager captures the current ApplicationContext ScopeProvider
            // in its current static instance (yea...) so we need to reset it here to ensure
            // it is using the proper ScopeProvider
            FileSystemProviderManager.ResetCurrent();
        }

        protected virtual ApplicationContext CreateApplicationContext()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var repoFactory = new RepositoryFactory(CacheHelper, Logger, sqlSyntax, SettingsForTests.GenerateMockSettings());

            var dbFactory = new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, Logger);
            var scopeProvider = new ScopeProvider(dbFactory);
            var evtMsgs = new TransientMessagesFactory();
            var applicationContext = new ApplicationContext(
                //assign the db context
                new DatabaseContext(scopeProvider, Logger, sqlSyntax, Constants.DatabaseProviders.SqlCe),
                //assign the service context
                new ServiceContext(repoFactory, new PetaPocoUnitOfWorkProvider(scopeProvider), CacheHelper, Logger, evtMsgs),
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
            PluginManager.Current = new PluginManager(
                new ActivatorServiceProvider(),
                CacheHelper.RuntimeCache, ProfilingLogger, false)
            {
                AssembliesToScan = new[]
                {
                    Assembly.Load("Umbraco.Core"),
                    Assembly.Load("umbraco"),
                    Assembly.Load("Umbraco.Tests"),
                    Assembly.Load("businesslogic"),
                    Assembly.Load("cms"),
                    Assembly.Load("controls"),
                    Assembly.Load("umbraco.editorControls"),
                    Assembly.Load("umbraco.MacroEngines"),
                    Assembly.Load("umbraco.providers"),
                }
            };
        }

        /// <summary>
        /// Inheritors can override this to setup any resolvers before resolution is frozen
        /// </summary>
        protected virtual void FreezeResolution()
        {
            Resolution.Freeze();
        }

        protected ServiceContext ServiceContext
        {
            get { return ApplicationContext.Services; }
        }

        protected ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }

        protected ILogger Logger
        {
            get { return ProfilingLogger.Logger; }
        }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        protected CacheHelper CacheHelper { get; private set; }
    }
}