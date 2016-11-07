using System.IO;
using System.Reflection;
using AutoMapper;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.DI;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Plugins;
using Umbraco.Web;
using Umbraco.Web.DI;
using Current = Umbraco.Core.DI.Current;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides the top-level base class for all Umbraco integration tests.
    /// </summary>
    /// <remarks>
    /// True unit tests do not need to inherit from this class, but most of Umbraco tests
    /// are not true unit tests but integration tests requiring services, databases, etc. This class
    /// provides all the necessary environment, through DI. Yes, DI is bad in tests - unit tests.
    /// But it is OK in integration tests.
    /// </remarks>
    public abstract class UmbracoTestBase
    {
        // this class
        // ensures that Current is properly resetted
        // ensures that a service container is properly initialized and disposed
        // compose the required dependencies according to test options (UmbracoTestAttribute)
        //
        // everything is virtual (because, why not?)
        // starting a test runs like this:
        // - SetUp() // when overriding, call base.SetUp() *first* then setup your own stuff
        // --- Compose() // when overriding, call base.Commpose() *first* then compose your own stuff
        // - test runs
        // - TearDown() // when overriding, clear you own stuff *then* call base.TearDown()
        //
        // about attributes
        //
        // this class defines the SetUp and TearDown methods, with proper attributes, and
        // these attributes are *inherited* so classes inheriting from this class should *not*
        // add the attributes to SetUp nor TearDown again
        //
        // this class is *not* marked with the TestFeature attribute because it is *not* a
        // test feature, and no test "base" class should be. only actual test feature classes
        // should be marked with that attribute.

        protected ServiceContainer Container { get; private set; }

        protected UmbracoTestAttribute Options { get; private set; }

        private static PluginManager _pluginManager;
        private static bool _firstDatabaseInSession = true;
        private bool _firstDatabaseInFixture = true;

        [SetUp]
        public virtual void SetUp()
        {
            // should not need this if all other tests were clean
            // but hey, never know, better avoid garbage-in
            Reset();

            Container = new ServiceContainer();
            Container.ConfigureUmbracoCore();

            // get/merge the attributes marking the method and/or the classes
            var testName = TestContext.CurrentContext.Test.Name;
            var pos = testName.IndexOf('(');
            if (pos > 0) testName = testName.Substring(0, pos);
            Options = UmbracoTestAttribute.Get(GetType().GetMethod(testName));

            Compose();
            Initialize();
        }

        protected virtual void Compose()
        {
            ComposeLogging(Options.Logger);
            ComposeCacheHelper();
            ComposeAutoMapper(Options.AutoMapper);
            ComposePluginManager(Options.ResetPluginManager);
            ComposeDatabase(Options.Database);
            // etc

            // not sure really
            var composition = new Composition(Container, RuntimeLevel.Run);
            Compose(composition);
        }

        protected virtual void Compose(Composition composition)
        { }

        protected virtual void Initialize()
        {
            InitializeAutoMapper(Options.AutoMapper);
        }

        #region Composing

        protected virtual void ComposeLogging(UmbracoTestOptions.Logger option)
        {
            if (option == UmbracoTestOptions.Logger.Mock)
            {
                Container.RegisterSingleton(f => Mock.Of<ILogger>());
                Container.RegisterSingleton(f => Mock.Of<IProfiler>());
            }
            else if (option == UmbracoTestOptions.Logger.Log4Net)
            {
                Container.RegisterSingleton<ILogger>(f => new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config"))));
                Container.RegisterSingleton<IProfiler>(f => new LogProfiler(f.GetInstance<ILogger>()));
            }

            Container.RegisterSingleton(f => new ProfilingLogger(f.GetInstance<ILogger>(), f.GetInstance<IProfiler>()));
        }

        protected virtual void ComposeCacheHelper()
        {
            Container.RegisterSingleton(f => CacheHelper.CreateDisabledCacheHelper());
            Container.RegisterSingleton(f => f.GetInstance<CacheHelper>().RuntimeCache);
        }

        protected virtual void ComposeAutoMapper(bool configure)
        {
            if (configure == false) return;

            Container.RegisterFrom<CoreModelMappersCompositionRoot>();
            Container.RegisterFrom<WebModelMappersCompositionRoot>();
        }

        protected virtual void ComposePluginManager(bool reset)
        {
            Container.RegisterSingleton(f =>
            {
                if (_pluginManager != null && reset == false) return _pluginManager;

                return _pluginManager = new PluginManager(f.GetInstance<CacheHelper>().RuntimeCache, f.GetInstance<ProfilingLogger>(), false)
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
            });
        }

        protected virtual void ComposeDatabase(UmbracoTestOptions.Database option)
        {
            if (option == UmbracoTestOptions.Database.None) return;

            // create the file
            // create the schema

        }

        #endregion

        #region Initialize

        protected virtual void InitializeAutoMapper(bool configure)
        {
            if (configure == false) return;

            Mapper.Initialize(configuration =>
            {
                var mappers = Container.GetAllInstances<ModelMapperConfiguration>();
                foreach (var mapper in mappers)
                    mapper.ConfigureMappings(configuration);
            });
        }

        #endregion

        #region TearDown and Reset

        [TearDown]
        public virtual void TearDown()
        {
            Reset();
        }

        protected virtual void Reset()
        {
            Current.Reset();

            Container?.Dispose();
            Container = null;

            // reset all other static things that should not be static ;(
            UriUtility.ResetAppDomainAppVirtualPath();
        }

        #endregion
    }
}
