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
public class PropertyGroupTests
{
    [SetUp]
    public void SetUp() => _builder = new PropertyGroupBuilder();

    private PropertyGroupBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var pg = BuildPropertyGroup();

        var clone = (PropertyGroup)pg.DeepClone();

        Assert.That(pg, Is.Not.SameAs(clone));
        Assert.That(pg, Is.EqualTo(clone));
        Assert.That(pg.Id, Is.EqualTo(clone.Id));
        Assert.That(pg.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(pg.Key, Is.EqualTo(clone.Key));
        Assert.That(pg.Name, Is.EqualTo(clone.Name));
        Assert.That(pg.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(pg.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(pg.PropertyTypes, Is.Not.SameAs(clone.PropertyTypes));
        Assert.That(pg.PropertyTypes, Is.EqualTo(clone.PropertyTypes));
        Assert.That(pg.PropertyTypes, Has.Count.EqualTo(clone.PropertyTypes.Count));
        for (var i = 0; i < pg.PropertyTypes.Count; i++)
        {
            Assert.That(pg.PropertyTypes[i], Is.Not.SameAs(clone.PropertyTypes[i]));
            Assert.That(pg.PropertyTypes[i], Is.EqualTo(clone.PropertyTypes[i]));
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(pg, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var pg = BuildPropertyGroup();

        var json = JsonSerializer.Serialize(pg);
        Debug.Print(json);
    }

    private PropertyGroup BuildPropertyGroup() =>
        _builder
            .WithId(77)
            .WithName("Group1")
            .AddPropertyType()
            .WithId(3)
            .WithAlias("test")
            .WithName("Test")
            .WithDescription("testing")
            .WithPropertyGroupId(11)
            .WithMandatory(true)
            .WithValidationRegExp("xxxx")
            .Done()
            .AddPropertyType()
            .WithId(4)
            .WithAlias("test2")
            .WithName("Test2")
            .Done()
            .Build();
}
