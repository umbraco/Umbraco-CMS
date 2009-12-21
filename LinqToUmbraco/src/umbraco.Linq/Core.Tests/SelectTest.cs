using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using umbraco.Test;
using TypeMock.ArrangeActAssert;

namespace umbraco.Linq.Core.Tests
{
    /// <summary>
    /// Summary description for SelectTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class SelectTest
    {
        public SelectTest()
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
        public void SelectTest_LoopTree_Query()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var homePages = from hp in ctx.CWSHomes
                                select hp;

                Assert.IsNotNull(homePages);
                foreach (var item in homePages)
                {
                    Assert.IsTrue(item.BodyText.Length > 0);
                }
            }
        }

        [TestMethod, Isolated]
        public void SelectTest_LoopTree_Lambda()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var homePages = ctx.CWSHomes;

                Assert.IsNotNull(homePages);
                foreach (CWSHome item in homePages)
                {
                    Assert.IsTrue(item.BodyText.Length > 0);
                }
            }
        }

        [TestMethod, Isolated]
        public void SelectTest_SingleProperty_Query()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var homePageText = from hp in ctx.CWSHomes
                                   select hp.BodyText;

                Assert.IsNotNull(homePageText);
                foreach (var item in homePageText)
                {
                    Assert.IsNotNull(item);
                }
            }
        }

        [TestMethod, Isolated]
        public void SelectTest_SingleProperty_Lambda()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var homePageText = ctx.CWSHomes.Select(hp => hp.BodyText);

                Assert.IsNotNull(homePageText);
                foreach (var item in homePageText)
                {
                    Assert.IsNotNull(item);
                }
            }
        }

        [TestMethod, Isolated]
        public void SelectTest_Anonymous_Query()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var anon = from hp in ctx.CWSHomes
                           select new
                           {
                               hp.BodyText,
                               CreatedDate = hp.CreateDate
                           };

                Assert.IsNotNull(anon);
                foreach (var item in anon)
                {
                    Assert.IsNotNull(item.BodyText);
                    Assert.AreNotEqual(DateTime.MinValue, item.CreatedDate);
                }
            }
        }

        [TestMethod, Isolated]
        public void SelectTest_Anonymous_Lambda()
        {
            MockHelpers.SetupFakeHttpContext();

            using (var ctx = new MyUmbracoDataContext())
            {
                var anon = ctx.CWSHomes.Select(hp => new
                           {
                               hp.BodyText,
                               CreatedDate = hp.CreateDate
                           });

                Assert.IsNotNull(anon);
                foreach (var item in anon)
                {
                    Assert.IsNotNull(item.BodyText);
                    Assert.AreNotEqual(DateTime.MinValue, item.CreatedDate);
                }
            }
        }
    }
}
