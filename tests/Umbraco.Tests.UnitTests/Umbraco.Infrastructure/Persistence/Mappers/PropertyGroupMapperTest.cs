// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="PropertyGroupMapper"/> class, verifying its mapping functionality within the persistence layer.
/// </summary>
[TestFixture]
public class PropertyGroupMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[id]"));
    }

    /// <summary>
    /// Tests that the SortOrder property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[sortorder]"));
    }

    /// <summary>
    /// Tests that the Name property is correctly mapped to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[text]"));
    }
}
