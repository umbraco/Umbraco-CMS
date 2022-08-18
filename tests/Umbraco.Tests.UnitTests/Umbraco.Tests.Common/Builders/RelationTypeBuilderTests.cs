// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
        var createDate = DateTime.Now.AddHours(-2);
        var updateDate = DateTime.Now.AddHours(-1);
        var deleteDate = DateTime.Now;
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
        Assert.AreEqual(id, relationType.Id);
        Assert.AreEqual(alias, relationType.Alias);
        Assert.AreEqual(name, relationType.Name);
        Assert.AreEqual(key, relationType.Key);
        Assert.AreEqual(createDate, relationType.CreateDate);
        Assert.AreEqual(updateDate, relationType.UpdateDate);
        Assert.AreEqual(deleteDate, relationType.DeleteDate);
        Assert.AreEqual(parentObjectType, relationType.ParentObjectType);
        Assert.AreEqual(childObjectType, relationType.ChildObjectType);
        Assert.AreEqual(isBidirectional, relationType.IsBidirectional);
        Assert.AreEqual(isDependency, relationType.IsDependency);
    }
}
