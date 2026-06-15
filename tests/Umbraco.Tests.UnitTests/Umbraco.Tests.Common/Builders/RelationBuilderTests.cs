// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        var createDate = DateTime.UtcNow.AddHours(-1);
        var updateDate = DateTime.UtcNow;
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
        Assert.That(relation.ParentId, Is.EqualTo(parentId));
        Assert.That(relation.ChildId, Is.EqualTo(childId));
        Assert.That(relation.Id, Is.EqualTo(id));
        Assert.That(relation.CreateDate, Is.EqualTo(createDate));
        Assert.That(relation.UpdateDate, Is.EqualTo(updateDate));
        Assert.That(relation.Key, Is.EqualTo(key));
        Assert.That(relation.Comment, Is.EqualTo(comment));
        Assert.That(relation.RelationType.Id, Is.EqualTo(relationTypeId));
        Assert.That(relation.RelationType.Alias, Is.EqualTo(relationTypeAlias));
        Assert.That(relation.RelationType.Name, Is.EqualTo(relationTypeName));
        Assert.That(relation.RelationType.IsBidirectional, Is.False);

        Assert.That((relation.RelationType as IRelationTypeWithIsDependency).IsDependency, Is.True);
        Assert.That(relation.RelationType.ParentObjectType, Is.EqualTo(parentObjectType));
        Assert.That(relation.RelationType.ChildObjectType, Is.EqualTo(childObjectType));
    }
}
