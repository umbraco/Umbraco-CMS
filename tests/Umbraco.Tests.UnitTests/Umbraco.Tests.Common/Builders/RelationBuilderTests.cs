// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class RelationBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int parentId = 9;
        const int childId = 8;
        const int id = 4;
        var key = Guid.NewGuid();
        var createDate = DateTime.Now.AddHours(-1);
        var updateDate = DateTime.Now;
        const string comment = "test comment";
        const int relationTypeId = 66;
        const string relationTypeAlias = "test";
        const string relationTypeName = "name";
        var parentObjectType = Guid.NewGuid();
        var childObjectType = Guid.NewGuid();

        var builder = new RelationBuilder();

        // Act
        var relation = builder
            .BetweenIds(parentId, childId)
            .WithId(id)
            .WithComment(comment)
            .WithCreateDate(createDate)
            .WithUpdateDate(updateDate)
            .WithKey(key)
            .AddRelationType()
            .WithId(relationTypeId)
            .WithAlias(relationTypeAlias)
            .WithName(relationTypeName)
            .WithIsBidirectional(false)
            .WithIsDependency(true)
            .WithParentObjectType(parentObjectType)
            .WithChildObjectType(childObjectType)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(parentId, relation.ParentId);
        Assert.AreEqual(childId, relation.ChildId);
        Assert.AreEqual(id, relation.Id);
        Assert.AreEqual(createDate, relation.CreateDate);
        Assert.AreEqual(updateDate, relation.UpdateDate);
        Assert.AreEqual(key, relation.Key);
        Assert.AreEqual(comment, relation.Comment);
        Assert.AreEqual(relationTypeId, relation.RelationType.Id);
        Assert.AreEqual(relationTypeAlias, relation.RelationType.Alias);
        Assert.AreEqual(relationTypeName, relation.RelationType.Name);
        Assert.IsFalse(relation.RelationType.IsBidirectional);

        Assert.IsTrue((relation.RelationType as IRelationTypeWithIsDependency).IsDependency);
        Assert.AreEqual(parentObjectType, relation.RelationType.ParentObjectType);
        Assert.AreEqual(childObjectType, relation.RelationType.ChildObjectType);
    }
}
