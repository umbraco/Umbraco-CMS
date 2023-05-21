// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class PropertyTests
{
    [SetUp]
    public void SetUp() => _builder = new PropertyBuilder();

    private PropertyBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var property = BuildProperty();

        property.SetValue("hello");
        property.PublishValues();

        var clone = (Property)property.DeepClone();

        Assert.AreNotSame(clone, property);
        Assert.AreNotSame(clone.Values, property.Values);
        Assert.AreNotSame(property.PropertyType, clone.PropertyType);
        for (var i = 0; i < property.Values.Count; i++)
        {
            Assert.AreNotSame(property.Values.ElementAt(i), clone.Values.ElementAt(i));
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(property, null));
        }
    }

    private IProperty BuildProperty() =>
        _builder
            .WithId(4)
            .AddPropertyType()
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
            .Done()
            .Build();
}
