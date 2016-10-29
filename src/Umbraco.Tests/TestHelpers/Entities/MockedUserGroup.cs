using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUserGroup
    {
        internal static UserGroup CreateUserGroup(string suffix = "", string[] permissions = null, string[] allowedSections = null)
        {
            var group = new UserGroup
            {
                Alias = "testUserGroup" + suffix,
                Name = "TestUserGroup" + suffix,
                Permissions = permissions ?? new[] { "A", "B", "C" }
            };

            if (allowedSections == null)
            {
                group.AddAllowedSection("content");
                group.AddAllowedSection("media");
            }
            else
            {
                foreach (var allowedSection in allowedSections)
                {
                    group.AddAllowedSection(allowedSection);
                }
            }

            return group;
        }
    }
}