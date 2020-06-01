using Moq;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedUser
    {
        /// <summary>
        /// Returns a <see cref="Mock{IUser}"/> and ensures that the ToUserCache and FromUserCache methods are mapped correctly for
        /// dealing with start node caches
        /// </summary>
        /// <returns></returns>
        internal static Mock<IUser> GetUserMock()
        {
            var userCache = new Dictionary<string, object>();
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.FromUserCache<int[]>(It.IsAny<string>())).Returns((string key) => userCache.TryGetValue(key, out var val) ? val is int[] iVal ? iVal : null : null);
            userMock.Setup(x => x.ToUserCache<int[]>(It.IsAny<string>(), It.IsAny<int[]>())).Callback((string key, int[] val) => userCache[key] = val);
            return userMock;
        }

        internal static User CreateUser(string suffix = "")
        {
            var user = new User(SettingsForTests.GenerateMockGlobalSettings())
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
                var user = new User(SettingsForTests.GenerateMockGlobalSettings(), name, "test" + i + "@test.com", "test" + i, "test" + i);

                onCreating?.Invoke(i, user);

                user.ResetDirtyProperties(false);

                list.Add(user);
            }

            return list;
        }
    }
}
