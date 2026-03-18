// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="ContentTypeMapper"/> class, verifying its mapping logic and behavior.
/// </summary>
[TestFixture]
public class ContentTypeMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
    }

    /// <summary>
    /// Tests that the Name property maps correctly to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[text]"));
    }

    /// <summary>
    /// Tests that the Thumbnail property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Thumbnail_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Thumbnail");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsContentType].[thumbnail]"));
    }

    /// <summary>
    /// Tests that the Description property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Description_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Description");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsContentType].[description]"));
    }
}
