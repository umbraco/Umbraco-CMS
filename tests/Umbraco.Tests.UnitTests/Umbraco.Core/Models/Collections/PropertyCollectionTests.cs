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

        Assert.That(collection, Has.Count.EqualTo(1));
    }

    [Test]
    public void Property_Contains_Case_Insensitive_Compare()
    {
        var collection = new PropertyCollection
        {
            new Property(new PropertyType(TestHelper.ShortStringHelper, "propEditor", ValueStorageType.Nvarchar, "test")),
        };

        Assert.That(collection.Contains("Test"), Is.True);
    }

    [Test]
    public void PropertyCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<Property>();
        var collection = new PropertyCollection(list);

        var first = collection.FirstOrDefault();
        var second = collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test"));

        Assert.That(first, Is.Null);
        Assert.That(first, Is.EqualTo(null));
        Assert.That(second, Is.EqualTo(null));
    }

    [Test]
    public void PropertyTypeCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyType>();
        var collection = new PropertyTypeCollection(false, list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test")), Is.EqualTo(null));
    }

    [Test]
    public void PropertyGroupCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyGroup>();
        var collection = new PropertyGroupCollection(list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Name.InvariantEquals("Test")), Is.EqualTo(null));
    }

    [Test]
    public void PropertyGroups_Collection_FirstOrDefault_Returns_Null()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        Assert.That(contentType.PropertyGroups, Is.Not.Null);
        Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Content")), Is.Not.EqualTo(null));
        Assert.That(contentType.PropertyGroups.FirstOrDefault(x => x.Name.InvariantEquals("Test")), Is.EqualTo(null));
        Assert.That(contentType.PropertyGroups.Any(x => x.Name.InvariantEquals("Test")), Is.False);
    }
}
