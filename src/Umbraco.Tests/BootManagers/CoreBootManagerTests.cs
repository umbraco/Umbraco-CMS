using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Examine;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Persistence;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using UmbracoExamine;

namespace Umbraco.Tests.BootManagers
{
    [TestFixture]
    public class CoreBootManagerTests : BaseUmbracoConfigurationTest
    {
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            ResolverCollection.ResetAll();
            TestApplicationEventHandler.Reset();
        }

     
        /// <summary>
        /// test application using a CoreBootManager instance to boot
        /// </summary>
        public class TestApp : UmbracoApplicationBase
        {
            protected override IBootManager GetBootManager()
            {
                return new TestBootManager(this, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            }

            private ILogger _logger;

            /// <summary>
            /// Returns the logger instance for the application - this will be used throughout the entire app
            /// </summary>
            public override ILogger Logger
            {
                get { return _logger ?? (_logger = Mock.Of<ILogger>()); }
            }
        }

        /// <summary>
        /// Test boot manager to add a custom application event handler
        /// </summary>
        public class TestBootManager : CoreBootManager
        {
            public TestBootManager(UmbracoApplicationBase umbracoApplication, ProfilingLogger logger)
                : base(umbracoApplication, logger)
            {
            }

            internal override void ConfigureCoreServices(ServiceContainer container)
            {
                base.ConfigureCoreServices(container);
                container.Register<IUmbracoSettingsSection>(factory => SettingsForTests.GetDefault());
                container.Register<DatabaseContext>(factory => new DatabaseContext(
                    factory.GetInstance<IDatabaseFactory>(),
                    factory.GetInstance<ILogger>()), new PerContainerLifetime());
                container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();
            }
        }
        
        /// <summary>
        /// test event handler
        /// </summary>
        public class TestApplicationEventHandler : DisposableObject, IApplicationEventHandler
        {
            public static void Reset()
            {
                Initialized = false;
                Starting = false;
                Started = false;
                Disposed = false;
            }

            public static bool Initialized = false;
            public static bool Starting = false;
            public static bool Started = false;
            public static bool Disposed = false;

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
                Disposed = true;
            }
        }

        [Test]
        public void Disposes_App_Startup_Handlers_After_Startup()
        {
            using (var app = new TestApp())
            {
                app.StartApplication(app, new EventArgs());
                
                Assert.IsTrue(TestApplicationEventHandler.Disposed);
            }
        }

        [Test]
        public void Handle_IApplicationEventHandler_Objects_Outside_Web_Context()
        {
            using (var app = new TestApp())
            {
                app.StartApplication(app, new EventArgs());

                Assert.IsTrue(TestApplicationEventHandler.Initialized);
                Assert.IsTrue(TestApplicationEventHandler.Starting);
                Assert.IsTrue(TestApplicationEventHandler.Started);
            }
        }

        [Test]
        public void Raises_Starting_Events()
        {
            using (var app = new TestApp())
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

                app.StartApplication(app, new EventArgs());

                app.ApplicationStarting -= starting;
                app.ApplicationStarting -= started;
            }
        }

    }
}
