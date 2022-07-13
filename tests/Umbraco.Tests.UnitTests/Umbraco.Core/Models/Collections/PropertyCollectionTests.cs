// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Collections;

[TestFixture]
public class PropertyCollectionTests
{
    [Test]
    public void Property_Adds_Case_Insensitive_Compare()
    {
        var collection = new PropertyCollection
        {
            new Property(new PropertyType(TestHelper.ShortStringHelper, "propEditor", ValueStorageType.Nvarchar, "test")),
            new Property(new PropertyType(TestHelper.ShortStringHelper, "propEditor", ValueStorageType.Nvarchar, "Test")),
        };

        Assert.AreEqual(1, collection.Count);
    }

    [Test]
    public void Property_Contains_Case_Insensitive_Compare()
    {
        var collection = new PropertyCollection
        {
            new Property(new PropertyType(TestHelper.ShortStringHelper, "propEditor", ValueStorageType.Nvarchar, "test")),
        };

        Assert.IsTrue(collection.Contains("Test"));
    }

    [Test]
    public void SimpleOrder_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var orders = new SimpleOrder();
        var item = orders.FirstOrDefault();

        Assert.That(item == null, Is.True);
    }

    [Test]
    public void PropertyCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<Property>();
        var collection = new PropertyCollection(list);

        var first = collection.FirstOrDefault();
        var second = collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test"));

        Assert.That(first, Is.Null);
        Assert.That(first == null, Is.True);
        Assert.That(second == null, Is.True);
    }

    [Test]
    public void PropertyTypeCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyType>();
        var collection = new PropertyTypeCollection(false, list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test")) == null, Is.True);
    }

    [Test]
    public void PropertyGroupCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyGroup>();
        var collection = new PropertyGroupCollection(list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Name.InvariantEquals("Test")) == null, Is.True);
    }

    [Test]
    public void PropertyGroups_Collection_FirstOrDefault_Returns_Null()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        Assert.That(contentType.PropertyGroups, Is.Not.Null);
        Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Content")) == null, Is.False);
        Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Test")) == null, Is.True);
        Assert.That(contentType.PropertyGroups.Any(x => x.Name.InvariantEquals("Test")), Is.False);
    }
}
