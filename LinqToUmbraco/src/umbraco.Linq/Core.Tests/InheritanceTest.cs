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
    /// Summary description for InheritanceTest
    /// </summary>
    [TestClass, DeploymentItem("umbraco.config")]
    public class InheritanceTest
    {
        public InheritanceTest()
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

        [TestMethod]
        public void InheritanceTest_ImplementsBaseClass()
        {
            var tmp = new TextPageThreeCol();

            Assert.AreEqual(typeof(CWSTextpageTwoCol), tmp.GetType().BaseType);
        }

        [TestMethod, Isolated]
        public void InheritanceTest_PropertyFromBase()
        {
            var fake = Isolate.Fake.Instance<TextPageThreeCol>();

            Assert.AreEqual(typeof(CWSTextpageTwoCol), fake.GetType().GetProperties().Single(p => p.Name == "Headertext").DeclaringType);
        }
    }
}
