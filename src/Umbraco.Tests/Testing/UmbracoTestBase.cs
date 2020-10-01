using System;
using System.IO;
using System.Reflection;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
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
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Services;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Trees;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Mapping;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.Sections;
using Current = Umbraco.Core.Composing.Current;
using FileSystems = Umbraco.Core.IO.FileSystems;
using Umbraco.Web.Templates;
using Umbraco.Web.PropertyEditors;
using Umbraco.Core.Models;

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

        protected IFactory Factory { get; private set; }

        protected UmbracoTestAttribute Options { get; private set; }

        protected static bool FirstTestInSession = true;

        protected bool FirstTestInFixture = true;

        internal TestObjects TestObjects { get; private set; }

        private static TypeLoader _commonTypeLoader;

        private TypeLoader _featureTypeLoader;

        #region Accessors

        protected ILogger Logger => Factory.GetInstance<ILogger>();

        protected IProfiler Profiler => Factory.GetInstance<IProfiler>();

        protected virtual IProfilingLogger ProfilingLogger => Factory.GetInstance<IProfilingLogger>();

        protected AppCaches AppCaches => Factory.GetInstance<AppCaches>();

        protected virtual ISqlSyntaxProvider SqlSyntax => Factory.GetInstance<ISqlSyntaxProvider>();

        protected IMapperCollection Mappers => Factory.GetInstance<IMapperCollection>();

        protected UmbracoMapper Mapper => Factory.GetInstance<UmbracoMapper>();

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

            // FIXME: align to runtimes & components - don't redo everything here

            var (logger, profiler) = GetLoggers(Options.Logger);
            var proflogger = new ProfilingLogger(logger, profiler);
            var appCaches = GetAppCaches();
            var globalSettings = SettingsForTests.GetDefaultGlobalSettings();
            var typeLoader = GetTypeLoader(appCaches.RuntimeCache, globalSettings, proflogger, Options.TypeLoader);

            var register = RegisterFactory.Create();

            Composition = new Composition(register, typeLoader, proflogger, ComponentTests.MockRuntimeState(RuntimeLevel.Run));

            Composition.RegisterUnique(typeLoader);
            Composition.RegisterUnique(logger);
            Composition.RegisterUnique(profiler);
            Composition.RegisterUnique<IProfilingLogger>(proflogger);
            Composition.RegisterUnique(appCaches);

            TestObjects = new TestObjects(register);
            Compose();
            Current.Factory = Factory = Composition.CreateFactory();
            Initialize();
        }

        protected virtual void Compose()
        {
            ComposeMapper(Options.Mapper);
            ComposeDatabase(Options.Database);
            ComposeApplication(Options.WithApplication);

            // etc
            ComposeWeb();
            ComposeMisc();

            // not sure really
            Compose(Composition);
        }

        protected virtual void Compose(Composition composition)
        { }

        protected virtual void Initialize()
        {
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

        protected virtual AppCaches GetAppCaches()
        {
            return AppCaches.Disabled;
        }

        protected virtual void ComposeWeb()
        {
            // imported from TestWithSettingsBase
            // which was inherited by TestWithApplicationBase so pretty much used everywhere
            Umbraco.Web.Composing.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();

            // web
            Composition.RegisterUnique(_ => Umbraco.Web.Composing.Current.UmbracoContextAccessor);
            Composition.RegisterUnique<IPublishedRouter, PublishedRouter>();
            Composition.WithCollectionBuilder<ContentFinderCollectionBuilder>();

            Composition.DataValueReferenceFactories();

            Composition.RegisterUnique<IContentLastChanceFinder, TestLastChanceFinder>();
            Composition.RegisterUnique<IVariationContextAccessor, TestVariationContextAccessor>();
            Composition.RegisterUnique<IPublishedSnapshotAccessor, TestPublishedSnapshotAccessor>();

            // register back office sections in the order we want them rendered
            Composition.WithCollectionBuilder<SectionCollectionBuilder>().Append<ContentSection>()
                .Append<MediaSection>()
                .Append<SettingsSection>()
                .Append<PackagesSection>()
                .Append<UsersSection>()
                .Append<MembersSection>()
                .Append<FormsSection>()
                .Append<TranslationSection>();
            Composition.RegisterUnique<ISectionService, SectionService>();

            Composition.RegisterUnique<HtmlLocalLinkParser>();
            Composition.RegisterUnique<HtmlUrlParser>();
            Composition.RegisterUnique<HtmlImageSourceParser>();
            Composition.RegisterUnique<RichTextEditorPastedImages>();

        }

        protected virtual void ComposeMisc()
        {
            // what else?
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            Composition.RegisterUnique(f => runtimeStateMock.Object);
            Composition.Register(_ => Mock.Of<IImageUrlGenerator>());

            // ah...
            Composition.WithCollectionBuilder<ActionCollectionBuilder>();
            Composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();
            Composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            Composition.RegisterUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            // register empty content apps collection
            Composition.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

            // manifest
            Composition.ManifestValueValidators();
            Composition.ManifestFilters();

        }

        protected virtual void ComposeMapper(bool configure)
        {
            if (configure == false) return;

            Composition
                .ComposeCoreMappingProfiles()
                .ComposeWebMappingProfiles();
        }

        protected virtual TypeLoader GetTypeLoader(IAppPolicyCache runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger, UmbracoTestOptions.TypeLoader option)
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

        protected virtual TypeLoader CreateTypeLoader(IAppPolicyCache runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            return CreateCommonTypeLoader(runtimeCache, globalSettings, logger);
        }

        // common to all tests = cannot be overriden
        private static TypeLoader CreateCommonTypeLoader(IAppPolicyCache runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger)
        {
            return new TypeLoader(runtimeCache, globalSettings.LocalTempPath, logger, false)
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

        protected virtual void ComposeSettings()
        {
            Composition.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);
            Composition.Configs.Add(SettingsForTests.GetDefaultGlobalSettings);
        }

        protected virtual void ComposeApplication(bool withApplication)
        {
            ComposeSettings();

            if (withApplication == false) return;

            // default Datalayer/Repositories/SQL/Database/etc...
            Composition.ComposeRepositories();

            // register basic stuff that might need to be there for some container resolvers to work
            Composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().Content);
            Composition.RegisterUnique(factory => factory.GetInstance<IUmbracoSettingsSection>().WebRouting);

            Composition.RegisterUnique<IExamineManager>(factory => ExamineManager.Instance);

            // register filesystems
            Composition.RegisterUnique(factory => TestObjects.GetFileSystemsMock());

            var logger = Mock.Of<ILogger>();
            var scheme = Mock.Of<IMediaPathScheme>();
            var config = Mock.Of<IContentSection>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), config, scheme, logger);
            Composition.RegisterUnique<IMediaFileSystem>(factory => mediaFileSystem);

            // no factory (noop)
            Composition.RegisterUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // register application stuff (database factory & context, services...)
            Composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            Composition.RegisterUnique<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());
            Composition.RegisterUnique<IUmbracoDatabaseFactory>(f => new UmbracoDatabaseFactory(
                Constants.System.UmbracoConnectionName,
                Logger,
                new Lazy<IMapperCollection>(f.GetInstance<IMapperCollection>)));
            Composition.RegisterUnique(f => f.TryGetInstance<IUmbracoDatabaseFactory>().SqlContext);

            Composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty

            Composition.RegisterUnique(factory
                => TestObjects.GetScopeProvider(factory.TryGetInstance<ILogger>(), factory.TryGetInstance<FileSystems>(), factory.TryGetInstance<IUmbracoDatabaseFactory>()));
            Composition.RegisterUnique(factory => (IScopeAccessor) factory.GetInstance<IScopeProvider>());

            Composition.ComposeServices();

            // composition root is doing weird things, fix
            Composition.RegisterUnique<ITreeService, TreeService>();
            Composition.RegisterUnique<ISectionService, SectionService>();

            // somehow property editor ends up wanting this
            Composition.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();
            Composition.RegisterUnique<ManifestParser>();

            // note - don't register collections, use builders
            Composition.WithCollectionBuilder<DataEditorCollectionBuilder>();
            Composition.RegisterUnique<PropertyEditorCollection>();
            Composition.RegisterUnique<ParameterEditorCollection>();
        }

        #endregion

        #region Initialize

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
            if (Factory?.TryGetInstance<IScopeProvider>() is ScopeProvider scopeProvider)
            {
                Scope scope;
                while ((scope = scopeProvider.AmbientScope) != null)
                {
                    scope.Reset();
                    scope.Dispose();
                }
            }

            Current.Reset(); // disposes the factory

            // reset all other static things that should not be static ;(
            UriUtility.ResetAppDomainAppVirtualPath();
            SettingsForTests.Reset(); // FIXME: should it be optional?

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
