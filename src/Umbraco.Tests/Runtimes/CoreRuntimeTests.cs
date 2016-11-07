using System;
using System.Collections.Generic;
using System.Web.Hosting;
using LightInject;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.DI;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using UmbracoExamine;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
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
                return new TestRuntime(this);
            }

            // the application's logger is created by the application
            // through GetLogger, that custom application can override
            protected override ILogger GetLogger()
            {
                //return Mock.Of<ILogger>();
                return new DebugDiagnosticsLogger();
            }

            // don't register anything against AppDomain
            protected override void ConfigureUnhandledException(ILogger logger)
            { }
        }

        // test runtime
        public class TestRuntime : CoreRuntime
        {
            public TestRuntime(UmbracoApplicationBase umbracoApplication)
                : base(umbracoApplication)
            { }

            public override void Compose(ServiceContainer container)
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
            private static IDatabaseFactory GetDatabaseFactory()
            {
                var mock = new Mock<IDatabaseFactory>();
                mock.Setup(x => x.Configured).Returns(true);
                mock.Setup(x => x.CanConnect).Returns(true);
                return mock.Object;
            }

            // pretend we have the proper migration
            // else BootFailedException because our mock IDatabaseFactory does not provide databases
            protected override bool EnsureMigration(IDatabaseFactory databaseFactory, SemVersion codeVersion)
            {
                return true;
            }

            private MainDom _mainDom;

            public override void Boot(ServiceContainer container)
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

                composition.Container.Register(factory => SettingsForTests.GetDefault());
                composition.Container.Register(factory => new DatabaseContext(
                    factory.GetInstance<IDatabaseFactory>(),
                    factory.GetInstance<ILogger>(), factory.GetInstance<IRuntimeState>(), Mock.Of<IMigrationEntryService>()), new PerContainerLifetime());
                composition.Container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();

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
