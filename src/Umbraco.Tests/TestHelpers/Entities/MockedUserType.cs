using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUserType
    {
        internal static UserType CreateUserType(string suffix = "")
        {
            return new UserType()
                {
                    Alias = "testUserType" + suffix,
                    Name = "TestUserType" + suffix,
                    Permissions = new[]{"A", "B", "C"}
                };
        }
    }
}