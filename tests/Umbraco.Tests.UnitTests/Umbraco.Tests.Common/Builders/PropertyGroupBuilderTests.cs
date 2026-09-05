// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
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
        Assert.That(propertyGroup.Id, Is.EqualTo(testId));
        Assert.That(propertyGroup.Name, Is.EqualTo(testName));
        Assert.That(propertyGroup.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(propertyGroup.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(propertyGroup.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(propertyGroup.Key, Is.EqualTo(testKey));
        Assert.That(propertyGroup.PropertyTypes, Has.Count.EqualTo(1));
        Assert.That(propertyGroup.PropertyTypes[0].Id, Is.EqualTo(testPropertyTypeId));
    }
}
