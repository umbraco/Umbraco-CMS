using System;
using System.Collections.Generic;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class CoreRuntimeTests : BaseUmbracoConfigurationTest
    {
        [TearDown] // fixme TearDown is INHERITED
        public override void TearDown()
        {
            base.TearDown();

            TestApplicationEventHandler.Reset();

            Current.Reset();
        }

        // test application
        public class TestUmbracoApplication : UmbracoApplicationBase
        {
            private readonly ILogger _logger = Mock.Of<ILogger>();

            protected override IRuntime GetRuntime()
            {
                return new TestRuntime(this, new ProfilingLogger(_logger, Mock.Of<IProfiler>()));
            }

            protected override ILogger GetLogger()
            {
                return _logger;
            }
        }

        // test runtime - inheriting from core runtime
        public class TestRuntime : CoreRuntime
        {
            private readonly ProfilingLogger _proflog;

            public TestRuntime(UmbracoApplicationBase umbracoApplication, ProfilingLogger proflog)
                : base(umbracoApplication)
            {
                _proflog = proflog;
            }

            public override void Boot(ServiceContainer container)
            {
                // do it before anything else - this is the only place where it's possible
                container.RegisterInstance<ILogger>(_proflog.Logger);
                container.RegisterInstance<IProfiler>(_proflog.Profiler);
                container.RegisterInstance<ProfilingLogger>(_proflog);

                base.Boot(container);
            }

            protected override void Compose1(ServiceContainer container)
            {
                base.Compose1(container);

                container.Register<IUmbracoSettingsSection>(factory => SettingsForTests.GetDefault());
                container.Register<DatabaseContext>(factory => new DatabaseContext(
                    factory.GetInstance<IDatabaseFactory>(),
                    factory.GetInstance<ILogger>()), new PerContainerLifetime());
                container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();
            }

            protected override IEnumerable<Type> GetComponentTypes()
            {
                return new[] { typeof(TestComponent) };
            }
        }

        public class TestComponent : UmbracoComponentBase
        {
            // test
            public static bool Ctored;
            public static bool Composed;
            public static bool Initialized1;
            public static bool Initialized2;
            public static bool Terminated;

            public TestComponent()
                : base()
            {
                Ctored = true;
            }

            public override void Compose(ServiceContainer container)
            {
                base.Compose(container);
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

            public override void Terminate()
            {
                base.Terminate();
                Terminated = true;
            }
        }

        // test all event handler
        // fixme should become test component
        public class TestApplicationEventHandler : DisposableObject, IApplicationEventHandler
        {
            public static void Reset()
            {
                Initialized = false;
                Starting = false;
                Started = false;
                HasBeenDisposed = false;
            }

            public static bool Initialized;
            public static bool Starting;
            public static bool Started;
            public static bool HasBeenDisposed;

            public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
            {
                Initialized = true;
            }

            public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
            {
                Starting = true;
            }

            public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
            {
                Started = true;
            }

            protected override void DisposeResources()
            {
                HasBeenDisposed = true;
            }
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

                Assert.IsFalse(TestComponent.Terminated);

                app.HandleApplicationEnd();
                Assert.IsTrue(TestComponent.Terminated);
            }
        }

        // note: components are NOT disposed after boot
        [Test]
        public void Disposes_App_Startup_Handlers_After_Startup()
        {
            using (var app = new TestUmbracoApplication())
            {
                app.HandleApplicationStart(app, new EventArgs());

                Assert.IsTrue(TestApplicationEventHandler.HasBeenDisposed);
            }
        }

        [Test]
        public void Handle_IApplicationEventHandler_Objects_Outside_Web_Context()
        {
            using (var app = new TestUmbracoApplication())
            {
                app.HandleApplicationStart(app, new EventArgs());

                Assert.IsTrue(TestApplicationEventHandler.Initialized);
                Assert.IsTrue(TestApplicationEventHandler.Starting);
                Assert.IsTrue(TestApplicationEventHandler.Started);
            }
        }

        [Test]
        public void Raises_Starting_Events()
        {
            using (var app = new TestUmbracoApplication())
            {
                EventHandler starting = (sender, args) =>
                {
                    Assert.IsTrue(TestApplicationEventHandler.Initialized);
                    Assert.IsTrue(TestApplicationEventHandler.Starting);
                    Assert.IsFalse(TestApplicationEventHandler.Started);
                };
                EventHandler started = (sender, args) =>
                {
                    Assert.IsTrue(TestApplicationEventHandler.Initialized);
                    Assert.IsTrue(TestApplicationEventHandler.Starting);
                    Assert.IsTrue(TestApplicationEventHandler.Started);
                };

                app.ApplicationStarting += starting;
                app.ApplicationStarted += started;

                app.HandleApplicationStart(app, new EventArgs());

                app.ApplicationStarting -= starting;
                app.ApplicationStarting -= started;
            }
        }
    }
}
