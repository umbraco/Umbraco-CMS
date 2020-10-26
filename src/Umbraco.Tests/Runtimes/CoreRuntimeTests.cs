using System;
using System.Collections.Generic;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Infrastructure.Composing;
using Umbraco.Net;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Hosting;
using Umbraco.Web.Runtime;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class CoreRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestComponent.Reset();
            Current.Reset();
        }

        public void TearDown()
        {
            TestComponent.Reset();
        }


        // test application
        public class TestUmbracoApplication : UmbracoApplicationBase
        {
            public TestUmbracoApplication() : base(new NullLogger<UmbracoApplicationBase>(),
                NullLoggerFactory.Instance,
                new SecuritySettings(),
                new GlobalSettings(),
                new ConnectionStrings(),
                _ioHelper, _profiler, new AspNetHostingEnvironment(Options.Create(new HostingSettings())), new AspNetBackOfficeInfo(_globalSettings, _ioHelper,  new NullLogger<AspNetBackOfficeInfo>(), Options.Create(new WebRoutingSettings())))
            {
            }

            private static readonly IIOHelper _ioHelper = TestHelper.IOHelper;
            private static readonly IProfiler _profiler = new TestProfiler();
            private static readonly GlobalSettings _globalSettings = new GlobalSettings();

            public IRuntime Runtime { get; private set; }

            protected override IRuntime GetRuntime(GlobalSettings globalSettings, ConnectionStrings connectionStrings, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger,  ILoggerFactory loggerFactory, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
            {
                return Runtime = new TestRuntime(globalSettings, connectionStrings, umbracoVersion, ioHelper, logger, loggerFactory, profiler, hostingEnvironment, backOfficeInfo);
            }
        }

        // test runtime
        public class TestRuntime : CoreRuntime
        {
            public TestRuntime(GlobalSettings globalSettings, ConnectionStrings connectionStrings, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger, ILoggerFactory loggerFactory, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
                :base(globalSettings, connectionStrings,umbracoVersion, ioHelper, loggerFactory, profiler, new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, TestHelper.DbProviderFactoryCreator, TestHelper.MainDom, TestHelper.GetTypeFinder(), AppCaches.NoCache)
            {

            }

            // must override the database factory
            // else BootFailedException because U cannot connect to the configured db
            protected internal override IUmbracoDatabaseFactory CreateDatabaseFactory()
            {
                var mock = new Mock<IUmbracoDatabaseFactory>();
                mock.Setup(x => x.Configured).Returns(true);
                mock.Setup(x => x.CanConnect).Returns(true);
                return mock.Object;
            }

            public override IFactory Configure(IServiceCollection services)
            {
                var container = ServiceCollectionRegistryAdapter.Wrap(services);
                container.Register<IApplicationShutdownRegistry, AspNetApplicationShutdownRegistry>(Lifetime.Singleton);
                container.Register<ISessionIdResolver, NullSessionIdResolver>(Lifetime.Singleton);
                container.Register(typeof(ILogger<>), typeof(Logger<>), Lifetime.Singleton);


                var factory = base.Configure(services);
                return factory;
            }

            // runs with only one single component
            // UmbracoCoreComponent will be force-added too
            // and that's it
            protected override IEnumerable<Type> GetComposerTypes(TypeLoader typeLoader)
            {
                return new[] { typeof(TestComposer) };
            }
        }


        public class TestComposer : IComposer
        {
            // test flags
            public static bool Ctored;
            public static bool Composed;

            public static void Reset()
            {
                Ctored = Composed = false;
            }

            public TestComposer()
            {
                Ctored = true;
            }

            public void Compose(Composition composition)
            {
                composition.RegisterUnique<IExamineManager, TestExamineManager>();
                composition.Components().Append<TestComponent>();

                Composed = true;
            }
        }

        public class TestComponent : IComponent, IDisposable
        {
            // test flags
            public static bool Ctored;
            public static bool Initialized;
            public static bool Terminated;
            public static IProfilingLogger ProfilingLogger;

            public bool Disposed;

            public static void Reset()
            {
                Ctored = Initialized = Terminated = false;
                ProfilingLogger = null;
            }

            public TestComponent(IProfilingLogger proflog)
            {
                Ctored = true;
                ProfilingLogger = proflog;
            }

            public void Initialize()
            {
                Initialized = true;
            }

            public void Terminate()
            {
                Terminated = true;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
