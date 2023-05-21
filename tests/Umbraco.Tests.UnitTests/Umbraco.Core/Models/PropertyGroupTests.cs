// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Newtonsoft.Json;
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

        Assert.AreNotSame(clone, pg);
        Assert.AreEqual(clone, pg);
        Assert.AreEqual(clone.Id, pg.Id);
        Assert.AreEqual(clone.CreateDate, pg.CreateDate);
        Assert.AreEqual(clone.Key, pg.Key);
        Assert.AreEqual(clone.Name, pg.Name);
        Assert.AreEqual(clone.SortOrder, pg.SortOrder);
        Assert.AreEqual(clone.UpdateDate, pg.UpdateDate);
        Assert.AreNotSame(clone.PropertyTypes, pg.PropertyTypes);
        Assert.AreEqual(clone.PropertyTypes, pg.PropertyTypes);
        Assert.AreEqual(clone.PropertyTypes.Count, pg.PropertyTypes.Count);
        for (var i = 0; i < pg.PropertyTypes.Count; i++)
        {
            Assert.AreNotSame(clone.PropertyTypes[i], pg.PropertyTypes[i]);
            Assert.AreEqual(clone.PropertyTypes[i], pg.PropertyTypes[i]);
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(pg, null));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var pg = BuildPropertyGroup();

        var json = JsonConvert.SerializeObject(pg);
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
