using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Examine;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Macros;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Runtime;
using File = System.IO.File;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Composing;
using Umbraco.Core.Media;
using Umbraco.Tests.Common.Builders;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class StandaloneTests
    {
        [Test]
        [Explicit("This test must be run manually")]
        public void StandaloneTest()
        {
            IFactory factory = null;

            // clear
            foreach (var file in Directory.GetFiles(Path.Combine(TestHelper.IOHelper.MapPath("~/App_Data")), "NuCache.*"))
                File.Delete(file);

            // settings
            // reset the current version to 0.0.0, clear connection strings
            ConfigurationManager.AppSettings[Constants.AppSettings.ConfigurationStatus] = "";
            // FIXME: we need a better management of settings here (and, true config files?)

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            // create the very basic and essential things we need
            var profiler = new LogProfiler(loggerFactory.CreateLogger<LogProfiler>());
            var profilingLogger = new ProfilingLogger(loggerFactory.CreateLogger("ProfilingLogger"), profiler);
            var appCaches = AppCaches.Disabled;
            var globalSettings = new GlobalSettings();
            var connectionStrings = new ConnectionStrings();
            var typeFinder = TestHelper.GetTypeFinder();
            var databaseFactory = new UmbracoDatabaseFactory(loggerFactory.CreateLogger<UmbracoDatabaseFactory>(), loggerFactory, Options.Create(globalSettings), Options.Create(connectionStrings), new Lazy<IMapperCollection>(() => factory.GetInstance<IMapperCollection>()),  TestHelper.DbProviderFactoryCreator);
            var ioHelper = TestHelper.IOHelper;
            var hostingEnvironment = Mock.Of<IHostingEnvironment>();
            var typeLoader = new TypeLoader(typeFinder, appCaches.RuntimeCache, new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")), loggerFactory.CreateLogger<TypeLoader>(), profilingLogger);
            var mainDom = new SimpleMainDom();
            var umbracoVersion = TestHelper.GetUmbracoVersion();
            var backOfficeInfo = TestHelper.GetBackOfficeInfo();
            var runtimeState = new RuntimeState(globalSettings, umbracoVersion, databaseFactory, loggerFactory.CreateLogger<RuntimeState>());
            var variationContextAccessor = TestHelper.VariationContextAccessor;

            // create the register and the composition
            var register = TestHelper.GetRegister();
            var composition = new Composition(register, typeLoader, profilingLogger, runtimeState, ioHelper, appCaches);
            composition.RegisterEssentials(loggerFactory.CreateLogger("Essentials"), loggerFactory, profiler, profilingLogger, mainDom, appCaches, databaseFactory, typeLoader, runtimeState, typeFinder, ioHelper, umbracoVersion, TestHelper.DbProviderFactoryCreator, hostingEnvironment, backOfficeInfo);

            // create the core runtime and have it compose itself
            var coreRuntime = new CoreRuntime(globalSettings, connectionStrings, umbracoVersion, ioHelper, loggerFactory.CreateLogger("CoreRunTime"), loggerFactory, profiler, new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, TestHelper.DbProviderFactoryCreator, TestHelper.MainDom, typeFinder, AppCaches.NoCache);

            // determine actual runtime level
            runtimeState.DetermineRuntimeLevel();
            Console.WriteLine(runtimeState.Level);
            // going to be Install BUT we want to force components to be there (nucache etc)
            runtimeState.Level = RuntimeLevel.Run;

            var composerTypes = typeLoader.GetTypes<IComposer>() // all of them
                .Where(x => !x.FullName.StartsWith("Umbraco.Tests.")) // exclude test components
                .Where(x => x != typeof(WebInitialComposer) && x != typeof(WebFinalComposer)); // exclude web runtime
            var composers = new Composers(composition, composerTypes, Enumerable.Empty<Attribute>(), loggerFactory.CreateLogger<Composers>(), profilingLogger);
            composers.Compose();

            // must registers stuff that WebRuntimeComponent would register otherwise
            // FIXME: UmbracoContext creates a snapshot that it does not register with the accessor
            //  and so, we have to use the UmbracoContextPublishedSnapshotAccessor
            //  the UmbracoContext does not know about the accessor
            //  else that would be a catch-22 where they both know about each other?
            //composition.Register<IPublishedSnapshotAccessor, TestPublishedSnapshotAccessor>(Lifetime.Singleton);
            composition.Register<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>(Lifetime.Singleton);
            composition.Register<IUmbracoContextAccessor, TestUmbracoContextAccessor>(Lifetime.Singleton);
            composition.Register<IVariationContextAccessor, TestVariationContextAccessor>(Lifetime.Singleton);
            composition.Register<IDefaultCultureAccessor, TestDefaultCultureAccessor>(Lifetime.Singleton);
            composition.Register<ISiteDomainHelper>(_ => Mock.Of<ISiteDomainHelper>(), Lifetime.Singleton);
            composition.RegisterUnique(f => new DistributedCache(f.GetInstance<IServerMessenger>(), f.GetInstance<CacheRefresherCollection>()));
            composition.Register(_ => Mock.Of<IImageUrlGenerator>(), Lifetime.Singleton);
            composition.WithCollectionBuilder<UrlProviderCollectionBuilder>().Append<DefaultUrlProvider>();
            composition.RegisterUnique<IDistributedCacheBinder, DistributedCacheBinder>();
            composition.RegisterUnique<IExamineManager, ExamineManager>();
            composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();
            composition.RegisterUnique<MediaUrlProviderCollection>(_ => new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()));

            // TODO: found these registration were necessary here as dependencies for ComponentCollection
            // are not resolved.  Need to check this if these explicit registrations are the best way to handle this.
            var contentSettings = new ContentSettings();
            var coreDebugSettings = new CoreDebugSettings();
            var nuCacheSettings = new NuCacheSettings();
            var requestHandlerSettings = new RequestHandlerSettings();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettings();
            var webRoutingSettings = new WebRoutingSettings();

            composition.Register(x => Options.Create(globalSettings));
            composition.Register(x => Options.Create(contentSettings));
            composition.Register(x => Options.Create(coreDebugSettings));
            composition.Register(x => Options.Create(nuCacheSettings));
            composition.Register(x => Options.Create(requestHandlerSettings));
            composition.Register(x => Options.Create(userPasswordConfigurationSettings));
            composition.Register(x => Options.Create(webRoutingSettings));

            // initialize some components only/individually
            composition.WithCollectionBuilder<ComponentCollectionBuilder>()
                .Clear()
                .Append<DistributedCacheBinderComponent>();

            // configure

            // create and register the factory
            Current.Factory = factory = composition.CreateFactory();

            // instantiate and initialize components
            var components = factory.GetInstance<ComponentCollection>();

            // do stuff
            Console.WriteLine(runtimeState.Level);

            // install
            if (true || runtimeState.Level == RuntimeLevel.Install)
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var file = databaseFactory.Configured ? Path.Combine(path, "UmbracoNPocoTests.sdf") : Path.Combine(path, "Umbraco.sdf");
                if (File.Exists(file))
                    File.Delete(file);

                // create the database file
                // databaseBuilder.ConfigureEmbeddedDatabaseConnection() can do it too,
                // but then it wants to write the connection string to web.config = bad
                var connectionString = databaseFactory.Configured ? databaseFactory.ConnectionString : "Data Source=|DataDirectory|\\Umbraco.sdf;Flush Interval=1;";
                using (var engine = new SqlCeEngine(connectionString))
                {
                    engine.CreateDatabase();
                }

                //var databaseBuilder = factory.GetInstance<DatabaseBuilder>();
                //databaseFactory.Configure(DatabaseBuilder.EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe);
                //databaseBuilder.CreateDatabaseSchemaAndData();

                if (!databaseFactory.Configured)
                    databaseFactory.Configure(DatabaseBuilder.EmbeddedDatabaseConnectionString, Constants.DbProviderNames.SqlCe);

                var scopeProvider = factory.GetInstance<IScopeProvider>();
                using (var scope = scopeProvider.CreateScope())
                {
                    var creator = new DatabaseSchemaCreator(scope.Database, loggerFactory.CreateLogger<DatabaseSchemaCreator>(), loggerFactory, umbracoVersion);
                    creator.InitializeDatabaseSchema();
                    scope.Complete();
                }
            }

            // done installing
            runtimeState.Level = RuntimeLevel.Run;

            components.Initialize();

            // instantiate to register events
            // should be done by Initialize?
            // should we invoke Initialize?
            _ = factory.GetInstance<IPublishedSnapshotService>();

            // at that point, Umbraco can run!
            // though, we probably still need to figure out what depends on HttpContext...
            var contentService = factory.GetInstance<IContentService>();
            var content = contentService.GetById(1234);
            Assert.IsNull(content);

            // create a document type and a document
            var contentType = new ContentType(TestHelper.ShortStringHelper, -1) { Alias = "ctype", Name = "ctype" };
            factory.GetInstance<IContentTypeService>().Save(contentType);
            content = new Content("test", -1, contentType);
            contentService.Save(content);

            // assert that it is possible to get the document back
            content = contentService.GetById(content.Id);
            Assert.IsNotNull(content);
            Assert.AreEqual("test", content.Name);

            // need an UmbracoCOntext to access the cache
            var umbracoContextFactory = factory.GetInstance<IUmbracoContextFactory>();
            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            var umbracoContext = umbracoContextReference.UmbracoContext;

            // assert that there is no published document
            var pcontent = umbracoContext.Content.GetById(content.Id);
            Assert.IsNull(pcontent);

            // but a draft document
            pcontent = umbracoContext.Content.GetById(true, content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("test", pcontent.Name(variationContextAccessor));
            Assert.IsTrue(pcontent.IsDraft());

            // no published url
            Assert.AreEqual("#", pcontent.Url());

            // now publish the document + make some unpublished changes
            contentService.SaveAndPublish(content);
            content.Name = "testx";
            contentService.Save(content);

            // assert that snapshot has been updated and there is now a published document
            pcontent = umbracoContext.Content.GetById(content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("test", pcontent.Name(variationContextAccessor));
            Assert.IsFalse(pcontent.IsDraft());

            // but the url is the published one - no draft url
            Assert.AreEqual("/test/", pcontent.Url());

            // and also an updated draft document
            pcontent = umbracoContext.Content.GetById(true, content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("testx", pcontent.Name(variationContextAccessor));
            Assert.IsTrue(pcontent.IsDraft());

            // and the published document has a url
            Assert.AreEqual("/test/", pcontent.Url());

            umbracoContextReference.Dispose();
            mainDom.Stop();
            components.Terminate();

            // exit!
        }

        [Test]
        [Explicit("This test must be run manually")]
        public void ValidateComposition()
        {
            // this is almost what CoreRuntime does, without
            // - managing MainDom
            // - configuring for unhandled exceptions, assembly resolution, application root path
            // - testing for database, and for upgrades (runtime level)
            // - assigning the factory to Current.Factory

            // create the very basic and essential things we need
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var profiler = Mock.Of<IProfiler>();
            var profilingLogger = new ProfilingLogger(loggerFactory.CreateLogger("ProfilingLogger"), profiler);
            var appCaches = AppCaches.Disabled;
            var databaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
            var typeFinder = TestHelper.GetTypeFinder();
            var ioHelper = TestHelper.IOHelper;
            var typeLoader = new TypeLoader(typeFinder, appCaches.RuntimeCache, new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")), loggerFactory.CreateLogger<TypeLoader>(), profilingLogger);
            var runtimeState = Mock.Of<IRuntimeState>();
            var hostingEnvironment = Mock.Of<IHostingEnvironment>();
            var backOfficeInfo = TestHelper.GetBackOfficeInfo();
            Mock.Get(runtimeState).Setup(x => x.Level).Returns(RuntimeLevel.Run);
            var mainDom = Mock.Of<IMainDom>();
            Mock.Get(mainDom).Setup(x => x.IsMainDom).Returns(true);

            // create the register and the composition
            var register = TestHelper.GetRegister();
            var composition = new Composition(register, typeLoader, profilingLogger, runtimeState, ioHelper, appCaches);
            var umbracoVersion = TestHelper.GetUmbracoVersion();
            composition.RegisterEssentials(loggerFactory.CreateLogger("RegisterEssentials"), loggerFactory, profiler, profilingLogger, mainDom, appCaches, databaseFactory, typeLoader, runtimeState, typeFinder, ioHelper, umbracoVersion, TestHelper.DbProviderFactoryCreator, hostingEnvironment, backOfficeInfo);

            // create the core runtime and have it compose itself
            var globalSettings = new GlobalSettings();
            var connectionStrings = new ConnectionStrings();

            var coreRuntime = new CoreRuntime(globalSettings, connectionStrings, umbracoVersion, ioHelper, loggerFactory.CreateLogger("CoreRuntime"), loggerFactory, profiler, new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, TestHelper.DbProviderFactoryCreator, TestHelper.MainDom, typeFinder, AppCaches.NoCache);

            // get the components
            // all of them?
            var composerTypes = typeLoader.GetTypes<IComposer>();
            // filtered
            composerTypes = composerTypes
                .Where(x => !x.FullName.StartsWith("Umbraco.Tests"));
            // single?
            //var componentTypes = new[] { typeof(CoreRuntimeComponent) };
            var composers = new Composers(composition, composerTypes, Enumerable.Empty<Attribute>(), loggerFactory.CreateLogger<Composers>(), profilingLogger);

            // get components to compose themselves
            composers.Compose();

            // create the factory
            var factory = composition.CreateFactory();

            // at that point Umbraco is fully composed
            // but nothing is initialized (no maindom, nothing - beware!)
            // to actually *run* Umbraco standalone, better use a StandaloneRuntime
            // that would inherit from CoreRuntime and ensure everything starts

            // get components to initialize themselves
            //components.Initialize(factory);

            // and then, validate
            var lightInjectContainer = (LightInject.ServiceContainer) factory.Concrete;
            var results = lightInjectContainer.Validate().ToList();
            foreach (var resultGroup in results.GroupBy(x => x.Severity).OrderBy(x => x.Key))
            {
                Console.WriteLine($"{resultGroup.Key}: {resultGroup.Count()}");
            }

            foreach (var resultGroup in results.GroupBy(x => x.Severity).OrderBy(x => x.Key))
            foreach (var result in resultGroup)
            {
                Console.WriteLine();
                Console.Write(result.ToText());
            }

            Assert.AreEqual(0, results.Count);
        }




    }
}
