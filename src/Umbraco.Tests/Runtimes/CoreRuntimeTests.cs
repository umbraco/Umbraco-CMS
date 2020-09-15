﻿using System;
using System.Collections.Generic;
using System.Data;
using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Net;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Hosting;
using Umbraco.Web.Runtime;
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

        [Test]
        [Ignore("No IApplicationShutdownRegistry or ISessionIdResolver registered")]
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
            public TestUmbracoApplication() : base(_logger, _configs, _ioHelper, _profiler, new AspNetHostingEnvironment(_hostingSettings), new AspNetBackOfficeInfo(_globalSettings, _ioHelper,  _logger, _settings))
            {
            }

            private static readonly DebugDiagnosticsLogger _logger = new DebugDiagnosticsLogger(new MessageTemplates());
            private static readonly IIOHelper _ioHelper = TestHelper.IOHelper;
            private static readonly IProfiler _profiler = new TestProfiler();
            private static readonly Configs _configs = GetConfigs();
            private static readonly IGlobalSettings _globalSettings = SettingsForTests.DefaultGlobalSettings;
            private static readonly IHostingSettings _hostingSettings = SettingsForTests.DefaultHostingSettings;
            private static readonly IContentSettings _contentSettings = SettingsForTests.GenerateMockContentSettings();
            private static readonly IWebRoutingSettings _settings = _configs.WebRouting();

            private static Configs GetConfigs()
            {
                var configs = new ConfigsFactory().Create();
                configs.Add(() => _globalSettings);
                configs.Add(() => _contentSettings);
                configs.Add(() => _hostingSettings);
                return configs;
            }

            public IRuntime Runtime { get; private set; }

            protected override IRuntime GetRuntime(Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
            {
                return Runtime = new TestRuntime(configs, umbracoVersion, ioHelper, logger, profiler, hostingEnvironment, backOfficeInfo);
            }
        }

        // test runtime
        public class TestRuntime : CoreRuntime
        {
            public TestRuntime(Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILogger logger, IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo)
                :base(configs, umbracoVersion, ioHelper, logger,  profiler, new AspNetUmbracoBootPermissionChecker(), hostingEnvironment, backOfficeInfo, TestHelper.DbProviderFactoryCreator, TestHelper.MainDom, TestHelper.GetTypeFinder(), AppCaches.NoCache)
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

            //public override IFactory Configure(IRegister container)
            //{
            //    container.Register<IApplicationShutdownRegistry, AspNetApplicationShutdownRegistry>(Lifetime.Singleton);
            //    container.Register<ISessionIdResolver, NullSessionIdResolver>(Lifetime.Singleton);

            //    var factory = base.Configure(container);
            //    return factory;
            //}

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
                composition.Register(factory => SettingsForTests.GenerateMockContentSettings());
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
