using NUnit.Framework;
using System.Linq;
using System.Threading;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the SectionService
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
    public class SectionServiceTests : TestWithSomeContentBase
    {
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
            var user = CreateTestUser();

            // Act
            var result = ServiceContext.SectionService.GetAllowedSections(user.Id).ToList();

            // Assert
            Assert.AreEqual(3, result.Count);
        }

        private IUser CreateTestUser()
        {
            var user = new User
            {
                Name = "Test user",
                Username = "testUser",
                Email = "testuser@test.com",
            };
            ServiceContext.UserService.Save(user, false);

            var userGroupA = new UserGroup
            {
                Alias = "GroupA",
                Name = "Group A"
            };
            userGroupA.AddAllowedSection("media");
            userGroupA.AddAllowedSection("settings");
            //TODO: This is failing the test
            ServiceContext.UserService.Save(userGroupA, new[] { user.Id }, false);

            var userGroupB = new UserGroup
            {
                Alias = "GroupB",
                Name = "Group B"
            };
            userGroupB.AddAllowedSection("settings");
            userGroupB.AddAllowedSection("developer");
            ServiceContext.UserService.Save(userGroupB, new[] { user.Id }, false);

            return ServiceContext.UserService.GetUserById(user.Id);
        }
    }
}
