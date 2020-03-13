using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Composing;
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
            var container = new ServiceContainer(ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings());
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

            Assertions.AssertContainer(umbracoContainer.Container, reportOnly: true); // TODO Change that to false eventually when we clean up the container
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
            var container = new ServiceContainer(ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings());
            var serviceProviderFactory = new UmbracoServiceProviderFactory(container);
            var umbracoContainer = serviceProviderFactory.GetContainer();

            // Add it!
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
                IsComposed = true;
            }

            public static bool IsComposed { get; private set; }
        }
    }

   
}
