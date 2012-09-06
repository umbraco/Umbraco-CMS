using umbraco.providers.members;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Security;
using System.Collections.Specialized;
using umbraco.cms.businesslogic.member;
using umbraco.BusinessLogic;
using System.Linq;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for UmbracoMembershipProviderTest and is intended
    ///to contain all UmbracoMembershipProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UmbracoMembershipProviderTest
    {


        /// <summary>
        /// Create a new member with the provider, then re-get them with the provider and make sure they are the same
        ///</summary>
        [TestMethod()]
        public void MembershipProvider_Create_User()
        {
           
            string username = Guid.NewGuid().ToString("N");
            string password = Guid.NewGuid().ToString("N");
            string email = Guid.NewGuid().ToString("N") + "@email.com";
            string passwordQuestion = Guid.NewGuid().ToString("N");
            string passwordAnswer = Guid.NewGuid().ToString("N");
            bool isApproved = true;

            MembershipCreateStatus status = new MembershipCreateStatus(); // TODO: Initialize to an appropriate value

            var m = m_Provider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, null, out status);

            Assert.AreEqual<MembershipCreateStatus>(MembershipCreateStatus.Success, status);
            Assert.AreEqual<string>(email, m.Email);

            var m1 = m_Provider.GetUser(m.ProviderUserKey, false);
            Assert.AreEqual<string>(m.UserName, m1.UserName);

            //delete the member
            m_Provider.DeleteUser(username, true);

            //make sure its gone
            var hasException = false;
            try
            {
                m_Provider.GetUser(m.ProviderUserKey, false);
            }
            catch (ArgumentException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);
        }

        /// <summary>
        /// Create a new member and role and assign the member to the role, then cleanup
        /// </summary>
        [TestMethod()]
        public void MembershipProvider_Create_User_Assign_New_Role()
        {

            string username = Guid.NewGuid().ToString("N");
            string password = Guid.NewGuid().ToString("N");
            string email = Guid.NewGuid().ToString("N") + "@email.com";
            string passwordQuestion = Guid.NewGuid().ToString("N");
            string passwordAnswer = Guid.NewGuid().ToString("N");
            bool isApproved = true;

            MembershipCreateStatus status = new MembershipCreateStatus(); // TODO: Initialize to an appropriate value

            var m = m_Provider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, null, out status);

            Assert.AreEqual<MembershipCreateStatus>(MembershipCreateStatus.Success, status);
            Assert.AreEqual<string>(email, m.Email);

            var m1 = m_Provider.GetUser(m.ProviderUserKey, false);
            Assert.AreEqual<string>(m.UserName, m1.UserName);


            //create role provider
            var roleProvider = new UmbracoRoleProvider();
            roleProvider.Initialize(string.Empty, new NameValueCollection());
            var newRole = Guid.NewGuid().ToString("N");
            roleProvider.CreateRole(newRole);
            //make sure it's there
            Assert.AreEqual<int>(1, roleProvider.GetAllRoles().Where(x => x == newRole).Count());

            //add the user to the role
            roleProvider.AddUsersToRoles(new string[] { m.UserName }, new string[] { newRole });

            //make sure they are in it
            Assert.IsTrue(roleProvider.IsUserInRole(m.UserName, newRole));

            //delete the member
            m_Provider.DeleteUser(username, true);

            //make sure its gone
            var hasException = false;
            try
            {
                m_Provider.GetUser(m.ProviderUserKey, false);
            }
            catch (ArgumentException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);

            //remove the role, this will throw an exception if the member is still assigned the role
            roleProvider.DeleteRole(newRole, true);
        }


        private UmbracoMembershipProvider m_Provider;
        private MemberType m_MemberType;

        #region Tests to write


        ///// <summary>
        /////A test for UmbracoMembershipProvider Constructor
        /////</summary>
        //[TestMethod()]
        //public void UmbracoMembershipProviderConstructorTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for ChangePassword
        /////</summary>
        //[TestMethod()]
        //public void ChangePasswordTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    string oldPassword = string.Empty; // TODO: Initialize to an appropriate value
        //    string newPassword = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.ChangePassword(username, oldPassword, newPassword);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ChangePasswordQuestionAndAnswer
        /////</summary>
        //[TestMethod()]
        //public void ChangePasswordQuestionAndAnswerTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    string password = string.Empty; // TODO: Initialize to an appropriate value
        //    string newPasswordQuestion = string.Empty; // TODO: Initialize to an appropriate value
        //    string newPasswordAnswer = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

       

        ///// <summary>
        /////A test for DeleteUser
        /////</summary>
        //[TestMethod()]
        //public void DeleteUserTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    bool deleteAllRelatedData = false; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.DeleteUser(username, deleteAllRelatedData);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for EncodePassword
        /////</summary>
        //[TestMethod()]
        //public void EncodePasswordTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string password = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.EncodePassword(password);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for FindUsersByEmail
        /////</summary>
        //[TestMethod()]
        //public void FindUsersByEmailTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string emailToMatch = string.Empty; // TODO: Initialize to an appropriate value
        //    int pageIndex = 0; // TODO: Initialize to an appropriate value
        //    int pageSize = 0; // TODO: Initialize to an appropriate value
        //    int totalRecords = 0; // TODO: Initialize to an appropriate value
        //    int totalRecordsExpected = 0; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection expected = null; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection actual;
        //    actual = target.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        //    Assert.AreEqual(totalRecordsExpected, totalRecords);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for FindUsersByName
        /////</summary>
        //[TestMethod()]
        //public void FindUsersByNameTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string usernameToMatch = string.Empty; // TODO: Initialize to an appropriate value
        //    int pageIndex = 0; // TODO: Initialize to an appropriate value
        //    int pageSize = 0; // TODO: Initialize to an appropriate value
        //    int totalRecords = 0; // TODO: Initialize to an appropriate value
        //    int totalRecordsExpected = 0; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection expected = null; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection actual;
        //    actual = target.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords);
        //    Assert.AreEqual(totalRecordsExpected, totalRecords);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAllUsers
        /////</summary>
        //[TestMethod()]
        //public void GetAllUsersTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int pageIndex = 0; // TODO: Initialize to an appropriate value
        //    int pageSize = 0; // TODO: Initialize to an appropriate value
        //    int totalRecords = 0; // TODO: Initialize to an appropriate value
        //    int totalRecordsExpected = 0; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection expected = null; // TODO: Initialize to an appropriate value
        //    MembershipUserCollection actual;
        //    actual = target.GetAllUsers(pageIndex, pageSize, out totalRecords);
        //    Assert.AreEqual(totalRecordsExpected, totalRecords);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetNumberOfUsersOnline
        /////</summary>
        //[TestMethod()]
        //public void GetNumberOfUsersOnlineTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.GetNumberOfUsersOnline();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetPassword
        /////</summary>
        //[TestMethod()]
        //public void GetPasswordTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    string answer = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetPassword(username, answer);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetUser
        /////</summary>
        //[TestMethod()]
        //public void GetUserTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    bool userIsOnline = false; // TODO: Initialize to an appropriate value
        //    MembershipUser expected = null; // TODO: Initialize to an appropriate value
        //    MembershipUser actual;
        //    actual = target.GetUser(username, userIsOnline);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetUser
        /////</summary>
        //[TestMethod()]
        //public void GetUserTest1()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    object providerUserKey = null; // TODO: Initialize to an appropriate value
        //    bool userIsOnline = false; // TODO: Initialize to an appropriate value
        //    MembershipUser expected = null; // TODO: Initialize to an appropriate value
        //    MembershipUser actual;
        //    actual = target.GetUser(providerUserKey, userIsOnline);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetUserNameByEmail
        /////</summary>
        //[TestMethod()]
        //public void GetUserNameByEmailTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string email = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetUserNameByEmail(email);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Initialize
        /////</summary>
        //[TestMethod()]
        //public void InitializeTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    NameValueCollection config = null; // TODO: Initialize to an appropriate value
        //    target.Initialize(name, config);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ResetPassword
        /////</summary>
        //[TestMethod()]
        //public void ResetPasswordTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    string answer = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.ResetPassword(username, answer);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for UnEncodePassword
        /////</summary>
        //[TestMethod()]
        //public void UnEncodePasswordTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string encodedPassword = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.UnEncodePassword(encodedPassword);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for UnlockUser
        /////</summary>
        //[TestMethod()]
        //public void UnlockUserTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string userName = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.UnlockUser(userName);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for UpdateUser
        /////</summary>
        //[TestMethod()]
        //public void UpdateUserTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    MembershipUser user = null; // TODO: Initialize to an appropriate value
        //    target.UpdateUser(user);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ValidateUser
        /////</summary>
        //[TestMethod()]
        //public void ValidateUserTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string username = string.Empty; // TODO: Initialize to an appropriate value
        //    string password = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.ValidateUser(username, password);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ApplicationName
        /////</summary>
        //[TestMethod()]
        //public void ApplicationNameTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.ApplicationName = expected;
        //    actual = target.ApplicationName;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for EnablePasswordReset
        /////</summary>
        //[TestMethod()]
        //public void EnablePasswordResetTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.EnablePasswordReset;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for EnablePasswordRetrieval
        /////</summary>
        //[TestMethod()]
        //public void EnablePasswordRetrievalTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.EnablePasswordRetrieval;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MaxInvalidPasswordAttempts
        /////</summary>
        //[TestMethod()]
        //public void MaxInvalidPasswordAttemptsTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.MaxInvalidPasswordAttempts;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MinRequiredNonAlphanumericCharacters
        /////</summary>
        //[TestMethod()]
        //public void MinRequiredNonAlphanumericCharactersTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.MinRequiredNonAlphanumericCharacters;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MinRequiredPasswordLength
        /////</summary>
        //[TestMethod()]
        //public void MinRequiredPasswordLengthTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.MinRequiredPasswordLength;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for PasswordAttemptWindow
        /////</summary>
        //[TestMethod()]
        //public void PasswordAttemptWindowTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.PasswordAttemptWindow;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for PasswordFormat
        /////</summary>
        //[TestMethod()]
        //public void PasswordFormatTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    MembershipPasswordFormat actual;
        //    actual = target.PasswordFormat;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for PasswordStrengthRegularExpression
        /////</summary>
        //[TestMethod()]
        //public void PasswordStrengthRegularExpressionTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.PasswordStrengthRegularExpression;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RequiresQuestionAndAnswer
        /////</summary>
        //[TestMethod()]
        //public void RequiresQuestionAndAnswerTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.RequiresQuestionAndAnswer;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RequiresUniqueEmail
        /////</summary>
        //[TestMethod()]
        //public void RequiresUniqueEmailTest()
        //{
        //    UmbracoMembershipProvider target = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.RequiresUniqueEmail;
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
        
        [TestInitialize()]
        public void MyTestInitialize()
        {
            //need to create a member type for the provider
            m_MemberType = MemberType.MakeNew(User.GetUser(0), Guid.NewGuid().ToString("N"));

            m_Provider = new UmbracoMembershipProvider(); // TODO: Initialize to an appropriate value
            
            //initialize the provider
            var config = new NameValueCollection();
            config.Add("enablePasswordRetrieval", "false");
            config.Add("enablePasswordReset", "false");
            config.Add("requiresQuestionAndAnswer", "false");
            config.Add("defaultMemberTypeAlias", m_MemberType.Alias);
            config.Add("passwordFormat", "Hashed");

            m_Provider.Initialize(string.Empty, config);            
            
        }
        
        
        [TestCleanup()]
        public void MyTestCleanup()
        {
            //remove the member type
            m_MemberType.delete();
        }
        
        #endregion
    }
}
