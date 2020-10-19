using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class UserGroupBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const int testId = 10;
            const string testAlias = "test";
            const string testName = "Test";
            const int testUserCount = 11;
            const string testIcon = "icon";
            const string testPermissions = "abc";
            const int testStartContentId = 3;
            const int testStartMediaId = 8;

            var builder = new UserGroupBuilder();

            // Act
            var userGroup = builder                
                .WithId(testId)
                .WithAlias(testAlias)
                .WithName(testName)
                .WithUserCount(testUserCount)
                .WithIcon(testIcon)
                .WithPermissions(testPermissions)
                .WithStartContentId(testStartContentId)
                .WithStartMediaId(testStartMediaId)
                .Build();

            // Assert
            Assert.AreEqual(testId, userGroup.Id);
            Assert.AreEqual(testAlias, userGroup.Alias);
            Assert.AreEqual(testName, userGroup.Name);
            Assert.AreEqual(testUserCount, userGroup.UserCount);
            Assert.AreEqual(testIcon, userGroup.Icon);
            Assert.AreEqual(testPermissions.Length, userGroup.Permissions.Count());
            Assert.AreEqual(testStartContentId, userGroup.StartContentId);
            Assert.AreEqual(testStartMediaId, userGroup.StartMediaId);
        }
    }
}
