using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using System.Web.Security;
using System.Xml.Linq;
using Examine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Serilog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Media;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Hosting;
using Umbraco.Web.Security;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

        protected IUmbracoBuilder Builder { get; private set; }

        protected IServiceProvider Factory { get; private set; }

        protected UmbracoTestAttribute Options { get; private set; }

        protected static bool FirstTestInSession = true;

        protected bool FirstTestInFixture = true;

        internal TestObjects TestObjects { get; private set; }

        private static TypeLoader _commonTypeLoader;

        private TypeLoader _featureTypeLoader;

        #region Accessors
        protected ServiceContext ServiceContext => Factory.GetRequiredService<ServiceContext>();

        protected ILoggerFactory LoggerFactory => Factory.GetRequiredService<ILoggerFactory>();

        protected IJsonSerializer JsonNetSerializer { get; } = new JsonNetSerializer();

        protected IIOHelper IOHelper { get; private set; }
        protected UriUtility UriUtility => new UriUtility(HostingEnvironment);
        protected IPublishedUrlProvider PublishedUrlProvider => Factory.GetRequiredService<IPublishedUrlProvider>();
        protected IDataTypeService DataTypeService => Factory.GetRequiredService<IDataTypeService>();
        protected IPasswordHasher PasswordHasher => Factory.GetRequiredService<IPasswordHasher>();
        protected Lazy<PropertyEditorCollection> PropertyEditorCollection => new Lazy<PropertyEditorCollection>(() => Factory.GetRequiredService<PropertyEditorCollection>());
        protected ILocalizationService LocalizationService => Factory.GetRequiredService<ILocalizationService>();
        protected ILocalizedTextService LocalizedTextService { get; private set; }
        protected IShortStringHelper ShortStringHelper => Factory?.GetRequiredService<IShortStringHelper>() ?? TestHelper.ShortStringHelper;
        protected IImageUrlGenerator ImageUrlGenerator => Factory.GetRequiredService<IImageUrlGenerator>();
        protected UploadAutoFillProperties UploadAutoFillProperties => Factory.GetRequiredService<UploadAutoFillProperties>();
        protected IUmbracoVersion UmbracoVersion { get; private set; }

        protected ITypeFinder TypeFinder { get; private set; }

        protected IProfiler Profiler => Factory.GetRequiredService<IProfiler>();

        protected virtual IProfilingLogger ProfilingLogger => Factory.GetRequiredService<IProfilingLogger>();

        protected IHostingEnvironment HostingEnvironment { get; } = new AspNetHostingEnvironment(Microsoft.Extensions.Options.Options.Create(new HostingSettings()));
        protected IApplicationShutdownRegistry HostingLifetime { get; } = new AspNetApplicationShutdownRegistry();
        protected IIpResolver IpResolver => Factory.GetRequiredService<IIpResolver>();
        protected IBackOfficeInfo BackOfficeInfo => Factory.GetRequiredService<IBackOfficeInfo>();
        protected AppCaches AppCaches => Factory.GetRequiredService<AppCaches>();

        protected virtual ISqlSyntaxProvider SqlSyntax => Factory.GetRequiredService<ISqlSyntaxProvider>();

        protected IMapperCollection Mappers => Factory.GetRequiredService<IMapperCollection>();

        protected UmbracoMapper Mapper => Factory.GetRequiredService<UmbracoMapper>();
        protected IHttpContextAccessor HttpContextAccessor => Factory.GetRequiredService<IHttpContextAccessor>();
        protected IRuntimeState RuntimeState => MockRuntimeState(RuntimeLevel.Run);
        private ILoggerFactory _loggerFactory;

        protected static IRuntimeState MockRuntimeState(RuntimeLevel level)
        {
            var runtimeState = Mock.Of<IRuntimeState>();
            Mock.Get(runtimeState).Setup(x => x.Level).Returns(level);
            return runtimeState;
        }
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

            var loggerFactory = GetLoggerFactory(Options.Logger);
            _loggerFactory = loggerFactory;
            var profiler = new LogProfiler(loggerFactory.CreateLogger<LogProfiler>());
            var msLogger = loggerFactory.CreateLogger("msLogger");
            var proflogger = new ProfilingLogger(loggerFactory.CreateLogger<ProfilingLogger>(), profiler);
            IOHelper = TestHelper.IOHelper;

            TypeFinder = new TypeFinder(loggerFactory.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly, loggerFactory), new VaryingRuntimeHash());
            var appCaches = GetAppCaches();
            var globalSettings = new GlobalSettings();
            var settings = new WebRoutingSettings();

            IBackOfficeInfo backOfficeInfo = new AspNetBackOfficeInfo(globalSettings, IOHelper, loggerFactory.CreateLogger<AspNetBackOfficeInfo>(), Microsoft.Extensions.Options.Options.Create(settings));
            IIpResolver ipResolver = new AspNetIpResolver();
            UmbracoVersion = new UmbracoVersion();


            LocalizedTextService = new LocalizedTextService(new Dictionary<CultureInfo, Lazy<XDocument>>(), loggerFactory.CreateLogger<LocalizedTextService>());
            var typeLoader = GetTypeLoader(IOHelper, TypeFinder, appCaches.RuntimeCache, HostingEnvironment, loggerFactory.CreateLogger<TypeLoader>(), proflogger, Options.TypeLoader);

            var services = TestHelper.GetRegister();

            Builder = new UmbracoBuilder(services, Mock.Of<IConfiguration>(), typeLoader);

            //TestHelper.GetConfigs().RegisterWith(register);
            services.AddUnique(typeof(ILoggerFactory), loggerFactory);
            services.AddTransient(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<ILogger>(msLogger);
            services.AddUnique(IOHelper);
            services.AddUnique(UriUtility);
            services.AddUnique(UmbracoVersion);
            services.AddUnique(TypeFinder);
            services.AddUnique(LocalizedTextService);
            services.AddUnique(typeLoader);
            services.AddUnique<IProfiler>(profiler);
            services.AddUnique<IProfilingLogger>(proflogger);
            services.AddUnique(appCaches);
            services.AddUnique(HostingEnvironment);
            services.AddUnique(backOfficeInfo);
            services.AddUnique(ipResolver);
            services.AddUnique<IPasswordHasher, AspNetPasswordHasher>();
            services.AddUnique(TestHelper.ShortStringHelper);
            services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();


            var memberService = Mock.Of<IMemberService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            var membershipHelper = new MembershipHelper(Mock.Of<IHttpContextAccessor>(), Mock.Of<IPublishedMemberCache>(), memberService, memberTypeService, Mock.Of<IPublicAccessService>(), AppCaches.Disabled, loggerFactory, ShortStringHelper, Mock.Of<IEntityService>(), Mock.Of<IIpResolver>(), Mock.Of<IMemberManager>());

            services.AddUnique(membershipHelper);

            TestObjects = new TestObjects();
            Compose();
            Current.Factory = Factory = TestHelper.CreateServiceProvider(Builder);
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
            Compose(Builder);
        }

        protected virtual void Compose(IUmbracoBuilder builder)
        { }

        protected virtual void Initialize()
        {
            InitializeApplication(Options.WithApplication);
        }

        #endregion

        #region Compose

        protected virtual ILoggerFactory GetLoggerFactory(UmbracoTestOptions.Logger option)
        {
            ILoggerFactory factory;

            switch (option)
            {
                case UmbracoTestOptions.Logger.Mock:
                    factory = NullLoggerFactory.Instance;
                    break;
                case UmbracoTestOptions.Logger.Serilog:
                    factory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { builder.AddSerilog(); });
                    break;
                case UmbracoTestOptions.Logger.Console:
                    factory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { builder.AddConsole(); });
                    break;
                default:
                    throw new NotSupportedException($"Logger option {option} is not supported.");
            }

            return factory;
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
            Builder.Services.AddUnique(Current.UmbracoContextAccessor);
            Builder.Services.AddUnique<IBackOfficeSecurityAccessor>(new HybridBackofficeSecurityAccessor(AppCaches.NoCache.RequestCache));
            Builder.Services.AddUnique<IPublishedRouter, PublishedRouter>();
            Builder.WithCollectionBuilder<ContentFinderCollectionBuilder>();


            Builder.DataValueReferenceFactories();

            Builder.Services.AddUnique<IContentLastChanceFinder, TestLastChanceFinder>();
            Builder.Services.AddUnique<IVariationContextAccessor, TestVariationContextAccessor>();
            Builder.Services.AddUnique<IPublishedSnapshotAccessor, TestPublishedSnapshotAccessor>();
            Builder.Services.AddUnique<IImageDimensionExtractor, ImageDimensionExtractor>();
            Builder.Services.AddUnique<IPublishedSnapshotAccessor, TestPublishedSnapshotAccessor>();
            Builder.SetCultureDictionaryFactory<DefaultCultureDictionaryFactory>();
            Builder.Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());
            // register back office sections in the order we want them rendered
            Builder.WithCollectionBuilder<SectionCollectionBuilder>().Append<ContentSection>()
                .Append<MediaSection>()
                .Append<SettingsSection>()
                .Append<PackagesSection>()
                .Append<UsersSection>()
                .Append<MembersSection>()
                .Append<FormsSection>()
                .Append<TranslationSection>();
            Builder.Services.AddUnique<ISectionService, SectionService>();

            Builder.Services.AddUnique<HtmlLocalLinkParser>();
            Builder.Services.AddUnique<IBackOfficeSecurity, BackOfficeSecurity>();
            Builder.Services.AddUnique<IEmailSender, EmailSender>();
            Builder.Services.AddUnique<HtmlUrlParser>();
            Builder.Services.AddUnique<HtmlImageSourceParser>();
            Builder.Services.AddUnique<RichTextEditorPastedImages>();
            Builder.Services.AddUnique<IPublishedValueFallback, NoopPublishedValueFallback>();

            var webRoutingSettings = new WebRoutingSettings();
            Builder.Services.AddUnique<IPublishedUrlProvider>(factory =>
                new UrlProvider(
                    factory.GetRequiredService<IUmbracoContextAccessor>(),
                    Microsoft.Extensions.Options.Options.Create(webRoutingSettings),
                    new UrlProviderCollection(Enumerable.Empty<IUrlProvider>()),
                    new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                    factory.GetRequiredService<IVariationContextAccessor>()));



        }

        protected virtual void ComposeMisc()
        {
            // what else?
            var runtimeStateMock = new Mock<IRuntimeState>();
            runtimeStateMock.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            Builder.Services.AddUnique(f => runtimeStateMock.Object);
            Builder.Services.AddTransient(_ => Mock.Of<IImageUrlGenerator>());
            Builder.Services.AddTransient<UploadAutoFillProperties>();

            // ah...
            Builder.WithCollectionBuilder<ActionCollectionBuilder>();
            Builder.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();
            Builder.Services.AddUnique<PropertyEditorCollection>();
            Builder.Services.AddUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            Builder.Services.AddUnique<IMediaPathScheme, UniqueMediaPathScheme>();

            // register empty content apps collection
            Builder.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

            // manifest
            Builder.ManifestValueValidators();
            Builder.ManifestFilters();
            Builder.MediaUrlGenerators()
                .Add<FileUploadPropertyEditor>()
                .Add<ImageCropperPropertyEditor>();

        }

        protected virtual void ComposeMapper(bool configure)
        {
            if (configure == false) return;

            Builder
                .AddCoreMappingProfiles();
        }

        protected virtual TypeLoader GetTypeLoader(IIOHelper ioHelper, ITypeFinder typeFinder, IAppPolicyCache runtimeCache, IHostingEnvironment hostingEnvironment, ILogger<TypeLoader> logger, IProfilingLogger profilingLogger, UmbracoTestOptions.TypeLoader option)
        {
            switch (option)
            {
                case UmbracoTestOptions.TypeLoader.Default:
                    return _commonTypeLoader ?? (_commonTypeLoader = CreateCommonTypeLoader(typeFinder, runtimeCache, logger, profilingLogger, hostingEnvironment));
                case UmbracoTestOptions.TypeLoader.PerFixture:
                    return _featureTypeLoader ?? (_featureTypeLoader = CreateTypeLoader(ioHelper, typeFinder, runtimeCache, logger, profilingLogger, hostingEnvironment));
                case UmbracoTestOptions.TypeLoader.PerTest:
                    return CreateTypeLoader(ioHelper, typeFinder, runtimeCache, logger, profilingLogger, hostingEnvironment);
                default:
                    throw new ArgumentOutOfRangeException(nameof(option));
            }
        }

        protected virtual TypeLoader CreateTypeLoader(IIOHelper ioHelper, ITypeFinder typeFinder, IAppPolicyCache runtimeCache, ILogger<TypeLoader> logger, IProfilingLogger profilingLogger, IHostingEnvironment hostingEnvironment)
        {
            return CreateCommonTypeLoader(typeFinder, runtimeCache, logger, profilingLogger, hostingEnvironment);
        }

        // common to all tests = cannot be overriden
        private static TypeLoader CreateCommonTypeLoader(ITypeFinder typeFinder, IAppPolicyCache runtimeCache, ILogger<TypeLoader> logger, IProfilingLogger profilingLogger, IHostingEnvironment hostingEnvironment)
        {
            return new TypeLoader(typeFinder, runtimeCache, new DirectoryInfo(hostingEnvironment.LocalTempPath), logger, profilingLogger, false, new[]
            {
                Assembly.Load("Umbraco.Core"),
                Assembly.Load("Umbraco.Web"),
                Assembly.Load("Umbraco.Tests"),
                Assembly.Load("Umbraco.Infrastructure"),
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
            var contentSettings = new ContentSettings();
            var coreDebugSettings = new CoreDebugSettings();
            var globalSettings = new GlobalSettings();
            var nuCacheSettings = new NuCacheSettings();
            var requestHandlerSettings = new RequestHandlerSettings();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettings();
            var webRoutingSettings = new WebRoutingSettings();

            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(contentSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(coreDebugSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(globalSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(nuCacheSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(requestHandlerSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(userPasswordConfigurationSettings));
            Builder.Services.AddTransient(x => Microsoft.Extensions.Options.Options.Create(webRoutingSettings));
        }

        protected virtual void ComposeApplication(bool withApplication)
        {
            ComposeSettings();

            if (withApplication == false) return;

            // default Datalayer/Repositories/SQL/Database/etc...
            Builder.AddRepositories();

            Builder.Services.AddUnique<IExamineManager, ExamineManager>();

            Builder.Services.AddUnique<IJsonSerializer, JsonNetSerializer>();
            Builder.Services.AddUnique<IConfigurationEditorJsonSerializer, ConfigurationEditorJsonSerializer>();
            Builder.Services.AddUnique<IMenuItemCollectionFactory, MenuItemCollectionFactory>();
            Builder.Services.AddUnique<InstallStatusTracker>();

            // register filesystems
            Builder.Services.AddUnique(factory => TestObjects.GetFileSystemsMock());


            var scheme = Mock.Of<IMediaPathScheme>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), scheme, _loggerFactory.CreateLogger<MediaFileSystem>(), TestHelper.ShortStringHelper);
            Builder.Services.AddUnique<IMediaFileSystem>(factory => mediaFileSystem);

            // no factory (noop)
            Builder.Services.AddUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // register application stuff (database factory & context, services...)
            Builder.WithCollectionBuilder<MapperCollectionBuilder>()
                .AddCoreMappers();

            Builder.Services.AddUnique<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());

            var globalSettings = new GlobalSettings();
            var connectionStrings = new ConnectionStrings();

            Builder.Services.AddUnique<IUmbracoDatabaseFactory>(f => new UmbracoDatabaseFactory(_loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                LoggerFactory,
                globalSettings,
                connectionStrings,
                new Lazy<IMapperCollection>(f.GetRequiredService<IMapperCollection>),
                TestHelper.DbProviderFactoryCreator,
                new DatabaseSchemaCreatorFactory(LoggerFactory.CreateLogger<DatabaseSchemaCreator>(), LoggerFactory, UmbracoVersion)));

            Builder.Services.AddUnique(f => f.GetService<IUmbracoDatabaseFactory>().SqlContext);

            Builder.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty

            Builder.Services.AddUnique(factory
                => TestObjects.GetScopeProvider(_loggerFactory, factory.GetService<FileSystems>(), factory.GetService<IUmbracoDatabaseFactory>()));
            Builder.Services.AddUnique(factory => (IScopeAccessor)factory.GetRequiredService<IScopeProvider>());

            Builder.AddServices();

            // composition root is doing weird things, fix
            Builder.Services.AddUnique<ITreeService, TreeService>();
            Builder.Services.AddUnique<ISectionService, SectionService>();

            // somehow property editor ends up wanting this
            Builder.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();

            Builder.Services.AddUnique<IManifestParser, ManifestParser>();

            // note - don't register collections, use builders
            Builder.WithCollectionBuilder<DataEditorCollectionBuilder>();
            Builder.Services.AddUnique<PropertyEditorCollection>();
            Builder.Services.AddUnique<ParameterEditorCollection>();


            Builder.Services.AddUnique<IHttpContextAccessor>(TestHelper.GetHttpContextAccessor(GetHttpContextFactory("/").HttpContext));
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
            try
            {
                // reset and dispose scopes
                // ensures we don't leak an opened database connection
                // which would lock eg SqlCe .sdf files
                if (Factory?.GetService<IScopeProvider>() is ScopeProvider scopeProvider)
                {
                    Scope scope;
                    while ((scope = scopeProvider.AmbientScope) != null)
                    {
                        scope.Reset();
                        scope.Dispose();
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (!ex.ObjectName.Equals(nameof(IServiceProvider)))
                    throw;
            }

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
