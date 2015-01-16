using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using umbraco.interfaces;

namespace Umbraco.Tests.BootManagers
{
    [TestFixture]
    public class CoreBootManagerTests : BaseUmbracoApplicationTest
    {

        private TestApp _testApp;

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
            _testApp = new TestApp();            
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _testApp = null;            
        }

        protected override void FreezeResolution()
        {
            //don't freeze resolution, we'll do that in the boot manager
        }

        /// <summary>
        /// test application using a CoreBootManager instance to boot
        /// </summary>
        public class TestApp : UmbracoApplicationBase
        {
            protected override IBootManager GetBootManager()
            {
                return new TestBootManager(this);
            }
        }

        /// <summary>
        /// Test boot manager to add a custom application event handler
        /// </summary>
        public class TestBootManager : CoreBootManager
        {
            public TestBootManager(UmbracoApplicationBase umbracoApplication)
                : base(umbracoApplication)
            {
            }

            protected override void InitializeApplicationEventsResolver()
            {
                //create an empty resolver so we can add our own custom ones (don't type find)
                ApplicationEventsResolver.Current = new ApplicationEventsResolver(
                    new ActivatorServiceProvider(), Mock.Of<ILogger>(),
                    new Type[]
                    {
                        typeof(LegacyStartupHandler),
                        typeof(TestApplicationEventHandler)
                    })
                    {
                        CanResolveBeforeFrozen = true
                    };
            }
            
        }

        /// <summary>
        /// Test legacy startup handler
        /// </summary>
        public class LegacyStartupHandler : IApplicationStartupHandler
        {
            public static bool Initialized = false;

            public LegacyStartupHandler()
            {
                Initialized = true;
            }
        }

        /// <summary>
        /// test event handler
        /// </summary>
        public class TestApplicationEventHandler : IApplicationEventHandler
        {
            public static bool Initialized = false;
            public static bool Starting = false;
            public static bool Started = false;

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
        }

        [Test]
        public void Handle_IApplicationEventHandler_Objects_Outside_Web_Context()
        {
            _testApp.StartApplication(_testApp, new EventArgs());

            Assert.IsTrue(TestApplicationEventHandler.Initialized);
            Assert.IsTrue(TestApplicationEventHandler.Starting);
            Assert.IsTrue(TestApplicationEventHandler.Started);
        }

        [Test]
        public void Ensure_Legacy_Startup_Handlers_Not_Started_Until_Complete()
        {
            EventHandler starting = (sender, args) =>
                {
                    Assert.IsTrue(TestApplicationEventHandler.Initialized);
                    Assert.IsTrue(TestApplicationEventHandler.Starting);
                    Assert.IsFalse(LegacyStartupHandler.Initialized);
                };
            EventHandler started = (sender, args) =>
                {
                    Assert.IsTrue(TestApplicationEventHandler.Started);
                    Assert.IsTrue(LegacyStartupHandler.Initialized);
                };
            TestApp.ApplicationStarting += starting;
            TestApp.ApplicationStarted += started;

            _testApp.StartApplication(_testApp, new EventArgs());

            TestApp.ApplicationStarting -= starting;
            TestApp.ApplicationStarting -= started;

        }

    }
}
