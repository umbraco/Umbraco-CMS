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
    /// Summary description for WhereTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class WhereTest
    {
        public WhereTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
        public void WhereTest_SingleParameter_Query()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyumbracoDataContext())
            {
                var pages = from page in ctx.CwsTextpages
                            where page.Bodytext.Length > 0
                            select page;

                Assert.IsNotNull(pages);
                Assert.IsTrue(pages.Count() > 0);
            }
        }

        [TestMethod, Isolated]
        public void WhereTest_DateTimeGreaterThan_Query()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyumbracoDataContext())
            {
                var pages = from page in ctx.CwsTextpages
                            where page.CreateDate > DateTime.MinValue
                            select page;

                Assert.IsNotNull(pages);
                Assert.IsTrue(pages.Count() > 0);
            }
        }

        [TestMethod, Isolated]
        public void WhereTest_TwoParameter_Query()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var pages = from page in ctx.CwsTextpages
                            where page.Bodytext.Length > 0
                            && page.CreateDate > DateTime.MinValue
                            select page;

                Assert.IsNotNull(pages);
                Assert.IsTrue(pages.Count() > 0);
            }
        }

        [TestMethod, Isolated]
        public void WhereTest_ToAnonymous()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var pages = from page in ctx.CwsTextpages
                            where page.Bodytext.Length > 0
                            select new
                            {
                                page.Id,
                                page.Bodytext
                            };

                Assert.AreNotEqual(0, pages.Count());
            }
        }
    }
}
