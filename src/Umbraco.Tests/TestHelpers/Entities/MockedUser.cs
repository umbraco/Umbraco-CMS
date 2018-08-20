using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUser
    {
        internal static User CreateUser(string suffix = "")
        {
            var user = new User
                {
                    Language = "en",
                    IsApproved = true,
                    Name = "TestUser" + suffix,
                    RawPasswordValue = "testing",
                    IsLockedOut = false,
                    Email = "test" + suffix + "@test.com",
                    Username = "TestUser" + suffix
                };

            return user;
        }

        internal static IEnumerable<IUser> CreateMulipleUsers(int amount, Action<int, IUser> onCreating = null)
        {
            var list = new List<IUser>();

            for (int i = 0; i < amount; i++)
            {
                var name = "Member No-" + i;
                var user = new User(name, "test" + i + "@test.com", "test" + i, "test" + i);
                
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