// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class PropertyTypeTests
{
    [SetUp]
    public void SetUp() => _builder = new PropertyTypeBuilder();

    private PropertyTypeBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var pt = BuildPropertyType();

        var clone = (PropertyType)pt.DeepClone();

        Assert.AreNotSame(clone, pt);
        Assert.AreEqual(clone, pt);
        Assert.AreEqual(clone.Id, pt.Id);
        Assert.AreEqual(clone.Alias, pt.Alias);
        Assert.AreEqual(clone.CreateDate, pt.CreateDate);
        Assert.AreEqual(clone.DataTypeId, pt.DataTypeId);
        Assert.AreEqual(clone.DataTypeId, pt.DataTypeId);
        Assert.AreEqual(clone.Description, pt.Description);
        Assert.AreEqual(clone.Key, pt.Key);
        Assert.AreEqual(clone.Mandatory, pt.Mandatory);
        Assert.AreEqual(clone.Name, pt.Name);
        Assert.AreEqual(clone.PropertyGroupId.Value, pt.PropertyGroupId.Value);
        Assert.AreEqual(clone.SortOrder, pt.SortOrder);
        Assert.AreEqual(clone.UpdateDate, pt.UpdateDate);
        Assert.AreEqual(clone.ValidationRegExp, pt.ValidationRegExp);
        Assert.AreEqual(clone.ValueStorageType, pt.ValueStorageType);

        // This double verifies by reflection (don't test properties marked with [DoNotClone]
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps.Where(p => p.GetCustomAttribute<DoNotCloneAttribute>(false) == null))
        {
            var expected = propertyInfo.GetValue(pt, null);
            var actual = propertyInfo.GetValue(clone, null);
            if (propertyInfo.PropertyType == typeof(Lazy<int>))
            {
                expected = ((Lazy<int>)expected).Value;
                actual = ((Lazy<int>)actual).Value;
            }

            Assert.AreEqual(expected, actual, $"Value of propery: '{propertyInfo.Name}': {expected} != {actual}");
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var pt = BuildPropertyType();

        var json = JsonConvert.SerializeObject(pt);
        Debug.Print(json);
    }

    private PropertyType BuildPropertyType() =>
        _builder
            .WithId(3)
            .WithPropertyEditorAlias("TestPropertyEditor")
            .WithAlias("test")
            .WithName("Test")
            .WithSortOrder(9)
            .WithDataTypeId(5)
            .WithDescription("testing")
            .WithPropertyGroupId(11)
            .WithMandatory(true)
            .WithValidationRegExp("xxxx")
            .Build();
}
