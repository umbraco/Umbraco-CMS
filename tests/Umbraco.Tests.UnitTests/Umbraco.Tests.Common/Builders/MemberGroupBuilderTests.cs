// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MemberGroupBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 6;
        const string testName = "Test Group";
        const int testCreatorId = 4;
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;

        var builder = new MemberGroupBuilder();

        // Act
        var group = builder
            .WithId(testId)
            .WithKey(testKey)
            .WithName(testName)
            .WithCreatorId(testCreatorId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .Build();

        // Assert
        Assert.That(group.Id, Is.EqualTo(testId));
        Assert.That(group.Key, Is.EqualTo(testKey));
        Assert.That(group.Name, Is.EqualTo(testName));
        Assert.That(group.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(group.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(group.CreatorId, Is.EqualTo(testCreatorId));
    }
}
