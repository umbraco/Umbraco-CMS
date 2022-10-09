// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class PropertyTypeMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
    }

    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
    }

    [Test]
    public void Can_Map_DataTypeDefinitionId_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DataTypeId");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
    }

    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
    }

    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column =
            new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("PropertyEditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
    }

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
