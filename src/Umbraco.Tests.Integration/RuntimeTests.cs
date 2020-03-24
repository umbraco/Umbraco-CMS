using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Common;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Web.BackOffice.AspNetCore;

namespace Umbraco.Tests.Integration
{
    [TestFixture]
    public class RuntimeTests
    {
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

        [Test]
        public void AddUmbracoCore()
        {
            var testHelper = new TestHelper();

            // MSDI
            var services = new ServiceCollection();
            // These services are required
            services.AddSingleton<IHttpContextAccessor>(x => testHelper.GetHttpContextAccessor());
            services.AddSingleton<IWebHostEnvironment>(x => testHelper.GetWebHostEnvironment());
            services.AddSingleton<IHostApplicationLifetime>(x => Mock.Of<IHostApplicationLifetime>());

            // LightInject / Umbraco
            var container = UmbracoServiceProviderFactory.CreateServiceContainer();
            var serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();

            // Some IConfiguration must exist in the container first
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            services.AddSingleton<IConfiguration>(x => configurationBuilder.Build());

            // Add it!
            services.AddUmbracoConfiguration();
            services.AddUmbracoCore(umbracoContainer, GetType().Assembly);

            // assert results
            var runtimeState = umbracoContainer.GetInstance<IRuntimeState>();
            var mainDom = umbracoContainer.GetInstance<IMainDom>();

            Assert.IsTrue(mainDom.IsMainDom);
            Assert.IsNull(runtimeState.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);
            Assert.IsTrue(MyComposer.IsComposed);
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
