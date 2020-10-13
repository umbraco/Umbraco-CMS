﻿using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Umbraco.Web.Services;

namespace Umbraco.Tests.Integration.Services
{
    /// <summary>
    /// Tests covering the SectionService
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class SectionServiceTests : UmbracoIntegrationTest
    {
        private ISectionService SectionService => GetRequiredService<ISectionService>();
        private IUserService UserService => GetRequiredService<IUserService>();

        [Test]
        public void SectionService_Can_Get_Allowed_Sections_For_User()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var result = SectionService.GetAllowedSections(user.Id).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        private IUser CreateTestUser()
        {
            var globalSettings = new GlobalSettings();
            var user = new User(globalSettings)
            {
                Name = "Test user",
                Username = "testUser",
                Email = "testuser@test.com",
            };
            UserService.Save(user, false);

            var userGroupA = new UserGroup(ShortStringHelper)
            {
                Alias = "GroupA",
                Name = "Group A"
            };
            userGroupA.AddAllowedSection("media");
            userGroupA.AddAllowedSection("settings");
            // TODO: This is failing the test
            UserService.Save(userGroupA, new[] { user.Id }, false);

            var userGroupB = new UserGroup(ShortStringHelper)
            {
                Alias = "GroupB",
                Name = "Group B"
            };
            userGroupB.AddAllowedSection("settings");
            userGroupB.AddAllowedSection("member");
            UserService.Save(userGroupB, new[] { user.Id }, false);

            return UserService.GetUserById(user.Id);
        }
    }
}
