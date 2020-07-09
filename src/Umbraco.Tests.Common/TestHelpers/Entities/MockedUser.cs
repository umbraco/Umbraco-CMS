using Moq;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.Common.TestHelpers.Entities
{
    public static class MockedUser
    {
        /// <summary>
        /// Returns a <see cref="Mock{IUser}"/> and ensures that the ToUserCache and FromUserCache methods are mapped correctly for
        /// dealing with start node caches
        /// </summary>
        /// <returns></returns>
        public static Mock<IUser> GetUserMock()
        {
            var userCache = new Dictionary<string, object>();
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.FromUserCache<int[]>(It.IsAny<string>())).Returns((string key) => userCache.TryGetValue(key, out var val) ? val is int[] iVal ? iVal : null : null);
            userMock.Setup(x => x.ToUserCache<int[]>(It.IsAny<string>(), It.IsAny<int[]>())).Callback((string key, int[] val) => userCache[key] = val);
            return userMock;
        }

    }
}
