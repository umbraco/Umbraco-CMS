using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Scoping;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Extensions;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Core.Runtime;
using Umbraco.Core;
using Moq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data.Common;
using System.IO;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Umbraco.Core.Logging.Serilog;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Umbraco.Tests.Integration.Testing
{

    /// <summary>
    /// Abstract class for integration tests
    /// </summary>
    /// <remarks>
    /// This will use a Host Builder to boot and install Umbraco ready for use
    /// </remarks>
    [SingleThreaded]
    [NonParallelizable]
    public abstract class UmbracoIntegrationTest
    {
        public static LightInjectContainer CreateUmbracoContainer(out UmbracoServiceProviderFactory serviceProviderFactory)
        {
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            serviceProviderFactory = new UmbracoServiceProviderFactory(container, false);
            var umbracoContainer = serviceProviderFactory.GetContainer();
            return umbracoContainer;
        }

        private List<Action> _testTeardown = null;
        private List<Action> _fixtureTeardown = new List<Action>();

        public void OnTestTearDown(Action tearDown)
        {
            if (_testTeardown == null)
                _testTeardown = new List<Action>();
            _testTeardown.Add(tearDown);
        }

        public void OnFixtureTearDown(Action tearDown) => _fixtureTeardown.Add(tearDown);

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            foreach (var a in _fixtureTeardown) a();
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var a in _testTeardown) a();
            _testTeardown = null;
            FirstTestInFixture = false;
            FirstTestInSession = false;
        }

        [SetUp]
        public virtual void Setup()
        {
            var hostBuilder = CreateHostBuilder();
            var host = hostBuilder.StartAsync().GetAwaiter().GetResult();
            Services = host.Services;
            var app = new ApplicationBuilder(host.Services);
            Configure(app); //Takes around 200 ms


            OnFixtureTearDown(() => host.Dispose());
        }

        #region Generic Host Builder and Runtime

        private ILoggerFactory CreateLoggerFactory()
        {
            ILoggerFactory factory;
            var testOptions = TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();
            switch (testOptions.Logger)
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
                    throw new NotSupportedException($"Logger option {testOptions.Logger} is not supported.");
            }

            return factory;
        }
        /// <summary>
        /// Create the Generic Host and execute startup ConfigureServices/Configure calls
        /// </summary>
        /// <returns></returns>
        public virtual IHostBuilder CreateHostBuilder()
        {
            UmbracoContainer = CreateUmbracoContainer(out var serviceProviderFactory);
            _serviceProviderFactory = serviceProviderFactory;

            var hostBuilder = Host.CreateDefaultBuilder()
                // IMPORTANT: We Cannot use UseStartup, there's all sorts of threads about this with testing. Although this can work
                // if you want to setup your tests this way, it is a bit annoying to do that as the WebApplicationFactory will
                // create separate Host instances. So instead of UseStartup, we just call ConfigureServices/Configure ourselves,
                // and in the case of the UmbracoTestServerTestBase it will use the ConfigureWebHost to Configure the IApplicationBuilder directly.
                //.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup(GetType()); })
                .UseUmbraco(_serviceProviderFactory)
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    context.HostingEnvironment = TestHelper.GetWebHostEnvironment();
                    Configuration = context.Configuration;
                    configBuilder.AddInMemoryCollection(InMemoryConfiguration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient(_ => CreateLoggerFactory());
                    ConfigureServices(services);
                });
            return hostBuilder;
        }

        /// <summary>
        /// Creates a <see cref="CoreRuntime"/> instance for testing and registers an event handler for database install
        /// </summary>
        /// <param name="connectionStrings"></param>
        /// <param name="umbracoVersion"></param>
        /// <param name="ioHelper"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="profiler"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="backOfficeInfo"></param>
        /// <param name="typeFinder"></param>
        /// <param name="appCaches"></param>
        /// <param name="dbProviderFactoryCreator"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public CoreRuntime CreateTestRuntime(
            GlobalSettings globalSettings,
            ConnectionStrings connectionStrings,
            IUmbracoVersion umbracoVersion, IIOHelper ioHelper,
            ILoggerFactory loggerFactory, IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder, AppCaches appCaches, IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            var runtime = CreateTestRuntime(
                globalSettings,
                connectionStrings,
                umbracoVersion,
                ioHelper,
                loggerFactory,
                profiler,
                hostingEnvironment,
                backOfficeInfo,
                typeFinder,
                appCaches,
                dbProviderFactoryCreator,
                TestHelper.MainDom, // SimpleMainDom
                UseTestLocalDb // DB Installation event handler
            );

            return runtime;
        }

        /// <summary>
        /// Creates a <see cref="CoreRuntime"/> instance for testing and registers an event handler for database install
        /// </summary>
        /// <param name="connectionStrings"></param>
        /// <param name="umbracoVersion"></param>
        /// <param name="ioHelper"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="profiler"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="backOfficeInfo"></param>
        /// <param name="typeFinder"></param>
        /// <param name="appCaches"></param>
        /// <param name="dbProviderFactoryCreator"></param>
        /// <param name="mainDom"></param>
        /// <param name="eventHandler">The event handler used for DB installation</param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static CoreRuntime CreateTestRuntime(
            GlobalSettings globalSettings,
            ConnectionStrings connectionStrings,
            IUmbracoVersion umbracoVersion, IIOHelper ioHelper,
            ILoggerFactory loggerFactory, IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder, AppCaches appCaches, IDbProviderFactoryCreator dbProviderFactoryCreator,
            IMainDom mainDom, Action<CoreRuntime, RuntimeEssentialsEventArgs> eventHandler)
        {
            var runtime = new CoreRuntime(
                globalSettings,
                connectionStrings,
                umbracoVersion,
                ioHelper,
                loggerFactory,
                profiler,
                Mock.Of<IUmbracoBootPermissionChecker>(),
                hostingEnvironment,
                backOfficeInfo,
                dbProviderFactoryCreator,
                mainDom,
                typeFinder,
                appCaches);

            runtime.RuntimeEssentials += (sender, args) => eventHandler(sender, args);

            return runtime;
        }

        #endregion

        #region IStartup

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(TestHelper.DbProviderFactoryCreator);
            var webHostEnvironment = TestHelper.GetWebHostEnvironment();
            services.AddRequiredNetCoreServices(TestHelper, webHostEnvironment);

            // Add it!
            services.AddUmbracoConfiguration(Configuration);
            services.AddUmbracoCore(
                webHostEnvironment,
                UmbracoContainer,
                GetType().Assembly,
                AppCaches.NoCache, // Disable caches for integration tests
                TestHelper.GetLoggingConfiguration(),
                Configuration,
                CreateTestRuntime,
                out _);

            services.AddSignalR();

            services.AddUmbracoWebComponents();
            services.AddUmbracoRuntimeMinifier(Configuration);
            services.AddUmbracoBackOffice();
            services.AddUmbracoBackOfficeIdentity();

            services.AddMvc();

            CustomTestSetup(services);
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            //get the currently set options
            var testOptions = TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();
            if (testOptions.Boot)
            {
                Services.GetRequiredService<IBackofficeSecurityFactory>().EnsureBackofficeSecurity();
                Services.GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
                app.UseUmbracoCore(); // Takes 200 ms
            }
        }

        #endregion

        #region LocalDb

        private static readonly object _dbLocker = new object();
        private static LocalDbTestDatabase _dbInstance;

        /// <summary>
        /// Event handler for the <see cref="CoreRuntime.RuntimeEssentials"/> to install the database and register the <see cref="IRuntime"/> to Terminate
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="args"></param>
        protected void UseTestLocalDb(CoreRuntime runtime, RuntimeEssentialsEventArgs args)
        {
            // MUST be terminated on teardown
            OnTestTearDown(() => runtime.Terminate());

            // This will create a db, install the schema and ensure the app is configured to run
            InstallTestLocalDb(args.DatabaseFactory, runtime.RuntimeLoggerFactory, runtime.State, TestHelper.WorkingDirectory, out var connectionString);
            TestDBConnectionString = connectionString;
            InMemoryConfiguration["ConnectionStrings:" + Constants.System.UmbracoConnectionName] = TestDBConnectionString;
        }

        /// <summary>
        /// Get or create an instance of <see cref="LocalDbTestDatabase"/>
        /// </summary>
        /// <param name="filesPath"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="globalSettings"></param>
        /// <param name="dbFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// There must only be ONE instance shared between all tests in a session
        /// </remarks>
        private static LocalDbTestDatabase GetOrCreateDatabase(string filesPath, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory dbFactory)
        {
            lock (_dbLocker)
            {
                if (_dbInstance != null) return _dbInstance;

                var localDb = new LocalDb();
                if (localDb.IsAvailable == false)
                    throw new InvalidOperationException("LocalDB is not available.");
                _dbInstance = new LocalDbTestDatabase(loggerFactory, localDb, filesPath, dbFactory);
                return _dbInstance;
            }
        }

        /// <summary>
        /// Creates a LocalDb instance to use for the test
        /// </summary>
        /// <param name="runtimeState"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseFactory"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        private void InstallTestLocalDb(
            IUmbracoDatabaseFactory databaseFactory, ILoggerFactory loggerFactory,
            IRuntimeState runtimeState, string workingDirectory, out string connectionString)
        {
            connectionString = null;
            var dbFilePath = Path.Combine(workingDirectory, "LocalDb");

            // get the currently set db options
            var testOptions = TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

            if (testOptions.Database == UmbracoTestOptions.Database.None)
                return;

            // need to manually register this factory
            DbProviderFactories.RegisterFactory(Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            if (!Directory.Exists(dbFilePath))
                Directory.CreateDirectory(dbFilePath);

            var db = GetOrCreateDatabase(dbFilePath, loggerFactory, databaseFactory);

            switch (testOptions.Database)
            {
                case UmbracoTestOptions.Database.NewSchemaPerTest:

                    // New DB + Schema
                    var newSchemaDbId = db.AttachSchema();

                    // Add teardown callback
                    OnTestTearDown(() => db.Detach(newSchemaDbId));

                    // We must re-configure our current factory since attaching a new LocalDb from the pool changes connection strings
                    if (!databaseFactory.Configured)
                    {
                        databaseFactory.Configure(db.ConnectionString, Constants.DatabaseProviders.SqlServer);
                    }

                    // re-run the runtime level check
                    runtimeState.DetermineRuntimeLevel();

                    Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

                    break;
                case UmbracoTestOptions.Database.NewEmptyPerTest:

                    var newEmptyDbId = db.AttachEmpty();

                    // Add teardown callback
                    OnTestTearDown(() => db.Detach(newEmptyDbId));


                    break;
                case UmbracoTestOptions.Database.NewSchemaPerFixture:
                    // Only attach schema once per fixture
                    // Doing it more than once will block the process since the old db hasn't been detached
                    // and it would be the same as NewSchemaPerTest even if it didn't block
                    if (FirstTestInFixture)
                    {
                        // New DB + Schema
                        var newSchemaFixtureDbId = db.AttachSchema();

                        // Add teardown callback
                        OnFixtureTearDown(() => db.Detach(newSchemaFixtureDbId));
                    }

                    // We must re-configure our current factory since attaching a new LocalDb from the pool changes connection strings
                    if (!databaseFactory.Configured)
                    {
                        databaseFactory.Configure(db.ConnectionString, Constants.DatabaseProviders.SqlServer);
                    }

                    // re-run the runtime level check
                    runtimeState.DetermineRuntimeLevel();

                    break;
                case UmbracoTestOptions.Database.NewEmptyPerFixture:

                    throw new NotImplementedException();

                    //// Add teardown callback
                    //integrationTest.OnFixtureTearDown(() => db.Detach());

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testOptions), testOptions, null);
            }
            connectionString = db.ConnectionString;
        }

        #endregion

        #region Common services

        protected LightInjectContainer UmbracoContainer { get; private set; }
        private UmbracoServiceProviderFactory _serviceProviderFactory;

        protected virtual T GetRequiredService<T>() => Services.GetRequiredService<T>();

        public Dictionary<string, string> InMemoryConfiguration { get; } = new Dictionary<string, string>();

        public IConfiguration Configuration { get; protected set; }

        public TestHelper TestHelper = new TestHelper();

        protected virtual string TestDBConnectionString { get; private set; }

        protected virtual Action<IServiceCollection> CustomTestSetup => services => { };

        /// <summary>
        /// Returns the DI container
        /// </summary>
        protected IServiceProvider Services { get; set; }

        /// <summary>
        /// Returns the <see cref="IScopeProvider"/>
        /// </summary>
        protected IScopeProvider ScopeProvider => Services.GetRequiredService<IScopeProvider>();

        /// <summary>
        /// Returns the <see cref="IScopeAccessor"/>
        /// </summary>
        protected IScopeAccessor ScopeAccessor => Services.GetRequiredService<IScopeAccessor>();

        /// <summary>
        /// Returns the <see cref="ILoggerFactory"/>
        /// </summary>
        protected ILoggerFactory LoggerFactory => Services.GetRequiredService<ILoggerFactory>();

        protected AppCaches AppCaches => Services.GetRequiredService<AppCaches>();
        protected IIOHelper IOHelper => Services.GetRequiredService<IIOHelper>();
        protected IShortStringHelper ShortStringHelper => Services.GetRequiredService<IShortStringHelper>();
        protected GlobalSettings GlobalSettings => Services.GetRequiredService<IOptions<GlobalSettings>>().Value;
        protected IMapperCollection Mappers => Services.GetRequiredService<IMapperCollection>();

        #endregion

        #region Builders

        protected UserBuilder UserBuilderInstance = new UserBuilder();
        protected UserGroupBuilder UserGroupBuilderInstance = new UserGroupBuilder();

        #endregion

        protected static bool FirstTestInSession = true;

        protected bool FirstTestInFixture = true;
    }
}
