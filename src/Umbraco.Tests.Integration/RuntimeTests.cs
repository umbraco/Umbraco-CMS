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
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
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
        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            TestLocalDb.Cleanup();
        }

        /// <summary>
        /// Manually configure the containers/dependencies and call Boot on Core runtime
        /// </summary>
        [Test]
        public void BootCoreRuntime()
        {
            // LightInject / Umbraco
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            var serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();            

            // Create the core runtime
            var testHelper = new TestHelper();
            var coreRuntime = new CoreRuntime(testHelper.GetConfigs(), testHelper.GetUmbracoVersion(),
                testHelper.IOHelper, testHelper.Logger, testHelper.Profiler, testHelper.UmbracoBootPermissionChecker,
                testHelper.GetHostingEnvironment(), testHelper.GetBackOfficeInfo(), testHelper.DbProviderFactoryCreator,
                testHelper.MainDom, testHelper.GetTypeFinder());

            // boot it!
            var factory = coreRuntime.Boot(umbracoContainer);

            Assert.IsTrue(coreRuntime.MainDom.IsMainDom);
            Assert.IsNull(coreRuntime.State.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, coreRuntime.State.Level);            
            Assert.IsTrue(MyComposer.IsComposed);
            Assert.IsTrue(MyComponent.IsInit);
            Assert.IsFalse(MyComponent.IsTerminated);

            Assertions.AssertContainer(umbracoContainer.Container, reportOnly: true); // TODO Change that to false eventually when we clean up the container

            coreRuntime.Terminate();

            Assert.IsTrue(MyComponent.IsTerminated);
        }

        /// <summary>
        /// Calling AddUmbracoCore to configure the container and boot the core runtime within a generic host
        /// </summary>
        [Test]
        public void AddUmbracoCore()
        {
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);

            var host = Host.CreateDefaultBuilder()
                .UseTestLifetime()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    var testHelper = new TestHelper();

                    AddRequiredNetCoreServices(services, testHelper);

                    // Add it!
                    services.AddUmbracoConfiguration();
                    services.AddUmbracoCore(umbracoContainer, GetType().Assembly);

                    // Run tests
                    services.AddHostedService(x => DelegateHostedService.Create(() =>
                    {
                        // assert results
                        var runtimeState = umbracoContainer.GetInstance<IRuntimeState>();
                        var mainDom = umbracoContainer.GetInstance<IMainDom>();

                        Assert.IsTrue(mainDom.IsMainDom);
                        Assert.IsNull(runtimeState.BootFailedException);
                        Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);
                        Assert.IsTrue(MyComposer.IsComposed);

                    }, null));
                })
                .Build();

            // NOTE: host.Run() could be used but that requires more work by manually calling IHostApplicationLifetime.StopApplication(). In these cases we don't need to wind down.
            host.Start();
        }


        [Test]
        public void UseUmbracoCore()
        {
            var umbracoContainer = GetUmbracoContainer(out var serviceProviderFactory);
            var testHelper = new TestHelper();

            var host = Host.CreateDefaultBuilder()
                //TODO: Need to have a configured umb version for the runtime state
                .UseLocalDb(Path.Combine(testHelper.CurrentAssemblyDirectory, "LocalDb"))
                .UseTestLifetime()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {   
                    AddRequiredNetCoreServices(services, testHelper);

                    // Add it!
                    services.AddUmbracoConfiguration();
                    services.AddUmbracoCore(umbracoContainer, GetType().Assembly);

                    // Run tests
                    services.AddHostedService(x => DelegateHostedService.Create(() =>
                    {
                        var runtimeState = (RuntimeState)umbracoContainer.GetInstance<IRuntimeState>();
                        Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);

                        var dbBuilder = umbracoContainer.GetInstance<DatabaseBuilder>();
                        Assert.IsNotNull(dbBuilder);

                        var canConnect = dbBuilder.CanConnectToDatabase;
                        Assert.IsTrue(canConnect);

                        var dbResult = dbBuilder.CreateSchemaAndData();
                        Assert.IsTrue(dbResult.Success);

                        var dbFactory = umbracoContainer.GetInstance<IUmbracoDatabaseFactory>();
                        var profilingLogger = umbracoContainer.GetInstance<IProfilingLogger>();
                        runtimeState.DetermineRuntimeLevel(dbFactory, profilingLogger);
                        Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

                    }, null));
                })
                .Build();

            // NOTE: host.Run() could be used but that requires more work by manually calling IHostApplicationLifetime.StopApplication(). In these cases we don't need to wind down.
            host.Start();
        }

        private LightInjectContainer GetUmbracoContainer(out UmbracoServiceProviderFactory serviceProviderFactory)
        {
            var container = new ServiceContainer(ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings());
            serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();
            return umbracoContainer;
        }

        private void AddRequiredNetCoreServices(IServiceCollection services, TestHelper testHelper)
        {
            services.AddSingleton<IHttpContextAccessor>(x => testHelper.GetHttpContextAccessor());
            services.AddSingleton<IWebHostEnvironment>(x => testHelper.GetWebHostEnvironment());
        }

        [RuntimeLevel(MinLevel = RuntimeLevel.Install)]
        public class MyComposer : IUserComposer
        {
            public void Compose(Composition composition)
            {
                composition.Components().Append<MyComponent>();
                IsComposed = true;
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
        }
    }

   
}
