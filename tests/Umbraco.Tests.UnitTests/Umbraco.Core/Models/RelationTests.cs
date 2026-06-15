// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class RelationTests
{
    [SetUp]
    public void SetUp() => _builder = new RelationBuilder();

    private RelationBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var relation = BuildRelation();

        var clone = (Relation)relation.DeepClone();

        Assert.That(relation, Is.Not.SameAs(clone));
        Assert.That(relation, Is.EqualTo(clone));
        Assert.That(relation.ChildId, Is.EqualTo(clone.ChildId));
        Assert.That(relation.Comment, Is.EqualTo(clone.Comment));
        Assert.That(relation.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(relation.Id, Is.EqualTo(clone.Id));
        Assert.That(relation.Key, Is.EqualTo(clone.Key));
        Assert.That(relation.ParentId, Is.EqualTo(clone.ParentId));
        Assert.That(relation.RelationType, Is.Not.SameAs(clone.RelationType));
        Assert.That(relation.RelationType, Is.EqualTo(clone.RelationType));
        Assert.That(relation.RelationTypeId, Is.EqualTo(clone.RelationTypeId));
        Assert.That(relation.UpdateDate, Is.EqualTo(clone.UpdateDate));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(relation, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var relation = BuildRelation();

        var json = JsonSerializer.Serialize(relation);
        Debug.Print(json);
    }

    private Relation BuildRelation() =>
        _builder
            .BetweenIds(9, 8)
            .WithId(4)
            .WithComment("test comment")
            .WithCreateDate(DateTime.UtcNow)
            .WithUpdateDate(DateTime.UtcNow)
            .WithKey(Guid.NewGuid())
            .AddRelationType()
            .WithId(66)
            .WithAlias("test")
            .WithName("Test")
            .WithIsBidirectional(false)
            .WithParentObjectType(Guid.NewGuid())
            .WithChildObjectType(Guid.NewGuid())
            .Done()
            .Build();
}
