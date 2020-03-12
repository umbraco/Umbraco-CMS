using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration
{
    [TestFixture]
    public class CrossWireContainer
    {
        [Test]
        public void CrossWire()
        {
            // MSDI
            var services = new ServiceCollection();
            services.AddSingleton<Foo>();
            var msdiServiceProvider = services.BuildServiceProvider();

            // LightInject
            var liContainer = new ServiceContainer(ContainerOptions.Default.WithMicrosoftSettings());
            var liServiceProvider = liContainer.CreateServiceProvider(services);

            // Umbraco
            var umbracoContainer = new LightInjectContainer(liContainer);

            // Dependencies needed for creating composition/register essentials
            var tempPath = Path.Combine(Path.GetTempPath(), "umbraco-temp-" + Guid.NewGuid());
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            var globalSettings = Mock.Of<IGlobalSettings>();
            var typeFinder = Mock.Of<ITypeFinder>();
            var ioHelper = Mock.Of<IIOHelper>();
            var runtimeCache = NoAppCache.Instance;
            var logger = Mock.Of<IProfilingLogger>();
            var typeLoader = new TypeLoader(ioHelper, typeFinder, runtimeCache, new DirectoryInfo(tempPath), logger, false);            
            var runtimeState = Mock.Of<IRuntimeState>();
            var configs = new Configs(x => null);
            var appCaches = new AppCaches(runtimeCache, NoAppCache.Instance, new IsolatedCaches(type => new ObjectCacheAppCache(typeFinder)));            
            var profiler = new VoidProfiler();
            var mainDom = Mock.Of<IMainDom>();
            var umbracoDatabaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
            var umbracoVersion = new UmbracoVersion();
            var dbProviderFactoryCreator = Mock.Of<IDbProviderFactoryCreator>();

            // Register in the container
            var composition = new Composition(umbracoContainer, typeLoader, logger, runtimeState, configs, ioHelper, appCaches);
            composition.RegisterEssentials(logger, profiler, logger, mainDom, appCaches, umbracoDatabaseFactory, typeLoader, runtimeState, typeFinder, ioHelper, umbracoVersion, dbProviderFactoryCreator);

            // Resolve

            // From MSDI
            var foo1 = msdiServiceProvider.GetService<Foo>();
            var foo2 = liServiceProvider.GetService<Foo>();
            var foo3 = umbracoContainer.GetInstance<Foo>();

            Assert.IsNotNull(foo1);
            Assert.IsNotNull(foo2);
            Assert.IsNotNull(foo3);

            // These are not the same because cross wiring means copying the container, not falling back to a container
            Assert.AreNotSame(foo1, foo2);
            // These are the same because the umbraco container wraps the light inject container
            Assert.AreSame(foo2, foo3);
        }

        private class Foo
        {
            public Foo()
            {
            }
        }
    }
}
