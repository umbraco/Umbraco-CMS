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

/// <summary>
/// Contains unit tests for the <see cref="PropertyCollection"/> class in the Umbraco CMS core models collections namespace.
/// </summary>
[TestFixture]
public class PropertyCollectionTests
{
    /// <summary>
    /// Tests that the PropertyCollection adds properties using a case-insensitive comparison,
    /// ensuring that properties with names differing only by case are treated as duplicates.
    /// </summary>
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

    /// <summary>
    /// Tests that the PropertyCollection.Contains method performs a case-insensitive comparison.
    /// </summary>
    [Test]
    public void Property_Contains_Case_Insensitive_Compare()
    {
        var collection = new PropertyCollection
        {
            new Property(new PropertyType(TestHelper.ShortStringHelper, "propEditor", ValueStorageType.Nvarchar, "test")),
        };

        Assert.IsTrue(collection.Contains("Test"));
    }

    /// <summary>
    /// Tests that the PropertyCollection returns null when FirstOrDefault is called on an empty collection.
    /// </summary>
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

    /// <summary>
    /// Tests that the PropertyTypeCollection returns null when FirstOrDefault is called on an empty collection.
    /// </summary>
    [Test]
    public void PropertyTypeCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyType>();
        var collection = new PropertyTypeCollection(false, list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Alias.InvariantEquals("Test")) == null, Is.True);
    }

    /// <summary>
    /// Tests that the PropertyGroupCollection returns null when FirstOrDefault is called on an empty collection.
    /// </summary>
    [Test]
    public void PropertyGroupCollection_Returns_Null_On_FirstOrDefault_When_Empty()
    {
        var list = new List<PropertyGroup>();
        var collection = new PropertyGroupCollection(list);

        Assert.That(collection.FirstOrDefault(), Is.Null);
        Assert.That(collection.FirstOrDefault(x => x.Name.InvariantEquals("Test")) == null, Is.True);
    }

    /// <summary>
    /// Tests that the PropertyGroups collection's FirstOrDefault method returns null when no matching group is found.
    /// </summary>
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
