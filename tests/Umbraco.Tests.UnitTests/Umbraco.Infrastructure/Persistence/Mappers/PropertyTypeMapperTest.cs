// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="PropertyTypeMapper"/> class to verify its behavior and functionality.
/// </summary>
[TestFixture]
public class PropertyTypeMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
    }

    /// <summary>
    /// Tests that the Alias property is correctly mapped by the PropertyTypeMapper.
    /// </summary>
    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
    }

    /// <summary>
    /// Tests that the DataTypeDefinitionId property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_DataTypeDefinitionId_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DataTypeId");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
    }

    /// <summary>
    /// Tests that the SortOrder property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
    }

    /// <summary>
    /// Tests that the PropertyEditorAlias property is correctly mapped to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column =
            new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("PropertyEditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
    }

    /// <summary>
    /// Tests that the PropertyTypeMapper correctly maps the DataTypeDatabaseType property.
    /// </summary>
    [Test]
    public void Can_Map_DataTypeDatabaseType_Property()
    {
        // Act
        var column =
            new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ValueStorageType");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[dbType]"));
    }
}
