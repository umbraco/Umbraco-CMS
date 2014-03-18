using System.IO;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using umbraco.BusinessLogic;
using System;
using System.Linq;
using Umbraco.Core;

namespace Umbraco.Tests.TreesAndSections
{
    /// <summary>
    ///This is a test class for ApplicationTest and is intended
    ///to contain all ApplicationTest Unit Tests
    ///</summary>
    [RequiresAutoMapperMappings]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture()]
    public class SectionTests : BaseDatabaseFactoryTest
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
        /// Create a new application and delete it
        ///</summary>
        [Test()]
        public void Application_Make_New()
        {
            var name = Guid.NewGuid().ToString("N");
            ApplicationContext.Current.Services.SectionService.MakeNew(name, name, "icon.jpg");
            
            //check if it exists
            var app = ApplicationContext.Current.Services.SectionService.GetByAlias(name);
            Assert.IsNotNull(app);

            //now remove it
            ApplicationContext.Current.Services.SectionService.DeleteSection(app);
            Assert.IsNull(ApplicationContext.Current.Services.SectionService.GetByAlias(name));
        }

        /// <summary>
        /// Creates a new user, assigns the user to existing application, 
        /// then deletes the user
        /// </summary>
        [Test()]
        public void Application_Create_New_User_Assign_Application_And_Delete_User()
        {
            var name = Guid.NewGuid().ToString("N");
         
            //new user
            var ut = UserType.GetAllUserTypes().First();
            var user = User.MakeNew(name, name, name, ut);

            //get application
            //var app = Application.getAll().First();

            //assign the app
            user.addApplication(Constants.Applications.Content);
            //ensure it's added
            Assert.AreEqual(1, user.Applications.Count(x => x.alias == Constants.Applications.Content));

            //delete the user
            user.delete();

            //make sure the assigned applications are gone
            Assert.AreEqual(0, user.Applications.Count(x => x.alias == name));
        }

        /// <summary>
        /// create a new application and assigne an new user and deletes the application making sure the assignments are removed
        /// </summary>
        [Test()]
        public void Application_Make_New_Assign_User_And_Delete()
        {
            var name = Guid.NewGuid().ToString("N");

            //new user
            var ut = UserType.GetAllUserTypes().First();
            var user = User.MakeNew(name, name, name, ut);

            ApplicationContext.Current.Services.SectionService.MakeNew(name, name, "icon.jpg");

            //check if it exists
            var app = ApplicationContext.Current.Services.SectionService.GetByAlias(name);
            Assert.IsNotNull(app);

            //assign the app
            user.addApplication(app.Alias);
            //ensure it's added
            Assert.AreEqual(1, user.Applications.Count(x => x.alias == app.Alias));

            //delete the app
            ApplicationContext.Current.Services.SectionService.DeleteSection(app);

            //make sure the assigned applications are gone
            Assert.AreEqual(0, user.Applications.Count(x => x.alias == name));
        }

        #region Tests to write


        ///// <summary>
        /////A test for Application Constructor
        /////</summary>
        //[TestMethod()]
        //public void ApplicationConstructorTest()
        //{
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
        //    string icon = string.Empty; // TODO: Initialize to an appropriate value
        //    Application target = new Application(name, alias, icon);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Application Constructor
        /////</summary>
        //[TestMethod()]
        //public void ApplicationConstructorTest1()
        //{
        //    Application target = new Application();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    Application target = new Application(); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

       

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest1()
        //{
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
        //    string icon = string.Empty; // TODO: Initialize to an appropriate value
        //    Application.MakeNew(name, alias, icon);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for RegisterIApplications
        /////</summary>
        //[TestMethod()]
        //public void RegisterIApplicationsTest()
        //{
        //    Application.RegisterIApplications();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for getAll
        /////</summary>
        //[TestMethod()]
        //public void getAllTest()
        //{
        //    List<Application> expected = null; // TODO: Initialize to an appropriate value
        //    List<Application> actual;
        //    actual = Application.getAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getByAlias
        /////</summary>
        //[TestMethod()]
        //public void getByAliasTest()
        //{
        //    string appAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    Application expected = null; // TODO: Initialize to an appropriate value
        //    Application actual;
        //    actual = Application.getByAlias(appAlias);
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
        //    actual = Application.SqlHelper;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for alias
        /////</summary>
        //[TestMethod()]
        //public void aliasTest()
        //{
        //    Application target = new Application(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.alias = expected;
        //    actual = target.alias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for icon
        /////</summary>
        //[TestMethod()]
        //public void iconTest()
        //{
        //    Application target = new Application(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.icon = expected;
        //    actual = target.icon;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for name
        /////</summary>
        //[TestMethod()]
        //public void nameTest()
        //{
        //    Application target = new Application(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.name = expected;
        //    actual = target.name;
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
