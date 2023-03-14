// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class PropertyGroupBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 77;
        var testKey = Guid.NewGuid();
        const string testName = "Group1";
        const int testSortOrder = 555;
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const int testPropertyTypeId = 3;

        var builder = new PropertyGroupBuilder();

        // Act
        var propertyGroup = builder
            .WithId(testId)
            .WithCreateDate(testCreateDate)
            .WithName(testName)
            .WithSortOrder(testSortOrder)
            .WithKey(testKey)
            .WithUpdateDate(testUpdateDate)
            .AddPropertyType()
            .WithId(3)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(testId, propertyGroup.Id);
        Assert.AreEqual(testName, propertyGroup.Name);
        Assert.AreEqual(testSortOrder, propertyGroup.SortOrder);
        Assert.AreEqual(testCreateDate, propertyGroup.CreateDate);
        Assert.AreEqual(testUpdateDate, propertyGroup.UpdateDate);
        Assert.AreEqual(testKey, propertyGroup.Key);
        Assert.AreEqual(1, propertyGroup.PropertyTypes.Count);
        Assert.AreEqual(testPropertyTypeId, propertyGroup.PropertyTypes[0].Id);
    }
}
