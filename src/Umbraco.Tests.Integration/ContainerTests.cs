using System.Threading.Tasks;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Common;
using Umbraco.Tests.Integration.Implementations;

namespace Umbraco.Tests.Integration
{

    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void CrossWire()
        {
            // MSDI
            var services = new ServiceCollection();
            services.AddSingleton<Foo>();
            var msdiServiceProvider = services.BuildServiceProvider();

            // LightInject / Umbraco
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            var serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();
            serviceProviderFactory.CreateBuilder(services); // called during Host Builder, needed to capture services

            // Dependencies needed for creating composition/register essentials
            var testHelper = new TestHelper();           
            var runtimeState = Mock.Of<IRuntimeState>();
            var umbracoDatabaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
            var dbProviderFactoryCreator = Mock.Of<IDbProviderFactoryCreator>();            
            var typeLoader = testHelper.GetMockedTypeLoader();

            // Register in the container
            var composition = new Composition(umbracoContainer, typeLoader,
                testHelper.Logger, runtimeState, testHelper.GetConfigs(), testHelper.IOHelper, testHelper.AppCaches);
            composition.RegisterEssentials(testHelper.Logger, testHelper.Profiler, testHelper.Logger, testHelper.MainDom,
                testHelper.AppCaches, umbracoDatabaseFactory, typeLoader, runtimeState, testHelper.GetTypeFinder(),
                testHelper.IOHelper, testHelper.GetUmbracoVersion(), dbProviderFactoryCreator,
                testHelper.GetHostingEnvironment(), testHelper.GetBackOfficeInfo());

            // Cross wire - this would be called by the Host Builder at the very end of ConfigureServices
            var lightInjectServiceProvider = serviceProviderFactory.CreateServiceProvider(umbracoContainer.Container);

            // From MSDI
            var foo1 = msdiServiceProvider.GetService<Foo>();
            var foo2 = lightInjectServiceProvider.GetService<Foo>();
            var foo3 = umbracoContainer.GetInstance<Foo>();

            Assert.IsNotNull(foo1);
            Assert.IsNotNull(foo2);
            Assert.IsNotNull(foo3);

            // These are not the same because cross wiring means copying the container, not falling back to a container
            Assert.AreNotSame(foo1, foo2);
            // These are the same because the umbraco container wraps the light inject container
            Assert.AreSame(foo2, foo3);

            Assertions.AssertContainer(umbracoContainer.Container);
        }

        [Explicit("This test just shows that resolving services from the container before the host is done resolves 2 different instances")]
        [Test]
        public async Task BuildServiceProvider_Before_Host_Is_Configured()
        {
            var umbracoContainer = RuntimeTests.GetUmbracoContainer(out var serviceProviderFactory);

            IHostApplicationLifetime lifetime1 = null;

            var hostBuilder = new HostBuilder()
                .UseUmbraco(serviceProviderFactory)
                .ConfigureServices((hostContext, services) =>
                {
                    // Resolve a service from the netcore container before the host has finished the ConfigureServices sequence
                    lifetime1 = services.BuildServiceProvider().GetRequiredService<IHostApplicationLifetime>();

                    // Re-add as a callback, ensures its the same instance all the way through (hack)
                    services.AddSingleton<IHostApplicationLifetime>(x => lifetime1);
                });

            var host = await hostBuilder.StartAsync();

            var lifetime2 = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var lifetime3 = umbracoContainer.GetInstance<IHostApplicationLifetime>();

            lifetime1.StopApplication();
            Assert.IsTrue(lifetime1.ApplicationStopping.IsCancellationRequested);
            Assert.AreEqual(lifetime1.ApplicationStopping.IsCancellationRequested, lifetime2.ApplicationStopping.IsCancellationRequested);
            Assert.AreEqual(lifetime1.ApplicationStopping.IsCancellationRequested, lifetime3.ApplicationStopping.IsCancellationRequested);

        }

        private class Foo
        {
            public Foo()
            {
            }
        }
    }
}
