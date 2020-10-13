using Umbraco.Core.Models.Membership;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUserGroup
    {
        public static IShortStringHelper ShortStringHelper { get; } =
            new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        public static UserGroup CreateUserGroup(string suffix = "", string[] permissions = null, string[] allowedSections = null)
        {
            var group = new UserGroup(ShortStringHelper)
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
