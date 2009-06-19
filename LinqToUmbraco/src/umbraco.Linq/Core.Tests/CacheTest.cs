using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using umbraco.Test;

namespace umbraco.Linq.Core.Tests
{
    /// <summary>
    /// Summary description for CacheTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class CacheTest
    {
        public CacheTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod, Isolated]
        public void CacheTest_ForcedRefresh()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var hps = ctx.CwsHomes;

                ctx.CwsHomes.ReloadCache();

                var reloadedHps = ctx.CwsHomes;

                Assert.AreNotSame(reloadedHps, hps, "Force reload should result in a different object");
            }
        }

        [TestMethod, Isolated]
        public void CacheTest_RequeryLoadFromCache()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var hps = ctx.CwsHomes;

                var requeriedHps = ctx.CwsHomes;

                Assert.AreSame(requeriedHps, hps, "Requeried objects should result in the same collection");
            }
        }
    }
}
