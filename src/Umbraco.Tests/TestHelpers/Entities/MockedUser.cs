using System.Linq;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUser
    {
        internal static User CreateUser(IUserType userType = null, string suffix = "", params string[] allowedSections)
        {
            if (userType == null)
            {
                userType = MockedUserType.CreateUserType();
            }

            var user = new User(userType)
                {
                    Language = "en",
                    IsApproved = true,
                    Name = "TestUser" + suffix,
                    Password = "testing",
                    NoConsole = false,
                    DefaultPermissions = "ABC",
                    StartContentId = -1,
                    StartMediaId = -1,
                    Email = "test" + suffix + "@test.com",
                    Username = "TestUser" + suffix
                };

            if (allowedSections.Any())
            {
                foreach (var s in allowedSections)
                {
                    user.AddAllowedSection(s);
                }
            }
            else
            {
                user.AddAllowedSection("content");
                user.AddAllowedSection("media");    
            }

            return user;
        }
    }
}