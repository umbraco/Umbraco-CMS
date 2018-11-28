using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.Composers;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Services;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing.Composers;
using Umbraco.Web.ContentApps;
using Current = Umbraco.Core.Composing.Current;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Testing
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
        // --- Initialize() // same
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

        protected Composition Composition { get; private set; }

        protected IContainer Container { get; private set; }

        protected UmbracoTestAttribute Options { get; private set; }

        protected static bool FirstTestInSession = true;

        protected bool FirstTestInFixture = true;

        internal TestObjects TestObjects { get; private set; }

        private static TypeLoader _commonTypeLoader;

        private TypeLoader _featureTypeLoader;

        #region Accessors

        protected ILogger Logger => Container.GetInstance<ILogger>();

        protected IProfiler Profiler => Container.GetInstance<IProfiler>();

        protected virtual IProfilingLogger ProfilingLogger => Container.GetInstance<IProfilingLogger>();

        protected CacheHelper CacheHelper => Container.GetInstance<CacheHelper>();

        protected virtual ISqlSyntaxProvider SqlSyntax => Container.GetInstance<ISqlSyntaxProvider>();

        protected IMapperCollection Mappers => Container.GetInstance<IMapperCollection>();

        #endregion

        #region Setup

        [SetUp]
        public virtual void SetUp()
        {
            // should not need this if all other tests were clean
            // but hey, never know, better avoid garbage-in
            Reset();

            // get/merge the attributes marking the method and/or the classes
            Options = TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

            // fixme see CoreRuntime and align!
            var (logger, profiler) = GetLoggers(Options.Logger);
            var proflogger = new ProfilingLogger(logger, profiler);
            var cacheHelper = GetCacheHelper();
            var globalSettings = SettingsForTests.GetDefaultGlobalSettings();
            var typeLoader = GetTypeLoader(cacheHelper.RuntimeCache, globalSettings, proflogger, Options.TypeLoader);

            Container = ContainerFactory.Create();
            Current.Factory = Container;

            Container.RegisterInstance(typeLoader);
            Container.RegisterInstance(logger);
            Container.RegisterInstance(profiler);
            Container.RegisterInstance<IProfilingLogger>(proflogger);
            Container.RegisterInstance(cacheHelper);
            Container.RegisterInstance(cacheHelper.RuntimeCache);

            Composition = new Composition(Container, typeLoader, proflogger, RuntimeLevel.Run);
            TestObjects = new TestObjects(Container);
            Compose();
            Initialize();
        }

        protected virtual void Compose()
        {
            ComposeAutoMapper(Options.AutoMapper);
            ComposeDatabase(Options.Database);
            ComposeApplication(Options.WithApplication);

            // etc
            ComposeWeb();
            ComposeWtf();

            // not sure really
            Compose(Composition);
        }

        protected virtual void Compose(Composition composition)
        { }

        protected virtual void Initialize()
        {
            InitializeAutoMapper(Options.AutoMapper);
            InitializeApplication(Options.WithApplication);
        }

        #endregion

        #region Compose

        protected virtual (ILogger, IProfiler) GetLoggers(UmbracoTestOptions.Logger option)
        {
            ILogger logger;
            IProfiler profiler;

            switch (option)
            {
                case UmbracoTestOptions.Logger.Mock:
                    logger = Mock.Of<ILogger>();
                    profiler = Mock.Of<IProfiler>();
                    break;
                case UmbracoTestOptions.Logger.Serilog:
                    logger = new SerilogLogger(new FileInfo(TestHelper.MapPathForTest("~/unit-test.config")));
                    profiler = new LogProfiler(logger);
                    break;
                case UmbracoTestOptions.Logger.Console:
                    logger = new ConsoleLogger();
                    profiler = new LogProfiler(logger);
                    break;
                default:
                    throw new NotSupportedException($"Logger option {option} is not supported.");
            }

            return (logger, profiler);
        }

        protected virtual CacheHelper GetCacheHelper()
        {
            return CacheHelper.Disabled;
        }

        protected virtual void ComposeWeb()
        {
            //TODO: Should we 'just' register the WebRuntimeComponent?

            // imported from TestWithSettingsBase
            // which was inherited by TestWithApplicationBase so pretty much used everywhere
            Umbraco.Web.Composing.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();

            // web
            Container.Register(_ => Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            Container.RegisterSingleton<PublishedRouter>();
            Composition.GetCollectionBuilder<ContentFinderCollectionBuilder>();
            Container.Register<IContentLastChanceFinder, TestLastChanceFinder>();
            Container.Register<IVariationContextAccessor, TestVariationContextAccessor>();
        }

        protected virtual void ComposeWtf()
        {
            // what else?
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            Container.RegisterSingleton(f => runtimeStateMock.Object);

            // ah...
            Composition.GetCollectionBuilder<ActionCollectionBuilder>();
            Composition.GetCollectionBuilder<PropertyValueConverterCollectionBuilder>();
            Container.RegisterSingleton<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            Container.RegisterSingleton<IMediaPathScheme, OriginalMediaPathScheme>();

            // register empty content apps collection
            Composition.GetCollectionBuilder<ContentAppDefinitionCollectionBuilder>();
        }

        protected virtual void ComposeAutoMapper(bool configure)
        {
            if (configure == false) return;

            Composition
                .ComposeCoreMappingProfiles()
                .ComposeWebMappingProfiles();
        }

        protected virtual TypeLoader GetTypeLoader(IRuntimeCacheProvider runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger, UmbracoTestOptions.TypeLoader option)
        {
            switch (option)
            {
                case UmbracoTestOptions.TypeLoader.Default:
                    return _commonTypeLoader ?? (_commonTypeLoader = CreateCommonTypeLoader(runtimeCache, globalSettings, logger));
                case UmbracoTestOptions.TypeLoader.PerFixture:
                    return _featureTypeLoader ?? (_featureTypeLoader = CreateTypeLoader(runtimeCache, globalSettings, logger));
                case UmbracoTestOptions.TypeLoader.PerTest:
                    return CreateTypeLoader(runtimeCache, globalSettings, logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(option));
            }
        }

        protected virtual TypeLoader CreateTypeLoader(IRuntimeCacheProvider runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            return CreateCommonTypeLoader(runtimeCache, globalSettings, logger);
        }

        // common to all tests = cannot be overriden
        private static TypeLoader CreateCommonTypeLoader(IRuntimeCacheProvider runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            return new TypeLoader(runtimeCache, globalSettings, logger, false)
            {
                AssembliesToScan = new[]
                {
                    Assembly.Load("Umbraco.Core"),
                    Assembly.Load("Umbraco.Web"),
                    Assembly.Load("Umbraco.Tests")
                }
            };
        }

        protected virtual void ComposeDatabase(UmbracoTestOptions.Database option)
        {
            if (option == UmbracoTestOptions.Database.None) return;

            // create the file
            // create the schema

        }

        protected virtual void ComposeApplication(bool withApplication)
        {
            if (withApplication == false) return;

            var umbracoSettings = SettingsForTests.GetDefaultUmbracoSettings();
            var globalSettings = SettingsForTests.GetDefaultGlobalSettings();
            //apply these globally
            SettingsForTests.ConfigureSettings(umbracoSettings);
            SettingsForTests.ConfigureSettings(globalSettings);

            // default Datalayer/Repositories/SQL/Database/etc...
            Composition.ComposeRepositories();

            // register basic stuff that might need to be there for some container resolvers to work
            Container.RegisterSingleton(factory => umbracoSettings);
            Container.RegisterSingleton(factory => globalSettings);
            Container.RegisterSingleton(factory => umbracoSettings.Content);
            Container.RegisterSingleton(factory => umbracoSettings.Templates);
            Container.RegisterSingleton(factory => umbracoSettings.WebRouting);

            Container.RegisterSingleton<IExamineManager>(factory => ExamineManager.Instance);

            // register filesystems
            Container.RegisterSingleton(factory => TestObjects.GetFileSystemsMock());

            var logger = Mock.Of<ILogger>();
            var scheme = Mock.Of<IMediaPathScheme>();
            var config = Mock.Of<IContentSection>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), config, scheme, logger);
            Container.RegisterSingleton<IMediaFileSystem>(factory => mediaFileSystem);

            // no factory (noop)
            Container.RegisterSingleton<IPublishedModelFactory, NoopPublishedModelFactory>();

            // register application stuff (database factory & context, services...)
            Composition.GetCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            Container.RegisterSingleton<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());
            Container.RegisterSingleton<IUmbracoDatabaseFactory>(f => new UmbracoDatabaseFactory(
                Constants.System.UmbracoConnectionName,
                Logger,
                new Lazy<IMapperCollection>(() => Mock.Of<IMapperCollection>())));
            Container.RegisterSingleton(f => f.TryGetInstance<IUmbracoDatabaseFactory>().SqlContext);

            Composition.GetCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty

            Container.RegisterSingleton(factory
                => TestObjects.GetScopeProvider(factory.TryGetInstance<ILogger>(), factory.TryGetInstance<FileSystems>(), factory.TryGetInstance<IUmbracoDatabaseFactory>()));
            Container.RegisterSingleton(factory => (IScopeAccessor) factory.GetInstance<IScopeProvider>());

            Composition.ComposeServices();

            // composition root is doing weird things, fix
            Container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            Container.RegisterSingleton<ISectionService, SectionService>();

            // somehow property editor ends up wanting this
            Composition.GetCollectionBuilder<ManifestValueValidatorCollectionBuilder>();
            Container.RegisterSingleton<ManifestParser>();

            // note - don't register collections, use builders
            Composition.GetCollectionBuilder<DataEditorCollectionBuilder>();
            Container.RegisterSingleton<PropertyEditorCollection>();
            Container.RegisterSingleton<ParameterEditorCollection>();
        }

        #endregion

        #region Initialize

        protected virtual void InitializeAutoMapper(bool configure)
        {
            if (configure == false) return;

            Mapper.Initialize(configuration =>
            {
                var profiles = Container.GetAllInstances<Profile>();
                foreach (var profile in profiles)
                    configuration.AddProfile(profile);
            });
        }

        protected virtual void InitializeApplication(bool withApplication)
        {
            if (withApplication == false) return;

            TestHelper.InitializeContentDirectories();
        }

        #endregion

        #region TearDown and Reset

        [TearDown]
        public virtual void TearDown()
        {
            FirstTestInFixture = false;
            FirstTestInSession = false;

            Reset();

            if (Options.WithApplication)
            {
                TestHelper.CleanContentDirectories();
                TestHelper.CleanUmbracoSettingsConfig();
            }
        }

        protected virtual void Reset()
        {
            // reset and dispose scopes
            // ensures we don't leak an opened database connection
            // which would lock eg SqlCe .sdf files
            if (Container?.TryGetInstance<IScopeProvider>() is ScopeProvider scopeProvider)
            {
                Core.Scoping.Scope scope;
                while ((scope = scopeProvider.AmbientScope) != null)
                {
                    scope.Reset();
                    scope.Dispose();
                }
            }

            Current.Reset();

            Container?.Dispose();
            Container = null;

            // reset all other static things that should not be static ;(
            UriUtility.ResetAppDomainAppVirtualPath();
            SettingsForTests.Reset(); // fixme - should it be optional?

            Mapper.Reset();

            // clear static events
            DocumentRepository.ClearScopeEvents();
            MediaRepository.ClearScopeEvents();
            MemberRepository.ClearScopeEvents();
            ContentTypeService.ClearScopeEvents();
            MediaTypeService.ClearScopeEvents();
            MemberTypeService.ClearScopeEvents();
        }

        #endregion
    }
}
