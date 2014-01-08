using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Security;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    public class MembershipProviderBaseTests
    {
        

        [Test]
        public void Change_Password_Without_AllowManuallyChangingPassword_And_No_Pass_Validation()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };         
            providerMock.Setup(@base => @base.AllowManuallyChangingPassword).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<NotSupportedException>(() => provider.ChangePassword("test", "", "test"));
        }

        [Test]
        public void Change_Password_With_AllowManuallyChangingPassword_And_Invalid_Creds()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };         
            providerMock.Setup(@base => @base.AllowManuallyChangingPassword).Returns(false);
            providerMock.Setup(@base => @base.ValidateUser("test", "test")).Returns(false);
            var provider = providerMock.Object;

            Assert.IsFalse(provider.ChangePassword("test", "test", "test"));

        }
        
        [Test]
        public void ChangePasswordQuestionAndAnswer_Without_RequiresQuestionAndAnswer()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.RequiresQuestionAndAnswer).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<NotSupportedException>(() => provider.ChangePasswordQuestionAndAnswer("test", "test", "test", "test"));
        }

        [Test]
        public void ChangePasswordQuestionAndAnswer_Without_AllowManuallyChangingPassword_And_Invalid_Creds()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.RequiresQuestionAndAnswer).Returns(true);
            providerMock.Setup(@base => @base.AllowManuallyChangingPassword).Returns(false);
            providerMock.Setup(@base => @base.ValidateUser("test", "test")).Returns(false);
            var provider = providerMock.Object;

            Assert.IsFalse(provider.ChangePasswordQuestionAndAnswer("test", "test", "test", "test"));
        }

        [Test]
        public void CreateUser_Not_Whitespace()
        {
            var providerMock = new Mock<MembershipProviderBase>() {CallBase = true};
            var provider = providerMock.Object;

            MembershipCreateStatus status;
            var result = provider.CreateUser("", "", "test@test.com", "", "", true, "", out status);

            Assert.IsNull(result);
            Assert.AreEqual(MembershipCreateStatus.InvalidUserName, status);
        }

        [Test]
        public void CreateUser_Invalid_Question()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.RequiresQuestionAndAnswer).Returns(true);
            var provider = providerMock.Object;

            MembershipCreateStatus status;
            var result = provider.CreateUser("test", "test", "test@test.com", "", "", true, "", out status);

            Assert.IsNull(result);
            Assert.AreEqual(MembershipCreateStatus.InvalidQuestion, status);
        }

        [Test]
        public void CreateUser_Invalid_Answer()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.RequiresQuestionAndAnswer).Returns(true);
            var provider = providerMock.Object;

            MembershipCreateStatus status;
            var result = provider.CreateUser("test", "test", "test@test.com", "test", "", true, "", out status);

            Assert.IsNull(result);
            Assert.AreEqual(MembershipCreateStatus.InvalidAnswer, status);
        }

        [Test]
        public void GetPassword_Without_EnablePasswordRetrieval()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordRetrieval).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<ProviderException>(() => provider.GetPassword("test", "test"));
        }

        [Test]
        public void GetPassword_With_Hashed()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordRetrieval).Returns(true);
            providerMock.Setup(@base => @base.PasswordFormat).Returns(MembershipPasswordFormat.Hashed);
            var provider = providerMock.Object;

            Assert.Throws<ProviderException>(() => provider.GetPassword("test", "test"));
        }

        [Test]
        public void ResetPassword_Without_EnablePasswordReset()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordReset).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<NotSupportedException>(() => provider.ResetPassword("test", "test"));
        }

        [Test]
        public void Sets_Defaults()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };                  
            var provider = providerMock.Object;
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
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };         
            var provider = providerMock.Object;

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
        public void Get_Stored_Password_Hashed()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });

            var salt = MembershipProviderBase.GenerateSalt();
            var stored = salt + "ThisIsAHashedPassword";

            string initSalt;
            var result = provider.StoredPassword(stored, out initSalt);

            Assert.AreEqual("ThisIsAHashedPassword", result);
        }

        [Test]
        public void Get_Stored_Password_Encrypted()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });

            var stored = "ThisIsAnEncryptedPassword";

            string initSalt;
            var result = provider.StoredPassword(stored, out initSalt);

            Assert.AreEqual("ThisIsAnEncryptedPassword", result);
        }

        [Test]
        public void Get_Stored_Password_Clear()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Clear" } });

            var salt = MembershipProviderBase.GenerateSalt();
            var stored = "ThisIsAClearPassword";

            string initSalt;
            var result = provider.StoredPassword(stored, out initSalt);

            Assert.AreEqual("ThisIsAClearPassword", result);
        }

        [Test]
        public void Format_Pass_For_Storage_Hashed()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });

            var salt = MembershipProviderBase.GenerateSalt();
            var stored = "ThisIsAHashedPassword";

            var result = provider.FormatPasswordForStorage(stored, salt);

            Assert.AreEqual(salt + "ThisIsAHashedPassword", result);
        }

        [Test]
        public void Format_Pass_For_Storage_Encrypted()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });

            var salt = MembershipProviderBase.GenerateSalt();
            var stored = "ThisIsAnEncryptedPassword";

            var result = provider.FormatPasswordForStorage(stored, salt);

            Assert.AreEqual("ThisIsAnEncryptedPassword", result);
        }

        [Test]
        public void Format_Pass_For_Storage_Clear()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Clear" } });

            var salt = MembershipProviderBase.GenerateSalt();
            var stored = "ThisIsAClearPassword";

            var result = provider.FormatPasswordForStorage(stored, salt);

            Assert.AreEqual("ThisIsAClearPassword", result);
        }

        [Test]
        public void Check_Password_Hashed_KeyedHashAlgorithm()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = provider.EncryptOrHashNewPassword(pass, out salt);
            var storedPassword = provider.FormatPasswordForStorage(hashed, salt);

            var result = provider.CheckPassword("ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Hashed_Non_KeyedHashAlgorithm()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" } });

            string salt;
            var pass = "ThisIsAHashedPassword";
            var hashed = provider.EncryptOrHashNewPassword(pass, out salt);
            var storedPassword = provider.FormatPasswordForStorage(hashed, salt);

            var result = provider.CheckPassword("ThisIsAHashedPassword", storedPassword);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Encrypted()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });

            string salt;
            var pass = "ThisIsAnEncryptedPassword";
            var encrypted = provider.EncryptOrHashNewPassword(pass, out salt);
            
            var result = provider.CheckPassword("ThisIsAnEncryptedPassword", encrypted);

            Assert.IsTrue(result);
        }

        [Test]
        public void Check_Password_Clear()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Clear" } });

            var pass = "ThisIsAClearPassword";

            var result = provider.CheckPassword("ThisIsAClearPassword", pass);

            Assert.IsTrue(result);
        }

        [Test]
        public void Can_Decrypt_Password()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });

            string salt;
            var pass = "ThisIsAnEncryptedPassword";
            var encrypted = provider.EncryptOrHashNewPassword(pass, out salt);
            
            var result = provider.DecryptPassword(encrypted);

            Assert.AreEqual(pass, result);

        }

        [Test]
        public void Get_Hash_Algorithm_Legacy()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { {"useLegacyEncoding", "true"}, { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });

            var alg = provider.GetHashAlgorithm("blah");

            Assert.IsTrue(alg is HMACSHA1);
        }

        [Test]
        public void Get_Hash_Algorithm()
        {
            var providerMock = new Mock<MembershipProviderBase>() { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });

            var alg = provider.GetHashAlgorithm("blah");

            Assert.IsTrue(alg is HMACSHA256);
        }


    }
}
