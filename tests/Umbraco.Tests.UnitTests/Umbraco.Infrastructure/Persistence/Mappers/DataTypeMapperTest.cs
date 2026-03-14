// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="DataTypeMapper"/> class, verifying its mapping functionality and behavior.
/// </summary>
[TestFixture]
public class DataTypeMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
    }

    /// <summary>
    /// Tests that the Key property maps correctly to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[uniqueId]"));
    }

    /// <summary>
    /// Tests that the DatabaseType property is correctly mapped.
    /// </summary>
    [Test]
    public void Can_Map_DatabaseType_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DatabaseType");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[dbType]"));
    }

    /// <summary>
    /// Tests that the PropertyEditorAlias property maps correctly to the database column.
    /// </summary>
    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("EditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
    }
}
