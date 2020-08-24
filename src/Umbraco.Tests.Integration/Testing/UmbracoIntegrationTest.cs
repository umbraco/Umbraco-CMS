using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using ILogger = Umbraco.Core.Logging.ILogger;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;

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
        public static LightInjectContainer GetUmbracoContainer(out UmbracoServiceProviderFactory serviceProviderFactory)
        {
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            serviceProviderFactory = new UmbracoServiceProviderFactory(container, false);
            var umbracoContainer = serviceProviderFactory.GetContainer();
            return umbracoContainer;
        }

        /// <summary>
        /// Get or create an instance of <see cref="LocalDbTestDatabase"/>
        /// </summary>
        /// <param name="filesPath"></param>
        /// <param name="logger"></param>
        /// <param name="globalSettings"></param>
        /// <param name="dbFactory"></param>
        /// <returns></returns>
        /// <remarks>
        /// There must only be ONE instance shared between all tests in a session
        /// </remarks>
        public static LocalDbTestDatabase GetOrCreate(string filesPath, ILogger logger, GlobalSettings globalSettings, IUmbracoDatabaseFactory dbFactory)
        {
            lock (_dbLocker)
            {
                if (_dbInstance != null) return _dbInstance;

                var localDb = new LocalDb();
                if (localDb.IsAvailable == false)
                    throw new InvalidOperationException("LocalDB is not available.");
                _dbInstance = new LocalDbTestDatabase(logger, globalSettings, localDb, filesPath, dbFactory);
                return _dbInstance;
            }
        }

        private static readonly object _dbLocker = new object();
        private static LocalDbTestDatabase _dbInstance;

        private Action _testTeardown = null;
        private Action _fixtureTeardown = null;

        public void OnTestTearDown(Action tearDown)
        {
            _testTeardown = tearDown;
        }

        public void OnFixtureTearDown(Action tearDown)
        {
            _fixtureTeardown = tearDown;
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            _fixtureTeardown?.Invoke();
        }

        [TearDown]
        public void TearDown()
        {
            _testTeardown?.Invoke();
        }

        [SetUp]
        public async Task Setup()
        {
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);
            var testHelper = new TestHelper();
            // get the currently set db options
            var testOptions = TestOptionAttributeBase.GetTestOptions<UmbracoTestAttribute>();

            var hostBuilder = new HostBuilder()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(testHelper.DbProviderFactoryCreator);
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    services.AddRequiredNetCoreServices(testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment, umbracoContainer, GetType().Assembly, NoAppCache.Instance, testHelper.GetLoggingConfiguration(), out _);
                    services.AddUmbracoWebComponents();
                    services.AddUmbracoRuntimeMinifier(hostContext.Configuration);
                    services.AddUmbracoBackOffice();
                    services.AddUmbracoBackOfficeIdentity();

                    services.AddMvc();


                    services.AddSingleton<ILogger>(new ConsoleLogger(new MessageTemplates()));

                    CustomTestSetup(services);
                });

            var host = await hostBuilder.StartAsync();
            var app = new ApplicationBuilder(host.Services);
            Services = app.ApplicationServices;

            Services.GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
            // This will create a db, install the schema and ensure the app is configured to run
            app.UseTestLocalDb(testHelper.WorkingDirectory, this, out var connectionString);
            TestDBConnectionString = connectionString;

            if (testOptions.Boot)
            {
                app.UseUmbracoCore();
            }

        }

        protected T GetRequiredService<T>() => Services.GetRequiredService<T>();

        #region Common services

        protected string TestDBConnectionString { get; private set; }

        protected virtual Action<IServiceCollection> CustomTestSetup => services => { };

        /// <summary>
        /// Returns the DI container
        /// </summary>
        protected IServiceProvider Services { get; private set; }

        /// <summary>
        /// Returns the <see cref="IScopeProvider"/>
        /// </summary>
        protected IScopeProvider ScopeProvider => Services.GetRequiredService<IScopeProvider>();

        /// <summary>
        /// Returns the <see cref="IScopeAccessor"/>
        /// </summary>
        protected IScopeAccessor ScopeAccessor => Services.GetRequiredService<IScopeAccessor>();

        /// <summary>
        /// Returns the <see cref="ILogger"/>
        /// </summary>
        protected ILogger Logger => Services.GetRequiredService<ILogger>();

        protected AppCaches AppCaches => Services.GetRequiredService<AppCaches>();
        protected IIOHelper IOHelper => Services.GetRequiredService<IIOHelper>();
        protected IShortStringHelper ShortStringHelper => Services.GetRequiredService<IShortStringHelper>();
        protected GlobalSettings GlobalSettings => Services.GetRequiredService<IOptions<GlobalSettings>>().Value;
        protected IMapperCollection Mappers => Services.GetRequiredService<IMapperCollection>();

        #endregion

        #region Builders

        protected GlobalSettingsBuilder GlobalSettingsBuilder = new GlobalSettingsBuilder();
        protected UserBuilder UserBuilder = new UserBuilder();
        protected UserGroupBuilder UserGroupBuilder = new UserGroupBuilder();

        #endregion
    }
}
