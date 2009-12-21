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
    /// Summary description for ParentTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class ParentTest
    {
        public ParentTest()
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
        public void ParentTest_ParentIdExists()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var p = ctx.CWSTextpages.First();
                Assert.AreNotEqual(0, p.ParentNodeId);
            }
        }

        [TestMethod, Isolated]
        public void ParentTest_ParentCreated()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var page = ctx.CWSTextpages.Single(p => p.Id == 1099);

                Assert.AreEqual(1098, page.ParentNodeId);

                var parent = page.Parent<CWSHome>();

                Assert.AreEqual(page.ParentNodeId, parent.Id);
            }
        }

        [TestMethod, Isolated]
        public void ParentTest_TopParentNull()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var hp = ctx.CWSHomes.First();
                Assert.IsNull(hp.Parent<CWSHome>());
            }
        }

        [TestMethod, Isolated, ExpectedException(typeof(DocTypeMissMatchException))]
        public void ParentTest_InvalidParentType()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var p = ctx.CWSPhotos.First().Parent<CWSHome>();
            }
        }
    }
}
