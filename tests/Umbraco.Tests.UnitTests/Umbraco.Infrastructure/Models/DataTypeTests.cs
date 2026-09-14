// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Models;

[TestFixture]
public class DataTypeTests
{
    [SetUp]
    public void SetUp() => _builder = new DataTypeBuilder();

    private DataTypeBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var dtd = _builder
            .WithId(3123)
            .Build();

        var clone = (DataType)dtd.DeepClone();

        Assert.That(dtd, Is.Not.SameAs(clone));
        Assert.That(dtd, Is.EqualTo(clone));
        Assert.That(dtd.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(dtd.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(dtd.DatabaseType, Is.EqualTo(clone.DatabaseType));
        Assert.That(dtd.Id, Is.EqualTo(clone.Id));
        Assert.That(dtd.Key, Is.EqualTo(clone.Key));
        Assert.That(dtd.Level, Is.EqualTo(clone.Level));
        Assert.That(dtd.Name, Is.EqualTo(clone.Name));
        Assert.That(dtd.ParentId, Is.EqualTo(clone.ParentId));
        Assert.That(dtd.Path, Is.EqualTo(clone.Path));
        Assert.That(dtd.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(dtd.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(dtd.UpdateDate, Is.EqualTo(clone.UpdateDate));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(dtd, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item));
    }
}
