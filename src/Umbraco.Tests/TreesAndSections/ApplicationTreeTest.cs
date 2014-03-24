using System.IO;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using System;
using System.Linq;
using ApplicationTree = umbraco.BusinessLogic.ApplicationTree;

namespace Umbraco.Tests.TreesAndSections
{
    
    
    /// <summary>
    ///This is a test class for ApplicationTreeTest and is intended
    ///to contain all ApplicationTreeTest Unit Tests
    ///</summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture, RequiresSTA]
    public class ApplicationTreeTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            var treesConfig = TestHelper.MapPathForTest("~/TEMP/TreesAndSections/trees.config");
            var appConfig = TestHelper.MapPathForTest("~/TEMP/TreesAndSections/applications.config");
            Directory.CreateDirectory(TestHelper.MapPathForTest("~/TEMP/TreesAndSections"));
            using (var writer = File.CreateText(treesConfig))
            {
                writer.Write(ResourceFiles.trees);
            }
            using (var writer = File.CreateText(appConfig))
            {
                writer.Write(ResourceFiles.applications);
            }

            ApplicationTreeService.TreeConfigFilePath = treesConfig;
            SectionService.AppConfigFilePath = appConfig;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            if (Directory.Exists(TestHelper.MapPathForTest("~/TEMP/TreesAndSections")))
            {
                Directory.Delete(TestHelper.MapPathForTest("~/TEMP/TreesAndSections"), true);    
            }
            ApplicationTreeService.TreeConfigFilePath = null;
            SectionService.AppConfigFilePath = null;
        }

        /// <summary>
        /// Creates a new app tree linked to an application, then delete the application and make sure the tree is gone as well
        ///</summary>
        [Test()]
        public void ApplicationTree_Make_New_Then_Delete_App()
        {
            //create new app
            var appName = Guid.NewGuid().ToString("N");
            var treeName = Guid.NewGuid().ToString("N");
            ApplicationContext.Services.SectionService.MakeNew(appName, appName, "icon.jpg");

            //check if it exists
            var app = ApplicationContext.Services.SectionService.GetByAlias(appName);
            Assert.IsNotNull(app);

            //create the new app tree assigned to the new app
            ApplicationContext.Services.ApplicationTreeService.MakeNew(false, 0, app.Alias, treeName, treeName, "icon.jpg", "icon.jpg", "Umbraco.Web.Trees.ContentTreeController, umbraco");
            var tree = ApplicationContext.Services.ApplicationTreeService.GetByAlias(treeName);
            Assert.IsNotNull(tree);

            //now delete the app
            ApplicationContext.Services.SectionService.DeleteSection(app);

            //check that the tree is gone
            Assert.AreEqual(0, ApplicationContext.Services.ApplicationTreeService.GetApplicationTrees(treeName).Count());
        }


        #region Tests to write
        ///// <summary>
        /////A test for ApplicationTree Constructor
        /////</summary>
        //[TestMethod()]
        //public void ApplicationTreeConstructorTest()
        //{
        //    bool silent = false; // TODO: Initialize to an appropriate value
        //    bool initialize = false; // TODO: Initialize to an appropriate value
        //    byte sortOrder = 0; // TODO: Initialize to an appropriate value
        //    string applicationAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
        //    string title = string.Empty; // TODO: Initialize to an appropriate value
        //    string iconClosed = string.Empty; // TODO: Initialize to an appropriate value
        //    string iconOpened = string.Empty; // TODO: Initialize to an appropriate value
        //    string assemblyName = string.Empty; // TODO: Initialize to an appropriate value
        //    string type = string.Empty; // TODO: Initialize to an appropriate value
        //    string action = string.Empty; // TODO: Initialize to an appropriate value
        //    ApplicationTree target = new ApplicationTree(silent, initialize, sortOrder, applicationAlias, alias, title, iconClosed, iconOpened, assemblyName, type, action);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for ApplicationTree Constructor
        /////</summary>
        //[TestMethod()]
        //public void ApplicationTreeConstructorTest1()
        //{
        //    ApplicationTree target = new ApplicationTree();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}


        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for getAll
        /////</summary>
        //[TestMethod()]
        //public void getAllTest()
        //{
        //    ApplicationTree[] expected = null; // TODO: Initialize to an appropriate value
        //    ApplicationTree[] actual;
        //    actual = ApplicationTree.getAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getApplicationTree
        /////</summary>
        //[TestMethod()]
        //public void getApplicationTreeTest()
        //{
        //    string applicationAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    ApplicationTree[] expected = null; // TODO: Initialize to an appropriate value
        //    ApplicationTree[] actual;
        //    actual = ApplicationTree.getApplicationTree(applicationAlias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getApplicationTree
        /////</summary>
        //[TestMethod()]
        //public void getApplicationTreeTest1()
        //{
        //    string applicationAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    bool onlyInitializedApplications = false; // TODO: Initialize to an appropriate value
        //    ApplicationTree[] expected = null; // TODO: Initialize to an appropriate value
        //    ApplicationTree[] actual;
        //    actual = ApplicationTree.getApplicationTree(applicationAlias, onlyInitializedApplications);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getByAlias
        /////</summary>
        //[TestMethod()]
        //public void getByAliasTest()
        //{
        //    string treeAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    ApplicationTree expected = null; // TODO: Initialize to an appropriate value
        //    ApplicationTree actual;
        //    actual = ApplicationTree.getByAlias(treeAlias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Action
        /////</summary>
        //[TestMethod()]
        //public void ActionTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Action = expected;
        //    actual = target.Action;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Alias
        /////</summary>
        //[TestMethod()]
        //public void AliasTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Alias;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ApplicationAlias
        /////</summary>
        //[TestMethod()]
        //public void ApplicationAliasTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.ApplicationAlias;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for AssemblyName
        /////</summary>
        //[TestMethod()]
        //public void AssemblyNameTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.AssemblyName = expected;
        //    actual = target.AssemblyName;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IconClosed
        /////</summary>
        //[TestMethod()]
        //public void IconClosedTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.IconClosed = expected;
        //    actual = target.IconClosed;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IconOpened
        /////</summary>
        //[TestMethod()]
        //public void IconOpenedTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.IconOpened = expected;
        //    actual = target.IconOpened;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Initialize
        /////</summary>
        //[TestMethod()]
        //public void InitializeTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.Initialize = expected;
        //    actual = target.Initialize;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Silent
        /////</summary>
        //[TestMethod()]
        //public void SilentTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.Silent = expected;
        //    actual = target.Silent;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for SortOrder
        /////</summary>
        //[TestMethod()]
        //public void SortOrderTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    byte expected = 0; // TODO: Initialize to an appropriate value
        //    byte actual;
        //    target.SortOrder = expected;
        //    actual = target.SortOrder;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for SqlHelper
        /////</summary>
        //[TestMethod()]
        //public void SqlHelperTest()
        //{
        //    ISqlHelper actual;
        //    actual = ApplicationTree.SqlHelper;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Title
        /////</summary>
        //[TestMethod()]
        //public void TitleTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Title = expected;
        //    actual = target.Title;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Type
        /////</summary>
        //[TestMethod()]
        //public void TypeTest()
        //{
        //    ApplicationTree target = new ApplicationTree(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Type = expected;
        //    actual = target.Type;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //} 
        #endregion

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
    }
}
