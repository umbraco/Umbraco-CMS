// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

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
        ISet<string> testPermissions = "abc".Select(x=>x.ToString()).ToHashSet();
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
        Assert.That(userGroup.Id, Is.EqualTo(testId));
        Assert.That(userGroup.Alias, Is.EqualTo(testAlias));
        Assert.That(userGroup.Name, Is.EqualTo(testName));
        Assert.That(userGroup.UserCount, Is.EqualTo(testUserCount));
        Assert.That(userGroup.Icon, Is.EqualTo(testIcon));
        Assert.That(userGroup.Permissions.Count(), Is.EqualTo(testPermissions.Count));
        Assert.That(userGroup.StartContentId, Is.EqualTo(testStartContentId));
        Assert.That(userGroup.StartMediaId, Is.EqualTo(testStartMediaId));
    }
}
