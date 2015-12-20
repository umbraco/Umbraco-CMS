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
using Umbraco.Core.Persistence;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;

namespace Umbraco.Tests.BootManagers
{
    [TestFixture]
    public class CoreBootManagerTests : BaseUmbracoConfigurationTest
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
            ResolverCollection.ResetAll();
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
        public void Raises_Events()
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

                _testApp.StartApplication(_testApp, new EventArgs());

                app.ApplicationStarting -= starting;
                app.ApplicationStarting -= started;
            }
           

        }

    }
}
