using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
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
                    RawPasswordValue = "testing",
                    IsLockedOut = false,
                    DefaultPermissions = new[]{"A", "B", "C"},
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

        internal static IEnumerable<IUser> CreateUser(IUserType userType, int amount, Action<int, IUser> onCreating = null)
        {
            var list = new List<IUser>();

            for (int i = 0; i < amount; i++)
            {
                var name = "Member No-" + i;
                var user = new User(name, "test" + i + "@test.com", "test" + i, "test" + i, userType);
                
                if (onCreating != null)
                {
                    onCreating(i, user);
                }

                user.ResetDirtyProperties(false);

                list.Add(user);
            }

            return list;
        }
    }
}