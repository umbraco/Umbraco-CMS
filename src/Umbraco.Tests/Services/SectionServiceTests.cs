﻿using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using System;
using System.Linq;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the SectionService
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class SectionServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void CreateTestData()
        {
            base.CreateTestData();

            ServiceContext.SectionService.MakeNew("Content", "content", "icon-content");
            ServiceContext.SectionService.MakeNew("Media", "media", "icon-media");
            ServiceContext.SectionService.MakeNew("Settings", "settings", "icon-settings");
            ServiceContext.SectionService.MakeNew("Developer", "developer", "icon-developer");
        }

        [Test]
        public void SectionService_Can_Get_Allowed_Sections_For_User()
        {
            // Arrange
            var user = CreateUser();

            // Act
            var result = ServiceContext.SectionService.GetAllowedSections(user.Id).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void SectionService_Can_Get_Allowed_Sections_For_User_With_Groups()
        {
            // Arrange
            var user = CreateUser(true);

            // Act
            var result = ServiceContext.SectionService.GetAllowedSections(user.Id).ToList();

            // Assert
            Assert.AreEqual(4, result.Count);
        }

        private IUser CreateUser(bool withGroups = false)
        {
            var userType = new UserType
            {
                Alias = "TypeA",
                Name = "Type A",
            };
            ServiceContext.UserService.SaveUserType(userType, false);

            var user = new User(userType)
            {
                Name = "Test user",
                Username = "testUser",
                Email = "testuser@test.com",
            };
            user.AddAllowedSection("content");
            user.AddAllowedSection("media");
            ServiceContext.UserService.Save(user, false);

            if (withGroups)
            {
                var userGroupA = new UserGroup
                {
                    Alias = "GroupA",
                    Name = "Group A"
                };
                userGroupA.AddAllowedSection("media");
                userGroupA.AddAllowedSection("settings");
                ServiceContext.UserService.SaveUserGroup(userGroupA, true, new[] {user.Id}, false);

                var userGroupB = new UserGroup
                {
                    Alias = "GroupB",
                    Name = "Group B"
                };
                userGroupB.AddAllowedSection("settings");
                userGroupB.AddAllowedSection("developer");
                ServiceContext.UserService.SaveUserGroup(userGroupB, true, new[] {user.Id}, false);
            }

            return ServiceContext.UserService.GetUserById(user.Id);
        }
    }
}
