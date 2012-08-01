using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using umbraco.Linq.Core.Node;
using umbraco.Test;
using System.IO;
using System.Web;

namespace umbraco.Linq.Core.Tests
{
    /// <summary>
    /// Summary description for DataContextTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class DataContextTest
    {
        public DataContextTest()
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

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void DataContextTest_FileNameRequired()
        {
            var nodeProvider = new NodeDataProvider(string.Empty);
        }

        [TestMethod, ExpectedException(typeof(FileNotFoundException))]
        public void DataContextTest_FileMustExist()
        {
            var nodeProvider = new NodeDataProvider(@"C:\fakeFile.config");
        }

        [TestMethod]
        public void DataContextTest_ConstructFromExistingProvider()
        {
            var nodeProvider = new NodeDataProvider(Path.Combine(Environment.CurrentDirectory, "umbraco.config"));
            
            MyUmbracoDataContext ctx = new MyUmbracoDataContext(nodeProvider);
        }

        [TestMethod]
        public void DataContextTest_EnsureSchemaValidation()
        {
            var nodeProvider = new NodeDataProvider(Path.Combine(Environment.CurrentDirectory, "umbraco.config"), true);

            MyUmbracoDataContext ctx = new MyUmbracoDataContext(nodeProvider);

            var hp = ctx.CWSHomes.First();

            Assert.IsNotNull(hp);
        }

        [TestMethod, Isolated]
        public void DataContextTest_DefaultConstructorIsNodeProvider()
        {
            MockHelpers.SetupFakeHttpContext();

            var ctx = new MyUmbracoDataContext();
            
            Assert.IsInstanceOfType(ctx.DataProvider, typeof(NodeDataProvider));
        }

        [TestMethod, Isolated]
        public void DataContextTest_NodeCached()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var hp = ctx.CWSHomes.First();
                var hp2 = ctx.CWSHomes.First();

                Assert.AreSame(hp, hp2);
            }
        }

        [TestMethod, Isolated]
        public void DataContextTest_FullCache()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var one = ctx.CWSHomes;
                var two = ctx.CWSHomes;

                Assert.AreSame(one, two);
            }
        }

        [TestMethod, Isolated]
        public void DataContextTest_OfNodeTypes()
        {
            MockHelpers.SetupFakeHttpContext();
            using (var ctx = new MyUmbracoDataContext())
            {
                var tree = ctx.CWSHomes;

                Assert.IsInstanceOfType(tree, typeof(NodeTree<CWSHome>));
                Assert.IsInstanceOfType(tree.First().CWSTextpages, typeof(NodeAssociationTree<CWSTextpage>));
            }
        }
    }
}
