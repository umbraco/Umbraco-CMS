using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUserGroup
    {
        internal static UserGroup CreateUserGroup(string suffix = "", string[] permissions = null)
        {
            return new UserGroup()
                {
                    Alias = "testUserGroup" + suffix,
                    Name = "TestUserGroup" + suffix,
                    Permissions = permissions ?? new[]{"A", "B", "C"}
                };
        }
    }
}