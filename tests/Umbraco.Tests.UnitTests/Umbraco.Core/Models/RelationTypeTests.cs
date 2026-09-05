// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class RelationTypeTests
{
    [SetUp]
    public void SetUp() => _builder = new RelationTypeBuilder();

    private RelationTypeBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        IRelationType item = _builder
            .WithId(1)
            .WithParentObjectType(Guid.NewGuid())
            .WithChildObjectType(Guid.NewGuid())
            .Build();

        var clone = (RelationType)item.DeepClone();

        Assert.That(item, Is.Not.SameAs(clone));
        Assert.That(item, Is.EqualTo(clone));
        Assert.That(item.Alias, Is.EqualTo(clone.Alias));
        Assert.That(item.ChildObjectType, Is.EqualTo(clone.ChildObjectType));
        Assert.That(item.ParentObjectType, Is.EqualTo(clone.ParentObjectType));
        Assert.That(item.IsBidirectional, Is.EqualTo(clone.IsBidirectional));
        Assert.That(item.Id, Is.EqualTo(clone.Id));
        Assert.That(item.Key, Is.EqualTo(clone.Key));
        Assert.That(item.Name, Is.EqualTo(clone.Name));
        Assert.That(item.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(item.UpdateDate, Is.EqualTo(clone.UpdateDate));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(item, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        IRelationType item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item));
    }
}
