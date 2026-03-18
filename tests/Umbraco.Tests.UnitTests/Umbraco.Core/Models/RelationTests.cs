// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Unit tests for the <see cref="Relation"/> model.
/// </summary>
[TestFixture]
public class RelationTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new RelationBuilder();

    private RelationBuilder _builder;

    /// <summary>
    /// Tests that a Relation object can be deep cloned correctly.
    /// Ensures that the clone is a different instance but has equal property values,
    /// including nested objects, verifying deep cloning behavior.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        var relation = BuildRelation();

        var clone = (Relation)relation.DeepClone();

        Assert.AreNotSame(clone, relation);
        Assert.AreEqual(clone, relation);
        Assert.AreEqual(clone.ChildId, relation.ChildId);
        Assert.AreEqual(clone.Comment, relation.Comment);
        Assert.AreEqual(clone.CreateDate, relation.CreateDate);
        Assert.AreEqual(clone.Id, relation.Id);
        Assert.AreEqual(clone.Key, relation.Key);
        Assert.AreEqual(clone.ParentId, relation.ParentId);
        Assert.AreNotSame(clone.RelationType, relation.RelationType);
        Assert.AreEqual(clone.RelationType, relation.RelationType);
        Assert.AreEqual(clone.RelationTypeId, relation.RelationTypeId);
        Assert.AreEqual(clone.UpdateDate, relation.UpdateDate);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(relation, null));
        }
    }

    /// <summary>
    /// Tests that a Relation object can be serialized to JSON without throwing an error.
    /// </summary>
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
