﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the UserService
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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
        public void UserService_Get_User_Permissions_For_Unassigned_Permission_Nodes()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var user = CreateTestUser();

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            var content = new[]
                {
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType)
                };
            ServiceContext.ContentService.Save(content);

            // Act
            var permissions = userService.GetPermissions(user, content.ElementAt(0).Id, content.ElementAt(1).Id, content.ElementAt(2).Id);

            //assert
            Assert.AreEqual(3, permissions.Count());
            Assert.AreEqual(17, permissions.ElementAt(0).AssignedPermissions.Count());
            Assert.AreEqual(17, permissions.ElementAt(1).AssignedPermissions.Count());
            Assert.AreEqual(17, permissions.ElementAt(2).AssignedPermissions.Count());
        }

        [Test]
        public void UserService_Get_User_Permissions_For_Assigned_Permission_Nodes()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var user = CreateTestUser();

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            var content = new[]
                {
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType)
                };
            ServiceContext.ContentService.Save(content);
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionBrowse.Instance.Letter, new int[] { 1 });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionDelete.Instance.Letter, new int[] { 1 });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionMove.Instance.Letter, new int[] { 1 });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionBrowse.Instance.Letter, new int[] { 1 });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionDelete.Instance.Letter, new int[] { 1 });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(2), ActionBrowse.Instance.Letter, new int[] { 1 });

            // Act
            var permissions = userService.GetPermissions(user, content.ElementAt(0).Id, content.ElementAt(1).Id, content.ElementAt(2).Id);

            //assert
            Assert.AreEqual(3, permissions.Count());
            Assert.AreEqual(3, permissions.ElementAt(0).AssignedPermissions.Count());
            Assert.AreEqual(2, permissions.ElementAt(1).AssignedPermissions.Count());
            Assert.AreEqual(1, permissions.ElementAt(2).AssignedPermissions.Count());
        }

        [Test]
        public void UserService_Get_UserGroup_Assigned_Permissions()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var userGroup = CreateTestUserGroup();

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            var content = new[]
                {
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType)
                };
            ServiceContext.ContentService.Save(content);
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionBrowse.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionDelete.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionMove.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionBrowse.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionDelete.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(2), ActionBrowse.Instance.Letter, new int[] { userGroup.Id });

            // Act
            var permissions = userService.GetPermissions(userGroup, true, content.ElementAt(0).Id, content.ElementAt(1).Id, content.ElementAt(2).Id);

            //assert
            Assert.AreEqual(3, permissions.Count());
            Assert.AreEqual(3, permissions.ElementAt(0).AssignedPermissions.Count());
            Assert.AreEqual(2, permissions.ElementAt(1).AssignedPermissions.Count());
            Assert.AreEqual(1, permissions.ElementAt(2).AssignedPermissions.Count());
        }

        [Test]
        public void UserService_Get_UserGroup_Assigned_And_Default_Permissions()
        {
            // Arrange
            var userService = ServiceContext.UserService;
            var userGroup = CreateTestUserGroup();

            var contentType = MockedContentTypes.CreateSimpleContentType();
            ServiceContext.ContentTypeService.Save(contentType);
            var content = new[]
                {
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType),
                    MockedContent.CreateSimpleContent(contentType)
                };
            ServiceContext.ContentService.Save(content);
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionBrowse.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionDelete.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(0), ActionMove.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionBrowse.Instance.Letter, new int[] { userGroup.Id });
            ServiceContext.ContentService.AssignContentPermission(content.ElementAt(1), ActionDelete.Instance.Letter, new int[] { userGroup.Id });

            // Act
            var permissions = userService.GetPermissions(userGroup, false, content.ElementAt(0).Id, content.ElementAt(1).Id, content.ElementAt(2).Id);

            //assert
            Assert.AreEqual(3, permissions.Count());
            Assert.AreEqual(3, permissions.ElementAt(0).AssignedPermissions.Count());
            Assert.AreEqual(2, permissions.ElementAt(1).AssignedPermissions.Count());
            Assert.AreEqual(17, permissions.ElementAt(2).AssignedPermissions.Count());
        }

        [Test]
        public void Can_Delete_User()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");
            
            ServiceContext.UserService.Delete(user, true);
            var deleted = ServiceContext.UserService.GetUserById(user.Id);

            // Assert
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Disables_User_Instead_Of_Deleting_If_Flag_Not_Set()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

            ServiceContext.UserService.Delete(user);
            var deleted = ServiceContext.UserService.GetUserById(user.Id);

            // Assert
            Assert.That(deleted, Is.Not.Null);
        }

        [Test]
        public void Exists_By_Username()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");
            var user2 = ServiceContext.UserService.CreateUserWithIdentity("john2@umbraco.io", "john2@umbraco.io");
            Assert.IsTrue(ServiceContext.UserService.Exists("JohnDoe"));
            Assert.IsFalse(ServiceContext.UserService.Exists("notFound"));
            Assert.IsTrue(ServiceContext.UserService.Exists("john2@umbraco.io"));
        }

        [Test]
        public void Get_By_Email()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

            Assert.IsNotNull(ServiceContext.UserService.GetByEmail(user.Email));
            Assert.IsNull(ServiceContext.UserService.GetByEmail("do@not.find"));
        }

        [Test]
        public void Get_By_Username()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

            Assert.IsNotNull(ServiceContext.UserService.GetByUsername(user.Username));
            Assert.IsNull(ServiceContext.UserService.GetByUsername("notFound"));
        }

        [Test]
        public void Get_By_Username_With_Backslash()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("mydomain\\JohnDoe", "john@umbraco.io");

            Assert.IsNotNull(ServiceContext.UserService.GetByUsername(user.Username));
            Assert.IsNull(ServiceContext.UserService.GetByUsername("notFound"));
        }

        [Test]
        public void Get_By_Object_Id()
        {
            var user = ServiceContext.UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

            Assert.IsNotNull(ServiceContext.UserService.GetUserById(user.Id));
            Assert.IsNull(ServiceContext.UserService.GetUserById(9876));
        }

        [Test]
        public void Find_By_Email_Starts_With()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);
            //don't find this
            var customUser = MockedUser.CreateUser();
            customUser.Email = "hello@hello.com";
            ServiceContext.UserService.Save(customUser);

            int totalRecs;
            var found = ServiceContext.UserService.FindByEmail("tes", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Find_By_Email_Ends_With()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);
            //include this
            var customUser = MockedUser.CreateUser();
            customUser.Email = "hello@test.com";
            ServiceContext.UserService.Save(customUser);

            int totalRecs;
            var found = ServiceContext.UserService.FindByEmail("test.com", 0, 100, out totalRecs, StringPropertyMatchType.EndsWith);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Contains()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);
            //include this
            var customUser = MockedUser.CreateUser();
            customUser.Email = "hello@test.com";
            ServiceContext.UserService.Save(customUser);

            int totalRecs;
            var found = ServiceContext.UserService.FindByEmail("test", 0, 100, out totalRecs, StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Exact()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);
            //include this
            var customUser = MockedUser.CreateUser();
            customUser.Email = "hello@test.com";
            ServiceContext.UserService.Save(customUser);

            int totalRecs;
            var found = ServiceContext.UserService.FindByEmail("hello@test.com", 0, 100, out totalRecs, StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_All_Paged_Users()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);

            int totalRecs;
            var found = ServiceContext.UserService.GetAll(0, 2, out totalRecs);

            Assert.AreEqual(2, found.Count());
            // + 1 because of the built in admin user
            Assert.AreEqual(11, totalRecs);
            Assert.AreEqual("admin", found.First().Username);
            Assert.AreEqual("test0", found.Last().Username);
        }

        [Test]
        public void Count_All_Users()
        {
            var users = MockedUser.CreateMulipleUsers(10);
            ServiceContext.UserService.Save(users);
            var customUser = MockedUser.CreateUser();
            ServiceContext.UserService.Save(customUser);

            var found = ServiceContext.UserService.GetCount(MemberCountType.All);

            // + 1 because of the built in admin user
            Assert.AreEqual(12, found);
        }

        [Ignore]
        [Test]
        public void Count_All_Online_Users()
        {
            var users = MockedUser.CreateMulipleUsers(10, (i, member) => member.LastLoginDate = DateTime.Now.AddMinutes(i * -2));
            ServiceContext.UserService.Save(users);

            var customUser = MockedUser.CreateUser();
            throw new NotImplementedException();
        }

        [Test]
        public void Count_All_Locked_Users()
        {
            var users = MockedUser.CreateMulipleUsers(10, (i, member) => member.IsLockedOut = i % 2 == 0);
            ServiceContext.UserService.Save(users);

            var customUser = MockedUser.CreateUser();
            customUser.IsLockedOut = true;
            ServiceContext.UserService.Save(customUser);

            var found = ServiceContext.UserService.GetCount(MemberCountType.LockedOut);

            Assert.AreEqual(6, found);
        }

        [Test]
        public void Count_All_Approved_Users()
        {
            var users = MockedUser.CreateMulipleUsers(10, (i, member) => member.IsApproved = i % 2 == 0);
            ServiceContext.UserService.Save(users);

            var customUser = MockedUser.CreateUser();
            customUser.IsApproved = false;
            ServiceContext.UserService.Save(customUser);

            var found = ServiceContext.UserService.GetCount(MemberCountType.Approved);

            // + 1 because of the built in admin user
            Assert.AreEqual(6, found);
        }

        [Test]
        public void Can_Persist_New_User()
        {
            // Arrange
            var userService = ServiceContext.UserService;

            // Act
            var membershipUser = userService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

            // Assert
            Assert.That(membershipUser.HasIdentity, Is.True);
            Assert.That(membershipUser.Id, Is.GreaterThan(0));
            IUser user = membershipUser as User;
            Assert.That(user, Is.Not.Null);
        }

        [Test]
        public void Can_Persist_New_User_With_Hashed_Password()
        {
            // Arrange
            var userService = ServiceContext.UserService;

            // Act
            // NOTE: Normally the hash'ing would be handled in the membership provider, so the service just saves the password
            var password = "123456";
            var hash = new HMACSHA1();
            hash.Key = Encoding.Unicode.GetBytes(password);
            var encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
            var membershipUser = new User("JohnDoe", "john@umbraco.io", encodedPassword, encodedPassword);
            userService.Save(membershipUser);

            // Assert
            Assert.That(membershipUser.HasIdentity, Is.True);
            Assert.That(membershipUser.RawPasswordValue, Is.Not.EqualTo(password));
            Assert.That(membershipUser.RawPasswordValue, Is.EqualTo(encodedPassword));
            IUser user = membershipUser as User;
            Assert.That(user, Is.Not.Null);
        }

        [Test]
        public void Can_Add_And_Remove_Sections_From_UserGroup()
        {
            var userGroup = new UserGroup
            {
                Id = 1,
                Alias = "Group1",
                Name = "Group 1"
            };
            userGroup.AddAllowedSection("content");
            userGroup.AddAllowedSection("mediat");
            ServiceContext.UserService.SaveUserGroup(userGroup);

            var result1 = ServiceContext.UserService.GetUserGroupById(userGroup.Id);

            Assert.AreEqual(2, result1.AllowedSections.Count());

            //adds some allowed sections
            userGroup.AddAllowedSection("test1");
            userGroup.AddAllowedSection("test2");
            userGroup.AddAllowedSection("test3");
            userGroup.AddAllowedSection("test4");
            ServiceContext.UserService.SaveUserGroup(userGroup);

            result1 = ServiceContext.UserService.GetUserGroupById(userGroup.Id);

            Assert.AreEqual(6, result1.AllowedSections.Count());

            //simulate clearing the sections
            foreach (var s in userGroup.AllowedSections)
            {
                result1.RemoveAllowedSection(s);
            }

            //now just re-add a couple
            result1.AddAllowedSection("test3");
            result1.AddAllowedSection("test4");
            ServiceContext.UserService.SaveUserGroup(result1);

            //assert
            //re-get
            result1 = ServiceContext.UserService.GetUserGroupById(userGroup.Id);
            Assert.AreEqual(2, result1.AllowedSections.Count());
        }

        [Test]
        public void Can_Remove_Section_From_All_Assigned_UserGroups()
        {
            var userGroup1 = new UserGroup
            {
                Id = 1,
                Alias = "Group1",
                Name = "Group 2"
            };
            var userGroup2 = new UserGroup
            {
                Id = 2,
                Alias = "Group2",
                Name = "Group 2"
            };
            ServiceContext.UserService.SaveUserGroup(userGroup1);
            ServiceContext.UserService.SaveUserGroup(userGroup2);

            //adds some allowed sections
            userGroup1.AddAllowedSection("test");
            userGroup2.AddAllowedSection("test");
            ServiceContext.UserService.SaveUserGroup(userGroup1);
            ServiceContext.UserService.SaveUserGroup(userGroup2);

            //now clear the section from all users
            ServiceContext.UserService.DeleteSectionFromAllUserGroups("test");

            //assert
            var result1 = ServiceContext.UserService.GetUserGroupById(userGroup1.Id);
            var result2 = ServiceContext.UserService.GetUserGroupById(userGroup2.Id);
            Assert.IsFalse(result1.AllowedSections.Contains("test"));
            Assert.IsFalse(result2.AllowedSections.Contains("test"));
        }

        [Test]
        public void Can_Add_Section_To_All_UserGroups()
        {
            var userGroup1 = new UserGroup
            {
                Id = 1,
                Alias = "Group1",
                Name = "Group 1"
            };
            var userGroup2 = new UserGroup
            {
                Id = 2,
                Alias = "Group2",
                Name = "Group 2"
            };
            var userGroup3 = new UserGroup
            {
                Id = 3,
                Alias = "Group3",
                Name = "Group 3"
            };
            ServiceContext.UserService.SaveUserGroup(userGroup1);
            ServiceContext.UserService.SaveUserGroup(userGroup2);
            ServiceContext.UserService.SaveUserGroup(userGroup3);

            //now add the section to specific groups
            ServiceContext.UserService.AddSectionToAllUserGroups("test", userGroup1.Id, userGroup2.Id);

            //assert
            var result1 = ServiceContext.UserService.GetUserGroupById(userGroup1.Id);
            var result2 = ServiceContext.UserService.GetUserGroupById(userGroup2.Id);
            var result3 = ServiceContext.UserService.GetUserGroupById(userGroup3.Id);
            Assert.IsTrue(result1.AllowedSections.Contains("test"));
            Assert.IsTrue(result2.AllowedSections.Contains("test"));
            Assert.IsFalse(result3.AllowedSections.Contains("test"));

            //now add the section to all groups
            ServiceContext.UserService.AddSectionToAllUserGroups("test");

            //assert
            result1 = ServiceContext.UserService.GetUserGroupById(userGroup1.Id);
            result2 = ServiceContext.UserService.GetUserGroupById(userGroup2.Id);
            result3 = ServiceContext.UserService.GetUserGroupById(userGroup3.Id);
            Assert.IsTrue(result1.AllowedSections.Contains("test"));
            Assert.IsTrue(result2.AllowedSections.Contains("test"));
            Assert.IsTrue(result3.AllowedSections.Contains("test"));
        }

        [Test]
        public void Get_By_Profile_Username()
        {
            // Arrange
            var user = ServiceContext.UserService.CreateUserWithIdentity("test1", "test1@test.com");

            // Act

            var profile = ServiceContext.UserService.GetProfileByUserName(user.Username);

            // Assert
            Assert.IsNotNull(profile);
            Assert.AreEqual(user.Username, profile.Name);
            Assert.AreEqual(user.Id, profile.Id);
        }

        [Test]
        public void Get_By_Profile_Id()
        {
            // Arrange
            var user = (IUser)ServiceContext.UserService.CreateUserWithIdentity("test1", "test1@test.com");

            // Act

            var profile = ServiceContext.UserService.GetProfileById((int)user.Id);

            // Assert
            Assert.IsNotNull(profile);
            Assert.AreEqual(user.Username, profile.Name);
            Assert.AreEqual(user.Id, profile.Id);
        }

        [Test]
        public void Get_User_By_Username()
        {
            // Arrange
            var originalUser = CreateTestUser();

            // Act

            var updatedItem = (User)ServiceContext.UserService.GetByUsername(originalUser.Username);

            // Assert
            Assert.IsNotNull(updatedItem);
            Assert.That(updatedItem.Id, Is.EqualTo(originalUser.Id));
            Assert.That(updatedItem.Name, Is.EqualTo(originalUser.Name));
            Assert.That(updatedItem.Language, Is.EqualTo(originalUser.Language));
            Assert.That(updatedItem.IsApproved, Is.EqualTo(originalUser.IsApproved));
            Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(originalUser.RawPasswordValue));
            Assert.That(updatedItem.IsLockedOut, Is.EqualTo(originalUser.IsLockedOut));
            Assert.That(updatedItem.StartContentId, Is.EqualTo(originalUser.StartContentId));
            Assert.That(updatedItem.StartMediaId, Is.EqualTo(originalUser.StartMediaId));
            Assert.That(updatedItem.Email, Is.EqualTo(originalUser.Email));
            Assert.That(updatedItem.Username, Is.EqualTo(originalUser.Username));
            Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(2));
        }

        private IUser CreateTestUser()
        {
            var userGroup = CreateTestUserGroup();

            var user = ServiceContext.UserService.CreateUserWithIdentity("test1", "test1@test.com");
            user.AddGroup(userGroup);
            user.SetGroupsLoaded();
            ServiceContext.UserService.Save(user);
            return user;
        }

        private UserGroup CreateTestUserGroup()
        {
            var userGroup = new UserGroup
            {
                Id = 1,
                Alias = "testGroup",
                Name = "Test Group",
                Permissions = "ABCDEFGHIJ1234567".ToCharArray().Select(x => x.ToString())
            };
            ServiceContext.UserService.SaveUserGroup(userGroup);
            ServiceContext.UserService.AddSectionToAllUserGroups("content", 1);
            ServiceContext.UserService.AddSectionToAllUserGroups("media", 1);
            return userGroup;
        }
    }
}
