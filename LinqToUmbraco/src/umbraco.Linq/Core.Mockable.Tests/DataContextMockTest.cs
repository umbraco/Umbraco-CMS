using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using umbraco.Linq.Core;
using umbraco.Test;

namespace umbraco.LinqCore.Mockable.Tests
{
    /// <summary>
    /// Summary description for DataContextMockTest
    /// </summary>
    [TestClass]
    public class DataContextMockTest
    {
        public DataContextMockTest()
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

        [TestMethod]
        public void DataContextMockTest_MockProvider()
        {
            var dataProvider = MockRepository.GenerateMock<umbracoDataProvider>();
            using (var ctx = new MyumbracoDataContext(dataProvider))
            {
            }
        }

        [TestMethod, ExpectedException(typeof(NotImplementedException))]
        public void DataContextMockTest_MockProviderThrowsException()
        {
            var dataProvider = MockRepository.GenerateMock<umbracoDataProvider>();

            dataProvider.Stub(d => d.LoadTree<CwsHome>()).Throw(new NotImplementedException());
            using (var ctx = new MyumbracoDataContext(dataProvider))
            {
                var homes = ctx.CwsHomes;
            }
        }
    }
}
