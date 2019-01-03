using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Hosting;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class CoreRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestComponent.Reset();

            // cannot boot runtime without some configuration
            var umbracoSettings = SettingsForTests.GenerateMockUmbracoSettings();
            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();
            SettingsForTests.ConfigureSettings(umbracoSettings);
            SettingsForTests.ConfigureSettings(globalSettings);
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

                var e = app.Runtime.State.BootFailedException;
                var m = "";
                switch (e)
                {
                    case null:
                        m = "";
                        break;
                    case BootFailedException bfe when bfe.InnerException != null:
                        m = "BootFailed: " + bfe.InnerException.GetType() + " " + bfe.InnerException.Message + " " + bfe.InnerException.StackTrace;
                        break;
                    default:
                        m = e.GetType() + " " + e.Message + " " + e.StackTrace;
                        break;
                }

                Assert.AreNotEqual(RuntimeLevel.BootFailed, app.Runtime.State.Level, m);
                Assert.IsTrue(TestComposer.Ctored);
                Assert.IsTrue(TestComposer.Composed);
                Assert.IsTrue(TestComponent.Ctored);
                Assert.IsNotNull(TestComponent.ProfilingLogger);
                Assert.IsInstanceOf<ProfilingLogger>(TestComponent.ProfilingLogger);
                Assert.IsInstanceOf<DebugDiagnosticsLogger>(((ProfilingLogger) TestComponent.ProfilingLogger).Logger);

                // note: components are NOT disposed after boot

                Assert.IsFalse(TestComponent.Terminated);

                app.HandleApplicationEnd();
                Assert.IsTrue(TestComponent.Terminated);
            }
        }

        // test application
        public class TestUmbracoApplication : UmbracoApplicationBase
        {
            public IRuntime Runtime { get; private set; }

            protected override IRuntime GetRuntime()
            {
                return Runtime = new TestRuntime();
            }
        }

        // test runtime
        public class TestRuntime : CoreRuntime
        {
            protected override ILogger GetLogger() => new DebugDiagnosticsLogger();
            protected override IProfiler GetProfiler() => new TestProfiler();

            // must override the database factory
            // else BootFailedException because U cannot connect to the configured db
            protected internal override IUmbracoDatabaseFactory GetDatabaseFactory()
            {
                var mock = new Mock<IUmbracoDatabaseFactory>();
                mock.Setup(x => x.Configured).Returns(true);
                mock.Setup(x => x.CanConnect).Returns(true);
                return mock.Object;
            }

            // fixme so how the f* should we do it now?
            /*
            // pretend we have the proper migration
            // else BootFailedException because our mock IUmbracoDatabaseFactory does not provide databases
            protected override bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory)
            {
                return true;
            }
            */

            // because we don't even have the core runtime component,
            // there are a few required stuff that we need to compose
            public override void Compose(Composition composition)
            {
                base.Compose(composition);

                var scopeProvider = Mock.Of<IScopeProvider>();
                Mock.Get(scopeProvider)
                    .Setup(x => x.CreateScope(
                        It.IsAny<IsolationLevel>(),
                        It.IsAny<RepositoryCacheMode>(),
                        It.IsAny<IEventDispatcher>(),
                        It.IsAny<bool?>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>()))
                    .Returns(Mock.Of<IScope>());

                composition.RegisterUnique(scopeProvider);
            }

            private IMainDom _mainDom;

            public override IFactory Boot(IRegister container)
            {
                var factory = base.Boot(container);
                _mainDom = factory.GetInstance<IMainDom>();
                return factory;
            }

            public override void Terminate()
            {
                ((IRegisteredObject) _mainDom).Stop(false);
                base.Terminate();
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
                composition.Register(factory => SettingsForTests.GetDefaultUmbracoSettings());
                composition.RegisterUnique<IExamineManager, TestExamineManager>();
                composition.Components().Append<TestComponent>();

                Composed = true;
            }
        }

        public class TestComponent : IComponent, IDisposable
        {
            // test flags
            public static bool Ctored;
            public static bool Initialized1;
            public static bool Initialized2;
            public static bool Terminated;
            public static IProfilingLogger ProfilingLogger;

            public bool Disposed;

            public static void Reset()
            {
                Ctored = Initialized1 = Initialized2 = Terminated = false;
                ProfilingLogger = null;
            }

            public TestComponent(IProfilingLogger proflog)
            {
                Ctored = true;
                ProfilingLogger = proflog;
            }

            public void Dispose()
            {
                Disposed = true;
                Terminated = true;
            }
        }
    }
}
