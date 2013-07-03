using System;
using System.Linq;
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
            var membershipUser = userService.CreateMembershipUser("John Doe", "john@umbraco.io", "12345", userType, "john@umbraco.io");

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
            var membershipUser = userService.CreateMembershipUser("John Doe", "john@umbraco.io", encodedPassword, userType, "john@umbraco.io");

            // Assert
            Assert.That(membershipUser.HasIdentity, Is.True);
            Assert.That(membershipUser.Password, Is.Not.EqualTo(password));
            Assert.That(membershipUser.Password, Is.EqualTo(encodedPassword));
            IUser user = membershipUser as User;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Permissions, Is.EqualTo(userType.Permissions));
        }

        [Test]
        public void Can_Remove_Section_From_All_Assigned_Users()
        {            
            var userType = ServiceContext.UserService.GetUserTypeByAlias("admin");
            //we know this actually is an IUser so we'll just cast
            var user1 = (IUser)ServiceContext.UserService.CreateMembershipUser("test1", "test1", "test1", userType, "test1@test.com");
            var user2 = (IUser)ServiceContext.UserService.CreateMembershipUser("test2", "test2", "test2", userType, "test2@test.com");
            
            //adds some allowed sections
            user1.AddAllowedSection("test");
            user2.AddAllowedSection("test");
            ServiceContext.UserService.SaveUser(user1);
            ServiceContext.UserService.SaveUser(user2);

            //now clear the section from all users
            ServiceContext.UserService.DeleteSectionFromAllUsers("test");

            //assert
            var result1 = ServiceContext.UserService.GetUserById((int)user1.Id);
            var result2 = ServiceContext.UserService.GetUserById((int)user2.Id);
            Assert.IsFalse(result1.AllowedSections.Contains("test"));
            Assert.IsFalse(result2.AllowedSections.Contains("test"));

        }
    }
}