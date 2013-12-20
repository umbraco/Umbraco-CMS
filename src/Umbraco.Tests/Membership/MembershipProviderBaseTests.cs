using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using NUnit.Framework;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    public class MembersMembershipProviderTests
    {
        //[Test]
        //public void Set_Default_Member_Type_On_Init()

        //[Test]
        //public void Question_Answer_Is_Encrypted()
    }

    [TestFixture]
    public class MembershipProviderBaseTests
    {
        //[Test]
        //public void Change_Password_Base_Validation()
        
        //[Test]
        //public void ChangePasswordQuestionAndAnswer_Base_Validation()

        //[Test]
        //public void CreateUser_Base_Validation()

        //[Test]
        //public void GetPassword_Base_Validation()

        //[Test]
        //public void ResetPassword_Base_Validation()

        [Test]
        public void Sets_Defaults()
        {
            var provider = new TestProvider();
            provider.Initialize("test", new NameValueCollection());

            Assert.AreEqual("test", provider.Name);
            Assert.AreEqual(MembershipProviderBase.GetDefaultAppName(), provider.ApplicationName);
            Assert.AreEqual(false, provider.EnablePasswordRetrieval);
            Assert.AreEqual(false, provider.EnablePasswordReset);
            Assert.AreEqual(false, provider.RequiresQuestionAndAnswer);
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
            Assert.AreEqual(5, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(10, provider.PasswordAttemptWindow);
            Assert.AreEqual(provider.DefaultMinPasswordLength, provider.MinRequiredPasswordLength);
            Assert.AreEqual(provider.DefaultMinNonAlphanumericChars, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(null, provider.PasswordStrengthRegularExpression);
            Assert.AreEqual(provider.DefaultUseLegacyEncoding, provider.UseLegacyEncoding);
            Assert.AreEqual(MembershipPasswordFormat.Hashed, provider.PasswordFormat);
        }

        [Test]
        public void Throws_Exception_With_Hashed_Password_And_Password_Retrieval()
        {
            var provider = new TestProvider();

            Assert.Throws<ProviderException>(() => provider.Initialize("test", new NameValueCollection()
                {
                    {"enablePasswordRetrieval", "true"},
                    {"passwordFormat", "Hashed"}
                }));
        }

        [TestCase("hello", 0, "", 5, true)]
        [TestCase("hello", 0, "", 4, true)]
        [TestCase("hello", 0, "", 6, false)]
        [TestCase("hello", 1, "", 5, false)]
        [TestCase("hello!", 1, "", 0, true)]
        [TestCase("hello!", 2, "", 0, false)]
        [TestCase("hello!!", 2, "", 0, true)]
        //8 characters or more in length, at least 1 lowercase letter,at least 1 character that is not a lower letter.
        [TestCase("hello", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("helloooo", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("helloooO", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, true)]
        [TestCase("HELLOOOO", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, false)]
        [TestCase("HELLOOOo", 0, "(?=.{8,})[a-z]+[^a-z]+|[^a-z]+[a-z]+", 0, true)]
        public void Valid_Password(string password, int minRequiredNonAlphanumericChars, string strengthRegex, int minLength, bool pass)
        {
            var result = MembershipProviderBase.IsPasswordValid(password, minRequiredNonAlphanumericChars, strengthRegex, minLength);
            Assert.AreEqual(pass, result.Success);
        }

        /// <summary>
        /// The salt generated is always the same length
        /// </summary>
        [Test]
        public void Check_Salt_Length()
        {
            var lastLength = 0;
            for (var i = 0; i < 10000; i++)
            {
                var result = MembershipProviderBase.GenerateSalt();
                
                if (i > 0)
                {
                    Assert.AreEqual(lastLength, result.Length);
                }

                lastLength = result.Length;                
            }
        }

        [Test]
        public void Get_StoredPassword()
        {
            var salt = MembershipProviderBase.GenerateSalt();
            var stored = salt + "ThisIsAHashedPassword";

            string initSalt;
            var result = MembershipProviderBase.StoredPassword(stored, MembershipPasswordFormat.Hashed, out initSalt);

            Assert.AreEqual(salt, initSalt);
        }

        private class TestProvider : MembershipProviderBase
        {
            public override void UpdateUser(MembershipUser user)
            {
                throw new NotImplementedException();
            }

            public override bool ValidateUser(string username, string password)
            {
                throw new NotImplementedException();
            }

            public override bool UnlockUser(string userName)
            {
                throw new NotImplementedException();
            }

            public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
            {
                throw new NotImplementedException();
            }

            public override MembershipUser GetUser(string username, bool userIsOnline)
            {
                throw new NotImplementedException();
            }

            public override string GetUserNameByEmail(string email)
            {
                throw new NotImplementedException();
            }

            public override bool DeleteUser(string username, bool deleteAllRelatedData)
            {
                throw new NotImplementedException();
            }

            public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            public override int GetNumberOfUsersOnline()
            {
                throw new NotImplementedException();
            }

            public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
            {
                throw new NotImplementedException();
            }

            protected override bool PerformChangePassword(string username, string oldPassword, string newPassword)
            {
                throw new NotImplementedException();
            }

            protected override bool PerformChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
            {
                throw new NotImplementedException();
            }

            protected override MembershipUser PerformCreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
            {
                throw new NotImplementedException();
            }

            protected override string PerformGetPassword(string username, string answer)
            {
                throw new NotImplementedException();
            }

            protected override string PerformResetPassword(string username, string answer, string generatedPassword)
            {
                throw new NotImplementedException();
            }
        }

    }
}
