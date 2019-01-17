//using System.IO;
//using NUnit.Framework;
//using Umbraco.Core.Services;
//using Umbraco.Tests.TestHelpers;
//using System;
//using Umbraco.Core.Composing;
//using Umbraco.Tests.Testing;
//using Umbraco.Web.Services;

//namespace Umbraco.Tests.TreesAndSections
//{
//    /// <summary>
//    ///This is a test class for ApplicationTest and is intended
//    ///to contain all ApplicationTest Unit Tests
//    ///</summary>
//    [TestFixture]
//    [UmbracoTest(AutoMapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
//    public class SectionTests : TestWithDatabaseBase
//    {
//        protected override void Compose()
//        {
//            base.Compose();
//            Composition.RegisterUnique<ISectionService, SectionService>();
//        }

//        public override void SetUp()
//        {
//            base.SetUp();

//            var treesConfig = TestHelper.MapPathForTest("~/TEMP/TreesAndSections/trees.config");
//            var appConfig = TestHelper.MapPathForTest("~/TEMP/TreesAndSections/applications.config");
//            Directory.CreateDirectory(TestHelper.MapPathForTest("~/TEMP/TreesAndSections"));
//            using (var writer = File.CreateText(treesConfig))
//            {
//                writer.Write(ResourceFiles.trees);
//            }
//            using (var writer = File.CreateText(appConfig))
//            {
//                writer.Write(ResourceFiles.applications);
//            }

//            ApplicationTreeService.TreeConfigFilePath = treesConfig;
//            SectionService.AppConfigFilePath = appConfig;
//        }

//        public override void TearDown()
//        {
//            base.TearDown();

//            if (Directory.Exists(TestHelper.MapPathForTest("~/TEMP/TreesAndSections")))
//            {
//                Directory.Delete(TestHelper.MapPathForTest("~/TEMP/TreesAndSections"), true);
//            }
//            ApplicationTreeService.TreeConfigFilePath = null;
//            SectionService.AppConfigFilePath = null;
//        }

//        ///// <summary>
//        ///// Create a new application and delete it
//        /////</summary>
//        //[Test()]
//        //public void Application_Make_New()
//        //{
//        //    var name = Guid.NewGuid().ToString("N");
//        //    ServiceContext.SectionService.MakeNew(name, name, "icon.jpg");

//        //    //check if it exists
//        //    var app = ServiceContext.SectionService.GetByAlias(name);
//        //    Assert.IsNotNull(app);

//        //    //now remove it
//        //    ServiceContext.SectionService.DeleteSection(app);
//        //    Assert.IsNull(ServiceContext.SectionService.GetByAlias(name));
//        //}

//        #region Tests to write


//        ///// <summary>
//        /////A test for Application Constructor
//        /////</summary>
//        //[TestMethod()]
//        //public void ApplicationConstructorTest()
//        //{
//        //    string name = string.Empty; // TODO: Initialize to an appropriate value
//        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
//        //    string icon = string.Empty; // TODO: Initialize to an appropriate value
//        //    Application target = new Application(name, alias, icon);
//        //    Assert.Inconclusive("TODO: Implement code to verify target");
//        //}

//        ///// <summary>
//        /////A test for Application Constructor
//        /////</summary>
//        //[TestMethod()]
//        //public void ApplicationConstructorTest1()
//        //{
//        //    Application target = new Application();
//        //    Assert.Inconclusive("TODO: Implement code to verify target");
//        //}

//        ///// <summary>
//        /////A test for Delete
//        /////</summary>
//        //[TestMethod()]
//        //public void DeleteTest()
//        //{
//        //    Application target = new Application(); // TODO: Initialize to an appropriate value
//        //    target.Delete();
//        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        //}



//        ///// <summary>
//        /////A test for MakeNew
//        /////</summary>
//        //[TestMethod()]
//        //public void MakeNewTest1()
//        //{
//        //    string name = string.Empty; // TODO: Initialize to an appropriate value
//        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
//        //    string icon = string.Empty; // TODO: Initialize to an appropriate value
//        //    Application.MakeNew(name, alias, icon);
//        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        //}

//        ///// <summary>
//        /////A test for RegisterIApplications
//        /////</summary>
//        //[TestMethod()]
//        //public void RegisterIApplicationsTest()
//        //{
//        //    Application.RegisterIApplications();
//        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
//        //}

//        ///// <summary>
//        /////A test for getAll
//        /////</summary>
//        //[TestMethod()]
//        //public void getAllTest()
//        //{
//        //    List<Application> expected = null; // TODO: Initialize to an appropriate value
//        //    List<Application> actual;
//        //    actual = Application.getAll();
//        //    Assert.AreEqual(expected, actual);
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}

//        ///// <summary>
//        /////A test for getByAlias
//        /////</summary>
//        //[TestMethod()]
//        //public void getByAliasTest()
//        //{
//        //    string appAlias = string.Empty; // TODO: Initialize to an appropriate value
//        //    Application expected = null; // TODO: Initialize to an appropriate value
//        //    Application actual;
//        //    actual = Application.getByAlias(appAlias);
//        //    Assert.AreEqual(expected, actual);
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}

//        ///// <summary>
//        /////A test for SqlHelper
//        /////</summary>
//        //[TestMethod()]
//        //public void SqlHelperTest()
//        //{
//        //    ISqlHelper actual;
//        //    actual = Application.SqlHelper;
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}

//        ///// <summary>
//        /////A test for alias
//        /////</summary>
//        //[TestMethod()]
//        //public void aliasTest()
//        //{
//        //    Application target = new Application(); // TODO: Initialize to an appropriate value
//        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
//        //    string actual;
//        //    target.alias = expected;
//        //    actual = target.alias;
//        //    Assert.AreEqual(expected, actual);
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}

//        ///// <summary>
//        /////A test for icon
//        /////</summary>
//        //[TestMethod()]
//        //public void iconTest()
//        //{
//        //    Application target = new Application(); // TODO: Initialize to an appropriate value
//        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
//        //    string actual;
//        //    target.icon = expected;
//        //    actual = target.icon;
//        //    Assert.AreEqual(expected, actual);
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}

//        ///// <summary>
//        /////A test for name
//        /////</summary>
//        //[TestMethod()]
//        //public void nameTest()
//        //{
//        //    Application target = new Application(); // TODO: Initialize to an appropriate value
//        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
//        //    string actual;
//        //    target.name = expected;
//        //    actual = target.name;
//        //    Assert.AreEqual(expected, actual);
//        //    Assert.Inconclusive("Verify the correctness of this test method.");
//        //}
//        #endregion

//        #region Additional test attributes
//        //
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion
//    }
//}
