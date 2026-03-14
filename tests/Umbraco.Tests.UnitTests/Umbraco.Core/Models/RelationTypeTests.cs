// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

    /// <summary>
    /// Contains unit tests for the <see cref="RelationType"/> model in the Umbraco Core.
    /// </summary>
[TestFixture]
public class RelationTypeTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new RelationTypeBuilder();

    private RelationTypeBuilder _builder;

    /// <summary>
    /// Tests that the DeepClone method creates a deep copy of the RelationType instance.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        IRelationType item = _builder
            .WithId(1)
            .WithParentObjectType(Guid.NewGuid())
            .WithChildObjectType(Guid.NewGuid())
            .Build();

        var clone = (RelationType)item.DeepClone();

        Assert.AreNotSame(clone, item);
        Assert.AreEqual(clone, item);
        Assert.AreEqual(clone.Alias, item.Alias);
        Assert.AreEqual(clone.ChildObjectType, item.ChildObjectType);
        Assert.AreEqual(clone.ParentObjectType, item.ParentObjectType);
        Assert.AreEqual(clone.IsBidirectional, item.IsBidirectional);
        Assert.AreEqual(clone.Id, item.Id);
        Assert.AreEqual(clone.Key, item.Key);
        Assert.AreEqual(clone.Name, item.Name);
        Assert.AreEqual(clone.CreateDate, item.CreateDate);
        Assert.AreEqual(clone.UpdateDate, item.UpdateDate);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }
    }

    /// <summary>
    /// Tests that a RelationType object can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        IRelationType item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item));
    }
}
