﻿using System;
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

            SetupCacheHelper();

            InitializeLegacyMappingsForCoreEditors();

            SetupPluginManager();

            SetupApplicationContext();

            InitializeMappers();

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
        private void InitializeMappers()
        {
            if (GetType().GetCustomAttribute<RequiresAutoMapperMappingsAttribute>(false) != null)
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
                        mapper.ConfigureMappings(configuration, ApplicationContext);
                    }
                });
            }
        }

        /// <summary>
        /// By default this returns false which means the plugin manager will not be reset so it doesn't need to re-scan 
        /// all of the assemblies. Inheritors can override this if plugin manager resetting is required, generally needs
        /// to be set to true if the SetupPluginManager has been overridden.
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

        protected virtual void SetupCacheHelper()
        {
            CacheHelper = CreateCacheHelper();
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
        }

        protected virtual ApplicationContext CreateApplicationContext()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var repoFactory = new RepositoryFactory(CacheHelper, Logger, sqlSyntax, SettingsForTests.GenerateMockSettings());

            var evtMsgs = new TransientMessagesFactory();
            var applicationContext = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory(Core.Configuration.GlobalSettings.UmbracoConnectionName, Logger), Logger, sqlSyntax, Constants.DatabaseProviders.SqlCe),
                //assign the service context
                new ServiceContext(repoFactory, new PetaPocoUnitOfWorkProvider(Logger), new FileUnitOfWorkProvider(), new PublishingStrategy(evtMsgs, Logger), CacheHelper, Logger, evtMsgs),
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
                        Assembly.Load("businesslogic"),
                        Assembly.Load("cms"),
                        Assembly.Load("controls"),
                        Assembly.Load("umbraco.editorControls"),
                        Assembly.Load("umbraco.MacroEngines"),
                        Assembly.Load("umbraco.providers"),
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