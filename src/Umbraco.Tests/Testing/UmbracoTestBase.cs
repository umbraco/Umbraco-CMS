﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using System.Web.Security;
using System.Xml.Linq;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Manifest;
using Umbraco.Core.Mapping;
using Umbraco.Core.Media;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Net;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Actions;
using Umbraco.Web.AspNet;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Hosting;
using Umbraco.Web.Install;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Sections;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.Services;
using Umbraco.Web.Templates;
using Umbraco.Web.Trees;
using Current = Umbraco.Web.Composing.Current;
using FileSystems = Umbraco.Core.IO.FileSystems;

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
        protected ServiceContext ServiceContext => Factory.GetInstance<ServiceContext>();

        protected ILogger Logger => Factory.GetInstance<ILogger>();
        protected IJsonSerializer JsonNetSerializer { get; } = new JsonNetSerializer();

        protected IIOHelper IOHelper { get; private set; }
        protected UriUtility UriUtility => new UriUtility(HostingEnvironment);
        protected IPublishedUrlProvider PublishedUrlProvider => Factory.GetInstance<IPublishedUrlProvider>();
        protected IDataTypeService DataTypeService => Factory.GetInstance<IDataTypeService>();
        protected IPasswordHasher PasswordHasher => Factory.GetInstance<IPasswordHasher>();
        protected Lazy<PropertyEditorCollection> PropertyEditorCollection => new Lazy<PropertyEditorCollection>(() => Factory.GetInstance<PropertyEditorCollection>());
        protected ILocalizationService LocalizationService => Factory.GetInstance<ILocalizationService>();
        protected ILocalizedTextService LocalizedTextService  { get; private set; }
        protected IShortStringHelper ShortStringHelper => Factory?.GetInstance<IShortStringHelper>() ?? TestHelper.ShortStringHelper;
        protected IUmbracoVersion UmbracoVersion { get; private set; }

        protected ITypeFinder TypeFinder { get; private set; }

        protected IProfiler Profiler => Factory.GetInstance<IProfiler>();

        protected virtual IProfilingLogger ProfilingLogger => Factory.GetInstance<IProfilingLogger>();

        protected IHostingEnvironment HostingEnvironment { get; } = new AspNetHostingEnvironment(Microsoft.Extensions.Options.Options.Create(new HostingSettings()));
        protected IApplicationShutdownRegistry HostingLifetime { get; } = new AspNetApplicationShutdownRegistry();
        protected IIpResolver IpResolver => Factory.GetInstance<IIpResolver>();
        protected IBackOfficeInfo BackOfficeInfo => Factory.GetInstance<IBackOfficeInfo>();
        protected AppCaches AppCaches => Factory.GetInstance<AppCaches>();

        protected virtual ISqlSyntaxProvider SqlSyntax => Factory.GetInstance<ISqlSyntaxProvider>();

        protected IMapperCollection Mappers => Factory.GetInstance<IMapperCollection>();

        protected UmbracoMapper Mapper => Factory.GetInstance<UmbracoMapper>();
        protected IHttpContextAccessor HttpContextAccessor => Factory.GetInstance<IHttpContextAccessor>();
        protected IRuntimeState RuntimeState => ComponentTests.MockRuntimeState(RuntimeLevel.Run);

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

            // FIXME: align to runtimes & components - don't redo everything here !!!! Yes this is getting painful

            var (logger, profiler) = GetLoggers(Options.Logger);
            var proflogger = new ProfilingLogger(logger, profiler);
            IOHelper = TestHelper.IOHelper;

            TypeFinder = new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            var appCaches = GetAppCaches();
            var globalSettings = new GlobalSettingsBuilder().Build();
            var settings = new WebRoutingSettings();

            IBackOfficeInfo backOfficeInfo = new AspNetBackOfficeInfo(globalSettings, IOHelper, logger, Microsoft.Extensions.Options.Options.Create(settings));
            IIpResolver ipResolver = new AspNetIpResolver();
            UmbracoVersion = new UmbracoVersion();


            LocalizedTextService = new LocalizedTextService(new Dictionary<CultureInfo, Lazy<XDocument>>(), logger);
            var typeLoader = GetTypeLoader(IOHelper, TypeFinder, appCaches.RuntimeCache, HostingEnvironment, proflogger, Options.TypeLoader);

            var register = TestHelper.GetRegister();



            Composition = new Composition(register, typeLoader, proflogger, ComponentTests.MockRuntimeState(RuntimeLevel.Run), TestHelper.IOHelper, AppCaches.NoCache);


            //TestHelper.GetConfigs().RegisterWith(register);


            Composition.RegisterUnique(IOHelper);
            Composition.RegisterUnique(UriUtility);
            Composition.RegisterUnique(UmbracoVersion);
            Composition.RegisterUnique(TypeFinder);
            Composition.RegisterUnique(LocalizedTextService);
            Composition.RegisterUnique(typeLoader);
            Composition.RegisterUnique(logger);
            Composition.RegisterUnique(profiler);
            Composition.RegisterUnique<IProfilingLogger>(proflogger);
            Composition.RegisterUnique(appCaches);
            Composition.RegisterUnique(HostingEnvironment);
            Composition.RegisterUnique(backOfficeInfo);
            Composition.RegisterUnique(ipResolver);
            Composition.RegisterUnique<IPasswordHasher, AspNetPasswordHasher>();
            Composition.RegisterUnique(TestHelper.ShortStringHelper);
            Composition.RegisterUnique<IRequestAccessor, AspNetRequestAccessor>();
            Composition.RegisterUnique<IPublicAccessChecker, PublicAccessChecker>();


            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipProvider = new MembersMembershipProvider(memberService, memberTypeService, Mock.Of<IUmbracoVersion>(), TestHelper.GetHostingEnvironment(), TestHelper.GetIpResolver());
            var membershipHelper = new MembershipHelper(Mock.Of<IHttpContextAccessor>(), Mock.Of<IPublishedMemberCache>(), membershipProvider, Mock.Of<RoleProvider>(), memberService, memberTypeService, Mock.Of<IPublicAccessService>(), AppCaches.Disabled, logger, ShortStringHelper, Mock.Of<IEntityService>());

            Composition.RegisterUnique(membershipHelper);




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
                    logger = new SerilogLogger(new FileInfo(TestHelper.MapPathForTestFiles("~/unit-test.config")));
                    profiler = new LogProfiler(logger);
                    break;
                case UmbracoTestOptions.Logger.Console:
                    logger = new ConsoleLogger(new MessageTemplates());
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
            Composition.SetCultureDictionaryFactory<DefaultCultureDictionaryFactory>();
            Composition.Register(f => f.GetInstance<ICultureDictionaryFactory>().CreateDictionary(), Lifetime.Singleton);
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
            Composition.RegisterUnique<IBackofficeSecurity, BackofficeSecurity>();
            Composition.RegisterUnique<IEmailSender, EmailSender>();
            Composition.RegisterUnique<HtmlUrlParser>();
            Composition.RegisterUnique<HtmlImageSourceParser>();
            Composition.RegisterUnique<RichTextEditorPastedImages>();
            Composition.RegisterUnique<IPublishedValueFallback, NoopPublishedValueFallback>();

            var webRoutingSettings = new WebRoutingSettingsBuilder().Build();
            Composition.RegisterUnique<IPublishedUrlProvider>(factory =>
                new UrlProvider(
                    factory.GetInstance<IUmbracoContextAccessor>(),
                    Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                    new UrlProviderCollection(Enumerable.Empty<IUrlProvider>()),
                    new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                    factory.GetInstance<IVariationContextAccessor>()));



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
            Composition.RegisterUnique<PropertyEditorCollection>();
            Composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            Composition.RegisterUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            // register empty content apps collection
            Composition.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

            // manifest
            Composition.ManifestValueValidators();
            Composition.ManifestFilters();
            Composition.MediaUrlGenerators()
                .Add<FileUploadPropertyEditor>()
                .Add<ImageCropperPropertyEditor>();

        }

        protected virtual void ComposeMapper(bool configure)
        {
            if (configure == false) return;

            Composition
                .ComposeCoreMappingProfiles();
        }

        protected virtual TypeLoader GetTypeLoader(IIOHelper ioHelper, ITypeFinder typeFinder, IAppPolicyCache runtimeCache, IHostingEnvironment hostingEnvironment, IProfilingLogger logger, UmbracoTestOptions.TypeLoader option)
        {
            switch (option)
            {
                case UmbracoTestOptions.TypeLoader.Default:
                    return _commonTypeLoader ?? (_commonTypeLoader = CreateCommonTypeLoader(typeFinder, runtimeCache, logger, hostingEnvironment));
                case UmbracoTestOptions.TypeLoader.PerFixture:
                    return _featureTypeLoader ?? (_featureTypeLoader = CreateTypeLoader(ioHelper, typeFinder, runtimeCache, logger, hostingEnvironment));
                case UmbracoTestOptions.TypeLoader.PerTest:
                    return CreateTypeLoader(ioHelper, typeFinder, runtimeCache, logger, hostingEnvironment);
                default:
                    throw new ArgumentOutOfRangeException(nameof(option));
            }
        }

        protected virtual TypeLoader CreateTypeLoader(IIOHelper ioHelper, ITypeFinder typeFinder, IAppPolicyCache runtimeCache, IProfilingLogger logger, IHostingEnvironment hostingEnvironment)
        {
            return CreateCommonTypeLoader(typeFinder, runtimeCache, logger, hostingEnvironment);
        }

        // common to all tests = cannot be overriden
        private static TypeLoader CreateCommonTypeLoader(ITypeFinder typeFinder, IAppPolicyCache runtimeCache, IProfilingLogger logger, IHostingEnvironment hostingEnvironment)
        {
            return new TypeLoader(typeFinder, runtimeCache, new DirectoryInfo(hostingEnvironment.LocalTempPath), logger, false, new[]
            {
                Assembly.Load("Umbraco.Core"),
                Assembly.Load("Umbraco.Web"),
                Assembly.Load("Umbraco.Tests"),
                Assembly.Load("Umbraco.Infrastructure")
            });
        }

        protected virtual void ComposeDatabase(UmbracoTestOptions.Database option)
        {
            if (option == UmbracoTestOptions.Database.None) return;

            // create the file
            // create the schema
        }

        protected virtual void ComposeSettings()
        {
            var contentSettings = new ContentSettingsBuilder().Build();
            var coreDebugSettings = new CoreDebugSettingsBuilder().Build();
            var globalSettings = new GlobalSettingsBuilder().Build();
            var nuCacheSettings = new NuCacheSettingsBuilder().Build();
            var requestHandlerSettings = new RequestHandlerSettingsBuilder().Build();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettingsBuilder().Build();
            var webRoutingSettings = new WebRoutingSettingsBuilder().Build();

            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(contentSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(coreDebugSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(globalSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(nuCacheSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(requestHandlerSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(userPasswordConfigurationSettings));
            Composition.Register(x => Microsoft.Extensions.Options.Options.Create(webRoutingSettings));
        }

        protected virtual void ComposeApplication(bool withApplication)
        {
            ComposeSettings();

            if (withApplication == false) return;

            // default Datalayer/Repositories/SQL/Database/etc...
            Composition.ComposeRepositories();

            Composition.RegisterUnique<IExamineManager, ExamineManager>();

            Composition.RegisterUnique<IJsonSerializer, JsonNetSerializer>();
            Composition.RegisterUnique<IMenuItemCollectionFactory, MenuItemCollectionFactory>();
            Composition.RegisterUnique<InstallStatusTracker>();

            // register filesystems
            Composition.RegisterUnique(factory => TestObjects.GetFileSystemsMock());


            var logger = Mock.Of<ILogger>();
            var scheme = Mock.Of<IMediaPathScheme>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), scheme, logger, TestHelper.ShortStringHelper);
            Composition.RegisterUnique<IMediaFileSystem>(factory => mediaFileSystem);

            // no factory (noop)
            Composition.RegisterUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // register application stuff (database factory & context, services...)
            Composition.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            Composition.RegisterUnique<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());

            var globalSettings = new GlobalSettingsBuilder().Build();
            var connectionStrings = new ConnectionStringsBuilder().Build();

            Composition.RegisterUnique<IUmbracoDatabaseFactory>(f => new UmbracoDatabaseFactory(Logger,
                globalSettings,
                connectionStrings,
                new Lazy<IMapperCollection>(f.GetInstance<IMapperCollection>),
                TestHelper.DbProviderFactoryCreator));

            Composition.RegisterUnique(f => f.TryGetInstance<IUmbracoDatabaseFactory>().SqlContext);

            Composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty

            Composition.RegisterUnique(factory
                => TestObjects.GetScopeProvider(factory.TryGetInstance<ILogger>(), factory.TryGetInstance<ITypeFinder>(), factory.TryGetInstance<FileSystems>(), factory.TryGetInstance<IUmbracoDatabaseFactory>()));
            Composition.RegisterUnique(factory => (IScopeAccessor) factory.GetInstance<IScopeProvider>());

            Composition.ComposeServices();

            // composition root is doing weird things, fix
            Composition.RegisterUnique<ITreeService, TreeService>();
            Composition.RegisterUnique<ISectionService, SectionService>();

            // somehow property editor ends up wanting this
            Composition.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();

            Composition.RegisterUnique<IManifestParser, ManifestParser>();

            // note - don't register collections, use builders
            Composition.WithCollectionBuilder<DataEditorCollectionBuilder>();
            Composition.RegisterUnique<PropertyEditorCollection>();
            Composition.RegisterUnique<ParameterEditorCollection>();


            Composition.RegisterUnique<IHttpContextAccessor>(TestHelper.GetHttpContextAccessor(GetHttpContextFactory("/").HttpContext));
        }

        #endregion

        protected FakeHttpContextFactory GetHttpContextFactory(string url, RouteData routeData = null)
        {
            var factory = routeData != null
                ? new FakeHttpContextFactory(url, routeData)
                : new FakeHttpContextFactory(url);

            return factory;
        }

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
            UriUtility.ResetAppDomainAppVirtualPath(HostingEnvironment);

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
