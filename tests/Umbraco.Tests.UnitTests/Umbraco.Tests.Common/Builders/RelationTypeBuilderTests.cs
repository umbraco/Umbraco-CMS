// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class RelationTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int id = 3;
        const string alias = "test";
        const string name = "Test";
        var key = Guid.NewGuid();
        var createDate = DateTime.UtcNow.AddHours(-2);
        var updateDate = DateTime.UtcNow.AddHours(-1);
        var deleteDate = DateTime.UtcNow;
        var parentObjectType = Guid.NewGuid();
        var childObjectType = Guid.NewGuid();
        const bool isBidirectional = true;
        const bool isDependency = true;

        var builder = new RelationTypeBuilder();

        // Act
        var relationType = builder
            .WithId(id)
            .WithAlias(alias)
            .WithName(name)
            .WithKey(key)
            .WithCreateDate(createDate)
            .WithUpdateDate(updateDate)
            .WithDeleteDate(deleteDate)
            .WithParentObjectType(parentObjectType)
            .WithChildObjectType(childObjectType)
            .WithIsBidirectional(isBidirectional)
            .WithIsDependency(isDependency)
            .Build();

        // Assert
        Assert.That(relationType.Id, Is.EqualTo(id));
        Assert.That(relationType.Alias, Is.EqualTo(alias));
        Assert.That(relationType.Name, Is.EqualTo(name));
        Assert.That(relationType.Key, Is.EqualTo(key));
        Assert.That(relationType.CreateDate, Is.EqualTo(createDate));
        Assert.That(relationType.UpdateDate, Is.EqualTo(updateDate));
        Assert.That(relationType.DeleteDate, Is.EqualTo(deleteDate));
        Assert.That(relationType.ParentObjectType, Is.EqualTo(parentObjectType));
        Assert.That(relationType.ChildObjectType, Is.EqualTo(childObjectType));
        Assert.That(relationType.IsBidirectional, Is.EqualTo(isBidirectional));
        Assert.That(relationType.IsDependency, Is.EqualTo(isDependency));
    }
}
