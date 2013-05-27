using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the UserService
    /// </summary>
    [TestFixture, RequiresSTA]
    public class UserServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void UserService_Can_Persist_New_User()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var userType = userService.GetUserTypeByAlias("admin");

            // Act
            var membershipUser = userService.CreateUser("John Doe", "john@umbraco.io", "12345", userType, "john@umbraco.io");

            // Assert
            Assert.That(membershipUser.HasIdentity, Is.True);
            IUser user = membershipUser as User;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Permissions, Is.EqualTo(userType.Permissions));
        }

        [Test]
        public void UserService_Can_Persist_New_User_With_Hashed_Password()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var userType = userService.GetUserTypeByAlias("admin");

            // Act
            // NOTE: Normally the hash'ing would be handled in the membership provider, so the service just saves the password
            var password = "123456";
            var hash = new HMACSHA1();
            hash.Key = Encoding.Unicode.GetBytes(password);
            var encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
            var membershipUser = userService.CreateUser("John Doe", "john@umbraco.io", encodedPassword, userType, "john@umbraco.io");

            // Assert
            Assert.That(membershipUser.HasIdentity, Is.True);
            Assert.That(membershipUser.Password, Is.Not.EqualTo(password));
            Assert.That(membershipUser.Password, Is.EqualTo(encodedPassword));
            IUser user = membershipUser as User;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Permissions, Is.EqualTo(userType.Permissions));
        }
    }
}