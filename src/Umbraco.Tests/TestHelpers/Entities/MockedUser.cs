using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUser
    {
        internal static User CreateUser(IUserType userType = null, string suffix = "")
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
                    Permissions = "ABC",
                    StartContentId = -1,
                    StartMediaId = -1,
                    DefaultToLiveEditing = false,
                    Email = "test" + suffix + "@test.com",
                    Username = "TestUser" + suffix
                };

            user.AddAllowedSection("content");
            user.AddAllowedSection("media");

            return user;
        }
    }
}