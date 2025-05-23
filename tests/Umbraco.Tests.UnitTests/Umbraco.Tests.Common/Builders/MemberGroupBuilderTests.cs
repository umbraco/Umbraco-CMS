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
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;

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
        Assert.AreEqual(testId, group.Id);
        Assert.AreEqual(testKey, group.Key);
        Assert.AreEqual(testName, group.Name);
        Assert.AreEqual(testCreateDate, group.CreateDate);
        Assert.AreEqual(testUpdateDate, group.UpdateDate);
        Assert.AreEqual(testCreatorId, group.CreatorId);
    }
}
