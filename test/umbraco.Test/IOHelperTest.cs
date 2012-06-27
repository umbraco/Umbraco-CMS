using umbraco.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for IOHelperTest and is intended
    ///to contain all IOHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IOHelperTest
    {


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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for MapPath verifying that HttpContext method (which includes vdirs) matches non-HttpContext method
        ///</summary>
        [TestMethod()]
        public void IOHelper_MapPathTestVDirTraversal()
        {
            //System.Diagnostics.Debugger.Break();

            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Bin, true), IOHelper.MapPath(SystemDirectories.Bin, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Config, true), IOHelper.MapPath(SystemDirectories.Config, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Css, true), IOHelper.MapPath(SystemDirectories.Css, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Data, true), IOHelper.MapPath(SystemDirectories.Data, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Install, true), IOHelper.MapPath(SystemDirectories.Install, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Masterpages, true), IOHelper.MapPath(SystemDirectories.Masterpages, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Media, true), IOHelper.MapPath(SystemDirectories.Media, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Packages, true), IOHelper.MapPath(SystemDirectories.Packages, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Preview, true), IOHelper.MapPath(SystemDirectories.Preview, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Python, true), IOHelper.MapPath(SystemDirectories.Python, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Root, true), IOHelper.MapPath(SystemDirectories.Root, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Scripts, true), IOHelper.MapPath(SystemDirectories.Scripts, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Umbraco, true), IOHelper.MapPath(SystemDirectories.Umbraco, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Umbraco_client, true), IOHelper.MapPath(SystemDirectories.Umbraco_client, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Usercontrols, true), IOHelper.MapPath(SystemDirectories.Usercontrols, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Webservices, true), IOHelper.MapPath(SystemDirectories.Webservices, false));
            Assert.AreEqual(IOHelper.MapPath(SystemDirectories.Xslt, true), IOHelper.MapPath(SystemDirectories.Xslt, false));
        }
    }
}
