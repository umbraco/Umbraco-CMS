using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Tests.BootManagers
{
    [TestFixture]
    public class CoreBootManagerTests
    {

        private TestApp _testApp;

        [SetUp]
        public void Setup()
        {
            _testApp = new TestApp();
        }

        [TearDown]
        public void TearDown()
        {
            _testApp = null;
            
            ApplicationEventsResolver.Reset();
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
                    Enumerable.Empty<Type>())
                    {
                        CanResolveBeforeFrozen = true
                    };
                ApplicationEventsResolver.Current.AddType<TestApplicationEventHandler>();
            }

            protected override void InitializeResolvers()
            {
                //Do nothing as we don't want to initialize all resolvers in this test
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

    }
}
