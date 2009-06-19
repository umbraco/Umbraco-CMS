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
    /// Summary description for AncestorTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class AncestorTest
    {
        public AncestorTest()
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
        public void AncestorTest_NoWhereCondition_Exists()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var textPage = ctx.CwsTextpages.First();

                var hp = textPage.AncestorOrDefault<CwsHome>();

                Assert.IsNotNull(hp);
            }
        }

        [TestMethod, Isolated]
        public void AncestorTest_NoWhereCondition_NotExists()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var textPage = ctx.CwsTextpages.First();

                var a = textPage.AncestorOrDefault<CwsPhoto>();

                Assert.IsNull(a);
            }
        }

        [TestMethod, Isolated]
        public void AncestorTest_Where_Exists()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var photo = ctx.CwsPhotos.First(p => p.Name.Contains("umbraco"));
                var gallery = photo.AncestorOrDefault<CwsGallery>(g => g.Name.Contains("Codegarden"));

                Assert.IsNotNull(gallery);
            }
        }

        [TestMethod, Isolated]
        public void AncestorTest_Where_NotExists()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyumbracoDataContext())
            {
                var photo = ctx.CwsPhotos.First(p => p.Name.Contains("umbraco"));
                var gallery = photo.AncestorOrDefault<CwsGallery>(g => g.Name.Contains("Bookhouses"));

                Assert.IsNull(gallery);
            }
        }
    }
}
