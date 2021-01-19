using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
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
using Umbraco.Tests.Composing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Macros;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Runtime;
using File = System.IO.File;

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
            foreach (var file in Directory.GetFiles(Path.Combine(IOHelper.MapPath("~/App_Data")), "NuCache.*"))
                File.Delete(file);

            // settings
            // reset the current version to 0.0.0, clear connection strings
            ConfigurationManager.AppSettings[Constants.AppSettings.ConfigurationStatus] = "";
            // FIXME: we need a better management of settings here (and, true config files?)

            // create the very basic and essential things we need
            var logger = new ConsoleLogger();
            var profiler = new LogProfiler(logger);
            var profilingLogger = new ProfilingLogger(logger, profiler);
            var appCaches = new AppCaches(); // FIXME: has HttpRuntime stuff?
            var databaseFactory = new UmbracoDatabaseFactory(logger, new Lazy<IMapperCollection>(() => factory.GetInstance<IMapperCollection>()));
            var typeLoader = new TypeLoader(appCaches.RuntimeCache, IOHelper.MapPath("~/App_Data/TEMP"), profilingLogger);
            var mainDom = new SimpleMainDom();
            var runtimeState = new RuntimeState(logger, null, null, new Lazy<IMainDom>(() => mainDom), new Lazy<IServerRegistrar>(() => factory.GetInstance<IServerRegistrar>()));

            // create the register and the composition
            var register = RegisterFactory.Create();
            var composition = new Composition(register, typeLoader, profilingLogger, runtimeState);
            composition.RegisterEssentials(logger, profiler, profilingLogger, mainDom, appCaches, databaseFactory, typeLoader, runtimeState);

            // create the core runtime and have it compose itself
            var coreRuntime = new CoreRuntime();
            coreRuntime.Compose(composition);

            // determine actual runtime level
            runtimeState.DetermineRuntimeLevel(databaseFactory);
            Console.WriteLine(runtimeState.Level);
            // going to be Install BUT we want to force components to be there (nucache etc)
            runtimeState.Level = RuntimeLevel.Run;

            var composerTypes = typeLoader.GetTypes<IComposer>() // all of them
                .Where(x => !x.FullName.StartsWith("Umbraco.Tests.")) // exclude test components
                .Where(x => x != typeof(WebInitialComposer) && x != typeof(WebFinalComposer)); // exclude web runtime
            var composers = new Composers(composition, composerTypes, Enumerable.Empty<Attribute>(), profilingLogger);
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
            composition.Register(_ => Mock.Of<IImageUrlGenerator>(), Lifetime.Singleton);
            composition.RegisterUnique(f => new DistributedCache());
            composition.WithCollectionBuilder<UrlProviderCollectionBuilder>().Append<DefaultUrlProvider>();
            composition.RegisterUnique<IDistributedCacheBinder, DistributedCacheBinder>();
            composition.RegisterUnique<IExamineManager>(f => ExamineManager.Instance);
            composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();
            composition.RegisterUnique<MediaUrlProviderCollection>(_ => new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()));

            // initialize some components only/individually
            composition.WithCollectionBuilder<ComponentCollectionBuilder>()
                .Clear()
                .Append<DistributedCacheBinderComponent>();

            // configure
            composition.Configs.Add(SettingsForTests.GetDefaultGlobalSettings);
            composition.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);

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
                    var creator = new DatabaseSchemaCreator(scope.Database, logger);
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
            var contentType = new ContentType(-1) { Alias = "ctype", Name = "ctype" };
            factory.GetInstance<IContentTypeService>().Save(contentType);
            content = new Content("test", -1, contentType);
            contentService.Save(content);

            // assert that it is possible to get the document back
            content = contentService.GetById(content.Id);
            Assert.IsNotNull(content);
            Assert.AreEqual("test", content.Name);

            // need an UmbracoCOntext to access the cache
            // FIXME: not exactly pretty, should not depend on HttpContext
            var httpContext = Mock.Of<HttpContextBase>();
            var umbracoContextFactory = factory.GetInstance<IUmbracoContextFactory>();
            var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext(httpContext);
            var umbracoContext = umbracoContextReference.UmbracoContext;

            // assert that there is no published document
            var pcontent = umbracoContext.Content.GetById(content.Id);
            Assert.IsNull(pcontent);

            // but a draft document
            pcontent = umbracoContext.Content.GetById(true, content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("test", pcontent.Name());
            Assert.IsTrue(pcontent.IsDraft());

            // no published URL
            Assert.AreEqual("#", pcontent.Url());

            // now publish the document + make some unpublished changes
            contentService.SaveAndPublish(content);
            content.Name = "testx";
            contentService.Save(content);

            // assert that snapshot has been updated and there is now a published document
            pcontent = umbracoContext.Content.GetById(content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("test", pcontent.Name());
            Assert.IsFalse(pcontent.IsDraft());

            // but the URL is the published one - no draft URL
            Assert.AreEqual("/test/", pcontent.Url());

            // and also an updated draft document
            pcontent = umbracoContext.Content.GetById(true, content.Id);
            Assert.IsNotNull(pcontent);
            Assert.AreEqual("testx", pcontent.Name());
            Assert.IsTrue(pcontent.IsDraft());

            // and the published document has a URL
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
            var logger = new ConsoleLogger();
            var profiler = Mock.Of<IProfiler>();
            var profilingLogger = new ProfilingLogger(logger, profiler);
            var appCaches = AppCaches.Disabled;
            var databaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
            var typeLoader = new TypeLoader(appCaches.RuntimeCache, IOHelper.MapPath("~/App_Data/TEMP"), profilingLogger);
            var runtimeState = Mock.Of<IRuntimeState>();
            Mock.Get(runtimeState).Setup(x => x.Level).Returns(RuntimeLevel.Run);
            var mainDom = Mock.Of<IMainDom>();
            Mock.Get(mainDom).Setup(x => x.IsMainDom).Returns(true);

            // create the register and the composition
            var register = RegisterFactory.Create();
            var composition = new Composition(register, typeLoader, profilingLogger, runtimeState);
            composition.RegisterEssentials(logger, profiler, profilingLogger, mainDom, appCaches, databaseFactory, typeLoader, runtimeState);

            // create the core runtime and have it compose itself
            var coreRuntime = new CoreRuntime();
            coreRuntime.Compose(composition);

            // get the components
            // all of them?
            var composerTypes = typeLoader.GetTypes<IComposer>();
            // filtered
            composerTypes = composerTypes
                .Where(x => !x.FullName.StartsWith("Umbraco.Tests"));
            // single?
            //var componentTypes = new[] { typeof(CoreRuntimeComponent) };
            var composers = new Composers(composition, composerTypes, Enumerable.Empty<Attribute>(), profilingLogger);

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
                Console.Write(ToText(result));
            }

            Assert.AreEqual(0, results.Count);
        }

        private static string ToText(ValidationResult result)
        {
            var text = new StringBuilder();

            text.AppendLine($"{result.Severity}: {WordWrap(result.Message, 120)}");
            var target = result.ValidationTarget;
            text.Append("\tsvce: ");
            text.Append(target.ServiceName);
            text.Append(target.DeclaringService.ServiceType);
            if (!target.DeclaringService.ServiceName.IsNullOrWhiteSpace())
            {
                text.Append(" '");
                text.Append(target.DeclaringService.ServiceName);
                text.Append("'");
            }

            text.Append("     (");
            if (target.DeclaringService.Lifetime == null)
                text.Append("Transient");
            else
                text.Append(target.DeclaringService.Lifetime.ToString().TrimStart("LightInject.").TrimEnd("Lifetime"));
            text.AppendLine(")");
            text.Append("\timpl: ");
            text.Append(target.DeclaringService.ImplementingType);
            text.AppendLine();
            text.Append("\tparm: ");
            text.Append(target.Parameter);
            text.AppendLine();

            return text.ToString();
        }

        private static string WordWrap(string text, int width)
        {
            int pos, next;
            var sb = new StringBuilder();
            var nl = Environment.NewLine;

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                var eol = text.IndexOf(nl, pos, StringComparison.Ordinal);

                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + nl.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        var len = eol - pos;

                        if (len > width)
                            len = BreakLine(text, pos, width);

                        if (pos > 0)
                            sb.Append("\t\t");
                        sb.Append(text, pos, len);
                        sb.Append(nl);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && char.IsWhiteSpace(text[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(nl); // Empty line
            }

            return sb.ToString();
        }

        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}
