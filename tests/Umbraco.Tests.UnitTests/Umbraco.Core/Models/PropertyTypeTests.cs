// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class PropertyTypeTests
{
    [SetUp]
    public void SetUp()
    {
        _propertyTypeBuilder = new PropertyTypeBuilder();
        _dataTypeBuilder = new DataTypeBuilder();
    }

    private PropertyTypeBuilder _propertyTypeBuilder;
    private DataTypeBuilder _dataTypeBuilder;

    [Test]
    public void Can_Create_From_DataType()
    {
        var shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        var dt = BuildDataType();
        var pt = new PropertyType(shortStringHelper, dt);

        Assert.That(pt.DataTypeId, Is.EqualTo(dt.Id));
        Assert.That(pt.DataTypeKey, Is.EqualTo(dt.Key));
        Assert.That(pt.PropertyEditorAlias, Is.EqualTo(dt.EditorAlias));
        Assert.That(pt.ValueStorageType, Is.EqualTo(dt.DatabaseType));
    }

    [Test]
    public void Can_Deep_Clone()
    {
        var pt = BuildPropertyType();

        var clone = (PropertyType)pt.DeepClone();

        Assert.That(pt, Is.Not.SameAs(clone));
        Assert.That(pt, Is.EqualTo(clone));
        Assert.That(pt.Id, Is.EqualTo(clone.Id));
        Assert.That(pt.Alias, Is.EqualTo(clone.Alias));
        Assert.That(pt.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(pt.DataTypeId, Is.EqualTo(clone.DataTypeId));
        Assert.That(pt.DataTypeKey, Is.EqualTo(clone.DataTypeKey));
        Assert.That(pt.Description, Is.EqualTo(clone.Description));
        Assert.That(pt.Key, Is.EqualTo(clone.Key));
        Assert.That(pt.Mandatory, Is.EqualTo(clone.Mandatory));
        Assert.That(pt.Name, Is.EqualTo(clone.Name));
        Assert.That(pt.PropertyGroupId.Value, Is.EqualTo(clone.PropertyGroupId.Value));
        Assert.That(pt.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(pt.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(pt.ValidationRegExp, Is.EqualTo(clone.ValidationRegExp));
        Assert.That(pt.ValueStorageType, Is.EqualTo(clone.ValueStorageType));

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

            Assert.That(actual, Is.EqualTo(expected), $"Value of propery: '{propertyInfo.Name}': {expected} != {actual}");
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var pt = BuildPropertyType();

        var json = JsonSerializer.Serialize(pt);
        Debug.Print(json);
    }

    private PropertyType BuildPropertyType() =>
        _propertyTypeBuilder
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

    private DataType BuildDataType() =>
        _dataTypeBuilder
            .Build();
}
