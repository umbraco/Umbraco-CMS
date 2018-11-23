using System;
using System.Collections.Generic;
using System.Web.Hosting;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    [Ignore("cannot work until we refactor IUmbracoDatabaseFactory vs UmbracoDatabaseFactory")]
    public class CoreRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestComponent.Reset();
        }

        public void TearDown()
        {
            TestComponent.Reset();
        }

        [Test]
        public void ComponentLifeCycle()
        {
            using (var app = new TestUmbracoApplication())
            {
                app.HandleApplicationStart(app, new EventArgs());

                Assert.IsTrue(TestComponent.Ctored);
                Assert.IsTrue(TestComponent.Composed);
                Assert.IsTrue(TestComponent.Initialized1);
                Assert.IsTrue(TestComponent.Initialized2);
                Assert.IsNotNull(TestComponent.ProfilingLogger);
                Assert.IsInstanceOf<DebugDiagnosticsLogger>(TestComponent.ProfilingLogger.Logger);

                // note: components are NOT disposed after boot

                Assert.IsFalse(TestComponent.Terminated);

                app.HandleApplicationEnd();
                Assert.IsTrue(TestComponent.Terminated);
            }
        }

        // test application
        public class TestUmbracoApplication : UmbracoApplicationBase
        {
            protected override IRuntime GetRuntime()
            {
                return new TestRuntime();
            }
        }

        // test runtime
        public class TestRuntime : CoreRuntime
        {
            // the application's logger is created by the application
            // through GetLogger, that custom application can override
            protected override ILogger GetLogger()
            {
                //return Mock.Of<ILogger>();
                return new DebugDiagnosticsLogger();
            }

            public override void Compose(IContainer container)
            {
                base.Compose(container);

                // the application's profiler and profiling logger are
                // registered by CoreRuntime.Compose() but can be
                // overriden afterwards - they haven't been resolved yet
                container.RegisterSingleton<IProfiler>(_ => new TestProfiler());
                container.RegisterSingleton(factory => new ProfilingLogger(factory.GetInstance<ILogger>(), factory.GetInstance<IProfiler>()));

                // must override the database factory
                container.RegisterSingleton(_ => GetDatabaseFactory());
            }

            // must override the database factory
            // else BootFailedException because U cannot connect to the configured db
            private static IUmbracoDatabaseFactory GetDatabaseFactory()
            {
                var mock = new Mock<IUmbracoDatabaseFactory>();
                mock.Setup(x => x.Configured).Returns(true);
                mock.Setup(x => x.CanConnect).Returns(true);
                return mock.Object;
            }

            // pretend we have the proper migration
            // else BootFailedException because our mock IUmbracoDatabaseFactory does not provide databases
            protected override bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
            {
                return true;
            }

            private MainDom _mainDom;

            public override void Boot(IContainer container)
            {
                base.Boot(container);
                _mainDom = container.GetInstance<MainDom>();
            }

            public override void Terminate()
            {
                ((IRegisteredObject) _mainDom).Stop(false);
                base.Terminate();
            }

            // runs with only one single component
            // UmbracoCoreComponent will be force-added too
            // and that's it
            protected override IEnumerable<Type> GetComponentTypes()
            {
                return new[] { typeof(TestComponent) };
            }
        }


        public class TestComponent : UmbracoComponentBase
        {
            // test flags
            public static bool Ctored;
            public static bool Composed;
            public static bool Initialized1;
            public static bool Initialized2;
            public static bool Terminated;
            public static ProfilingLogger ProfilingLogger;

            public static void Reset()
            {
                Ctored = Composed = Initialized1 = Initialized2 = Terminated = false;
                ProfilingLogger = null;
            }

            public TestComponent()
            {
                Ctored = true;
            }

            public override void Compose(Composition composition)
            {
                base.Compose(composition);

                composition.Container.Register(factory => SettingsForTests.GetDefaultUmbracoSettings());
                composition.Container.RegisterSingleton<IExamineManager, TestExamineManager>();

                Composed = true;
            }

            public void Initialize()
            {
                Initialized1 = true;
            }

            public void Initialize(ILogger logger)
            {
                Initialized2 = true;
            }

            public void Initialize(ProfilingLogger proflog)
            {
                ProfilingLogger = proflog;
            }

            public override void Terminate()
            {
                base.Terminate();
                Terminated = true;
            }
        }
    }
}
