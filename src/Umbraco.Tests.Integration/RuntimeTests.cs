using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Runtime;
using Umbraco.Extensions;
using Umbraco.Tests.Common;
using Umbraco.Tests.Integration.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration
{

    [TestFixture]
    public class RuntimeTests
    {
        [TearDown]
        public void TearDown()
        {
            MyComponent.Reset();
            MyComposer.Reset();
        }

        [SetUp]
        public void Setup()
        {
            MyComponent.Reset();
            MyComposer.Reset();
        }

        /// <summary>
        /// Manually configure the containers/dependencies and call Boot on Core runtime
        /// </summary>
        [Test]
        public void Boot_Core_Runtime()
        {
            var services = new ServiceCollection().AddLazySupport();

            // Special case since we are not using the Generic Host, we need to manually add an AspNetCore service to the container
            services.AddTransient(x => Mock.Of<IHostApplicationLifetime>());

            var testHelper = new TestHelper();

            var globalSettings = new GlobalSettings();
            var connectionStrings = new ConnectionStrings();

            // TODO: found these registration were necessary here (as we haven't called the HostBuilder?), as dependencies for ComponentCollection
            // are not resolved.  Need to check this if these explicit registrations are the best way to handle this.
            var contentSettings = new ContentSettings();
            var coreDebugSettings = new CoreDebugSettings();
            var nuCacheSettings = new NuCacheSettings();
            var requestHandlerSettings = new RequestHandlerSettings();
            var userPasswordConfigurationSettings = new UserPasswordConfigurationSettings();
            var webRoutingSettings = new WebRoutingSettings();

            services.AddTransient(x => Options.Create(globalSettings));
            services.AddTransient(x => Options.Create(connectionStrings));
            services.AddTransient(x => Options.Create(contentSettings));
            services.AddTransient(x => Options.Create(coreDebugSettings));
            services.AddTransient(x => Options.Create(nuCacheSettings));
            services.AddTransient(x => Options.Create(requestHandlerSettings));
            services.AddTransient(x => Options.Create(userPasswordConfigurationSettings));
            services.AddTransient(x => Options.Create(webRoutingSettings));
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Create the core runtime
            var bootstrapper = new CoreRuntimeBootstrapper(globalSettings, connectionStrings, testHelper.GetUmbracoVersion(),
                testHelper.IOHelper, testHelper.ConsoleLoggerFactory, testHelper.Profiler, testHelper.UmbracoBootPermissionChecker,
                testHelper.GetHostingEnvironment(), testHelper.GetBackOfficeInfo(), testHelper.DbProviderFactoryCreator,
                testHelper.MainDom, testHelper.GetTypeFinder(), AppCaches.NoCache);

            bootstrapper.Configure(services);

            Assert.IsTrue(bootstrapper.MainDom.IsMainDom);
            Assert.IsNull(bootstrapper.State.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, bootstrapper.State.Level);
            Assert.IsTrue(MyComposer.IsComposed);
            Assert.IsFalse(MyComponent.IsInit);
            Assert.IsFalse(MyComponent.IsTerminated);

            var container = services.BuildServiceProvider();

            var runtime = container.GetRequiredService<IRuntime>();

            runtime.Start();

            Assert.IsTrue(MyComponent.IsInit);
            Assert.IsFalse(MyComponent.IsTerminated);

            runtime.Terminate();

            Assert.IsTrue(MyComponent.IsTerminated);
        }

        /// <summary>
        /// Calling AddUmbracoCore to configure the container
        /// </summary>
        [Test]
        public async Task AddUmbracoCore()
        {
            var testHelper = new TestHelper();

            var hostBuilder = new HostBuilder()
                .UseUmbraco()
                .ConfigureServices((hostContext, services) =>
                {
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    services.AddSingleton(testHelper.DbProviderFactoryCreator);
                    services.AddRequiredNetCoreServices(testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment, GetType().Assembly, AppCaches.NoCache, testHelper.GetLoggingConfiguration(), hostContext.Configuration);
                });

            var host = await hostBuilder.StartAsync();
            var app = new ApplicationBuilder(host.Services);

            // assert results
            var runtimeState = app.ApplicationServices.GetRequiredService<IRuntimeState>();
            var mainDom = app.ApplicationServices.GetRequiredService<IMainDom>();

            Assert.IsFalse(mainDom.IsMainDom); // We haven't "Started" the runtime yet
            Assert.IsNull(runtimeState.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);
            Assert.IsFalse(MyComponent.IsInit); // We haven't "Started" the runtime yet

            await host.StopAsync();

            Assert.IsFalse(MyComponent.IsTerminated); // we didn't "Start" the runtime so nothing was registered for shutdown
        }

        /// <summary>
        /// Calling AddUmbracoCore to configure the container and UseUmbracoCore to start the runtime
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task UseUmbracoCore()
        {
            var testHelper = new TestHelper();

            var hostBuilder = new HostBuilder()
                .UseUmbraco()
                .ConfigureServices((hostContext, services) =>
                {
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    services.AddSingleton(testHelper.DbProviderFactoryCreator);
                    services.AddRequiredNetCoreServices(testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment,  GetType().Assembly, AppCaches.NoCache, testHelper.GetLoggingConfiguration(),hostContext.Configuration);
                });

            var host = await hostBuilder.StartAsync();
            var app = new ApplicationBuilder(host.Services);

            app.UseUmbracoCore();


            // assert results
            var runtimeState = app.ApplicationServices.GetRequiredService<IRuntimeState>();
            var mainDom = app.ApplicationServices.GetRequiredService<IMainDom>();

            Assert.IsTrue(mainDom.IsMainDom);
            Assert.IsNull(runtimeState.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);
            Assert.IsTrue(MyComponent.IsInit);

            await host.StopAsync();

            Assert.IsTrue(MyComponent.IsTerminated);
        }


        [RuntimeLevel(MinLevel = RuntimeLevel.Install)]
        public class MyComposer : IUserComposer
        {
            public void Compose(Composition composition)
            {
                composition.Components().Append<MyComponent>();
                IsComposed = true;
            }

            public static void Reset()
            {
                IsComposed = false;
            }

            public static bool IsComposed { get; private set; }
        }

        public class MyComponent : IComponent
        {
            public static bool IsInit { get; private set; }
            public static bool IsTerminated { get; private set; }

            private readonly ILogger<MyComponent> _logger;

            public MyComponent(ILogger<MyComponent> logger)
            {
                _logger = logger;
            }

            public void Initialize()
            {
                IsInit = true;
            }

            public void Terminate()
            {
                IsTerminated = true;
            }

            public static void Reset()
            {
                IsTerminated = false;
                IsInit = false;
            }
        }
    }


}
