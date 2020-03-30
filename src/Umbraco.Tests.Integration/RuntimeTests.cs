using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Umbraco.Configuration.Models;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Common;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web.BackOffice.AspNetCore;
using static Umbraco.Core.Migrations.Install.DatabaseBuilder;

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

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            
        }

        /// <summary>
        /// Manually configure the containers/dependencies and call Boot on Core runtime
        /// </summary>
        [Test]
        public void Boot_Core_Runtime()
        {
            // LightInject / Umbraco
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            var serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();            

            // Special case since we are not using the Generic Host, we need to manually add an AspNetCore service to the container
            umbracoContainer.Register(x => Mock.Of<IHostApplicationLifetime>());
            
            var testHelper = new TestHelper();

            // Create the core runtime
            var coreRuntime = new CoreRuntime(testHelper.GetConfigs(), testHelper.GetUmbracoVersion(),
                testHelper.IOHelper, testHelper.Logger, testHelper.Profiler, testHelper.UmbracoBootPermissionChecker,
                testHelper.GetHostingEnvironment(), testHelper.GetBackOfficeInfo(), testHelper.DbProviderFactoryCreator,
                testHelper.MainDom, testHelper.GetTypeFinder());

                // boot it!
            var factory = coreRuntime.Configure(umbracoContainer);

            Assert.IsTrue(coreRuntime.MainDom.IsMainDom);
            Assert.IsNull(coreRuntime.State.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, coreRuntime.State.Level);            
            Assert.IsTrue(MyComposer.IsComposed);
            Assert.IsFalse(MyComponent.IsInit);
            Assert.IsFalse(MyComponent.IsTerminated);

            coreRuntime.Start();

            Assert.IsTrue(MyComponent.IsInit);
            Assert.IsFalse(MyComponent.IsTerminated);

            Assertions.AssertContainer(umbracoContainer.Container, reportOnly: true); // TODO Change that to false eventually when we clean up the container

            coreRuntime.Terminate();

            Assert.IsTrue(MyComponent.IsTerminated);
        }

        /// <summary>
        /// Calling AddUmbracoCore to configure the container 
        /// </summary>
        [Test]
        public async Task AddUmbracoCore()
        {
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);
            var testHelper = new TestHelper();

            var hostBuilder = new HostBuilder()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    AddRequiredNetCoreServices(services, testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment, umbracoContainer, GetType().Assembly);
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
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);
            var testHelper = new TestHelper();

            var hostBuilder = new HostBuilder()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    AddRequiredNetCoreServices(services, testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment, umbracoContainer, GetType().Assembly);
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

        [Test]
        public async Task Install_Database()
        {
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);
            var testHelper = new TestHelper();

            var hostBuilder = new HostBuilder()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    var webHostEnvironment = testHelper.GetWebHostEnvironment();
                    AddRequiredNetCoreServices(services, testHelper, webHostEnvironment);

                    // Add it!
                    services.AddUmbracoConfiguration(hostContext.Configuration);
                    services.AddUmbracoCore(webHostEnvironment, umbracoContainer, GetType().Assembly);
                });

            var host = await hostBuilder.StartAsync();
            var app = new ApplicationBuilder(host.Services);

            // This will create a db, install the schema and ensure the app is configured to run
            app.UseTestLocalDb(Path.Combine(testHelper.CurrentAssemblyDirectory, "LocalDb"));

            app.UseUmbracoCore();

            var runtimeState = app.ApplicationServices.GetRequiredService<IRuntimeState>();
            Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);
        }

        internal static LightInjectContainer GetUmbracoContainer(out UmbracoServiceProviderFactory serviceProviderFactory)
        {
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();
            return umbracoContainer;
        }

        /// <summary>
        /// These services need to be manually added because they do not get added by the generic host
        /// </summary>
        /// <param name="services"></param>
        /// <param name="testHelper"></param>
        /// <param name="webHostEnvironment"></param>
        private void AddRequiredNetCoreServices(IServiceCollection services, TestHelper testHelper, IWebHostEnvironment webHostEnvironment)
        {
            services.AddSingleton<IHttpContextAccessor>(x => testHelper.GetHttpContextAccessor());
            // the generic host does add IHostEnvironment but not this one because we are not actually in a web context
            services.AddSingleton<IWebHostEnvironment>(x => webHostEnvironment);
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

            private readonly ILogger _logger;

            public MyComponent(ILogger logger)
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
