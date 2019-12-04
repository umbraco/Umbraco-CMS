using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Security;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class MembershipProviderBaseTests : UmbracoTestBase
    {
        [Test]
        public void ChangePasswordQuestionAndAnswer_Without_RequiresQuestionAndAnswer()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
            providerMock.Setup(@base => @base.RequiresQuestionAndAnswer).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<NotSupportedException>(() => provider.ChangePasswordQuestionAndAnswer("test", "test", "test", "test"));
        }

        [Test]
        public void CreateUser_Not_Whitespace()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) {CallBase = true};
            var provider = providerMock.Object;

            MembershipCreateStatus status;
            var result = provider.CreateUser("", "", "test@test.com", "", "", true, "", out status);

            Assert.IsNull(result);
            Assert.AreEqual(MembershipCreateStatus.InvalidUserName, status);
        }

        [Test]
        public void CreateUser_Invalid_Question()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
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
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
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
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordRetrieval).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<ProviderException>(() => provider.GetPassword("test", "test"));
        }

        [Test]
        public void GetPassword_With_Hashed()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordRetrieval).Returns(true);
            providerMock.Setup(@base => @base.PasswordFormat).Returns(MembershipPasswordFormat.Hashed);
            var provider = providerMock.Object;

            Assert.Throws<ProviderException>(() => provider.GetPassword("test", "test"));
        }

        // FIXME: in v7 this test relies on ApplicationContext.Current being null, which makes little
        // sense, not going to port the weird code in MembershipProviderBase.ResetPassword, so
        // what shall we do?
        [Test]
        [Ignore("makes no sense?")]
        public void ResetPassword_Without_EnablePasswordReset()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
            providerMock.Setup(@base => @base.EnablePasswordReset).Returns(false);
            var provider = providerMock.Object;

            Assert.Throws<NotSupportedException>(() => provider.ResetPassword("test", "test"));
        }

        [Test]
        public void Sets_Defaults()
        {
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
            var provider = providerMock.Object;
            provider.Initialize("test", new NameValueCollection());

            Assert.AreEqual("test", provider.Name);
            Assert.AreEqual(MembershipProviderBase.GetDefaultAppName(TestHelper.GetHostingEnvironment()), provider.ApplicationName);
            Assert.AreEqual(false, provider.EnablePasswordRetrieval);
            Assert.AreEqual(true, provider.EnablePasswordReset);
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
            var providerMock = new Mock<MembershipProviderBase>(TestHelper.GetHostingEnvironment()) { CallBase = true };
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





    }
}
