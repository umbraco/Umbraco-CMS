using umbraco.BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using umbraco.DataLayer;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for UserTest and is intended
    ///to contain all UserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UserTest
    {

        /// <summary>
        /// Make sure that the admin account cannot be deleted
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void User_Delete_Admin_Not_Allowed() 
        {
            User u = new User(0);
            u.delete();
        }

        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void User_Not_Found_Constructor()
        {
            User u = new User(-1111);
        }

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void User_Make_New_Override1()
        {
            //System.Diagnostics.Debugger.Break();
            var name = "TEST" + Guid.NewGuid().ToString("N");
            var username = "TEST" + Guid.NewGuid().ToString("N");
            var password = "TEST" + Guid.NewGuid().ToString("N");
            UserType ut = UserType.GetAllUserTypes().First();
            User.MakeNew(name, username, password, ut);

            //make sure it's there
            var id = User.getUserId(username);
            Assert.IsTrue(id > 0);

            var user = User.GetUser(id);
            Assert.AreEqual(id, user.Id);

            //System.Diagnostics.Debugger.Launch();
            user.delete();

            var stillUser = User.GetUser(id);
            Assert.IsNull(stillUser);
        }

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void User_Make_New_Override2()
        {
            var name = "TEST" + Guid.NewGuid().ToString("N");
            var username = "TEST" + Guid.NewGuid().ToString("N");
            var password = "TEST" + Guid.NewGuid().ToString("N");
            var email = "TEST" + Guid.NewGuid().ToString("N") + "@test.com"; 

            UserType ut = UserType.GetAllUserTypes().First();
            User.MakeNew(name, username, password, email, ut);

            //make sure it's there
            var id = User.getUserId(username);
            Assert.IsTrue(id > 0);

            var user = User.GetUser(id);
            Assert.AreEqual(id, user.Id);

            user.delete();

            var stillUser = User.GetUser(id);
            Assert.IsNull(stillUser);
        }

        [TestMethod()]
        public void User_Make_New_Duplicate_Login()
        {
            var name1 = "TEST" + Guid.NewGuid().ToString("N");
            var name2 = "TEST" + Guid.NewGuid().ToString("N");
            var username = "TEST" + Guid.NewGuid().ToString("N");
            var password1 = "TEST" + Guid.NewGuid().ToString("N");
            var password2 = "TEST" + Guid.NewGuid().ToString("N");

            UserType ut = UserType.GetAllUserTypes().First();
            
            User.MakeNew(name1, username, password1, ut);

            var hasError = false;
            try
            {
                User.MakeNew(name2, username, password2, ut);
            }
            catch (SqlHelperException)
            {
                hasError = true;
            }
            Assert.IsTrue(hasError);

            var user = User.GetUser(User.getUserId(username));
            
            //TODO: Move to common method
            user.delete();
            var stillUser = User.GetUser(User.getUserId(username));
            Assert.IsNull(stillUser);
        }

        #region Tests to write

        ///// <summary>
        /////A test for UserType
        /////</summary>
        //[TestMethod()]
        //public void UserTypeTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    UserType expected = null; // TODO: Initialize to an appropriate value
        //    UserType actual;
        //    target.UserType = expected;
        //    actual = target.UserType;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for StartNodeId
        /////</summary>
        //[TestMethod()]
        //public void StartNodeIdTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.StartNodeId = expected;
        //    actual = target.StartNodeId;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for StartMediaId
        /////</summary>
        //[TestMethod()]
        //public void StartMediaIdTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.StartMediaId = expected;
        //    actual = target.StartMediaId;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Password
        /////</summary>
        //[TestMethod()]
        //public void PasswordTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Password = expected;
        //    actual = target.Password;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for NoConsole
        /////</summary>
        //[TestMethod()]
        //public void NoConsoleTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.NoConsole = expected;
        //    actual = target.NoConsole;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Name
        /////</summary>
        //[TestMethod()]
        //public void NameTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Name = expected;
        //    actual = target.Name;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for LoginName
        /////</summary>
        //[TestMethod()]
        //public void LoginNameTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.LoginName = expected;
        //    actual = target.LoginName;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Language
        /////</summary>
        //[TestMethod()]
        //public void LanguageTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Language = expected;
        //    actual = target.Language;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.Id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Email
        /////</summary>
        //[TestMethod()]
        //public void EmailTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Email = expected;
        //    actual = target.Email;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Disabled
        /////</summary>
        //[TestMethod()]
        //public void DisabledTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.Disabled = expected;
        //    actual = target.Disabled;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for DefaultToLiveEditing
        /////</summary>
        //[TestMethod()]
        //public void DefaultToLiveEditingTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.DefaultToLiveEditing = expected;
        //    actual = target.DefaultToLiveEditing;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Applications
        /////</summary>
        //[TestMethod()]
        //public void ApplicationsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    Application[] actual;
        //    actual = target.Applications;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ValidatePassword
        /////</summary>
        //[TestMethod()]
        //public void ValidatePasswordTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string password = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.ValidatePassword(password);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for validateCredentials
        /////</summary>
        //[TestMethod()]
        //public void validateCredentialsTest1()
        //{
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string passw = string.Empty; // TODO: Initialize to an appropriate value
        //    bool checkForUmbracoConsoleAccess = false; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = User.validateCredentials(lname, passw, checkForUmbracoConsoleAccess);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for validateCredentials
        /////</summary>
        //[TestMethod()]
        //public void validateCredentialsTest()
        //{
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string passw = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = User.validateCredentials(lname, passw);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Update
        /////</summary>
        //[TestMethod()]
        //public void UpdateTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string email = string.Empty; // TODO: Initialize to an appropriate value
        //    UserType ut = null; // TODO: Initialize to an appropriate value
        //    User.Update(id, name, lname, email, ut);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for resetNotificationCache
        /////</summary>
        //[TestMethod()]
        //public void resetNotificationCacheTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.resetNotificationCache();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest1()
        //{
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string passw = string.Empty; // TODO: Initialize to an appropriate value
        //    UserType ut = null; // TODO: Initialize to an appropriate value
        //    User.MakeNew(name, lname, passw, ut);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest()
        //{
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string passw = string.Empty; // TODO: Initialize to an appropriate value
        //    string email = string.Empty; // TODO: Initialize to an appropriate value
        //    UserType ut = null; // TODO: Initialize to an appropriate value
        //    User.MakeNew(name, lname, passw, email, ut);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for IsRoot
        /////</summary>
        //[TestMethod()]
        //public void IsRootTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.IsRoot();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IsAdmin
        /////</summary>
        //[TestMethod()]
        //public void IsAdminTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.IsAdmin();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for initNotifications
        /////</summary>
        //[TestMethod()]
        //public void initNotificationsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.initNotifications();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for initCruds
        /////</summary>
        //[TestMethod()]
        //public void initCrudsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.initCruds();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for getUserId
        /////</summary>
        //[TestMethod()]
        //public void getUserIdTest1()
        //{
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = User.getUserId(lname);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getUserId
        /////</summary>
        //[TestMethod()]
        //public void getUserIdTest()
        //{
        //    string lname = string.Empty; // TODO: Initialize to an appropriate value
        //    string passw = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = User.getUserId(lname, passw);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetUser
        /////</summary>
        //[TestMethod()]
        //public void GetUserTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    User expected = null; // TODO: Initialize to an appropriate value
        //    User actual;
        //    actual = User.GetUser(id);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetPermissions
        /////</summary>
        //[TestMethod()]
        //public void GetPermissionsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string Path = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetPermissions(Path);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetPassword
        /////</summary>
        //[TestMethod()]
        //public void GetPasswordTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetPassword();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetNotifications
        /////</summary>
        //[TestMethod()]
        //public void GetNotificationsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string Path = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetNotifications(Path);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetCurrent
        /////</summary>
        //[TestMethod()]
        //public void GetCurrentTest()
        //{
        //    User expected = null; // TODO: Initialize to an appropriate value
        //    User actual;
        //    actual = User.GetCurrent();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getAllByLoginName
        /////</summary>
        //[TestMethod()]
        //public void getAllByLoginNameTest()
        //{
        //    string login = string.Empty; // TODO: Initialize to an appropriate value
        //    User[] expected = null; // TODO: Initialize to an appropriate value
        //    User[] actual;
        //    actual = User.getAllByLoginName(login);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getAllByEmail
        /////</summary>
        //[TestMethod()]
        //public void getAllByEmailTest()
        //{
        //    string email = string.Empty; // TODO: Initialize to an appropriate value
        //    User[] expected = null; // TODO: Initialize to an appropriate value
        //    User[] actual;
        //    actual = User.getAllByEmail(email);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getAll
        /////</summary>
        //[TestMethod()]
        //public void getAllTest()
        //{
        //    User[] expected = null; // TODO: Initialize to an appropriate value
        //    User[] actual;
        //    actual = User.getAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for disable
        /////</summary>
        //[TestMethod()]
        //public void disableTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.disable();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for clearApplications
        /////</summary>
        //[TestMethod()]
        //public void clearApplicationsTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    target.clearApplications();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for addApplication
        /////</summary>
        //[TestMethod()]
        //public void addApplicationTest()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID); // TODO: Initialize to an appropriate value
        //    string AppAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    target.addApplication(AppAlias);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for User Constructor
        /////</summary>
        //[TestMethod()]
        //public void UserConstructorTest3()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    bool noSetup = false; // TODO: Initialize to an appropriate value
        //    User target = new User(ID, noSetup);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for User Constructor
        /////</summary>
        //[TestMethod()]
        //public void UserConstructorTest2()
        //{
        //    int ID = 0; // TODO: Initialize to an appropriate value
        //    User target = new User(ID);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for User Constructor
        /////</summary>
        //[TestMethod()]
        //public void UserConstructorTest1()
        //{
        //    string Login = string.Empty; // TODO: Initialize to an appropriate value
        //    User target = new User(Login);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for User Constructor
        /////</summary>
        //[TestMethod()]
        //public void UserConstructorTest()
        //{
        //    string Login = string.Empty; // TODO: Initialize to an appropriate value
        //    string Password = string.Empty; // TODO: Initialize to an appropriate value
        //    User target = new User(Login, Password);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
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
