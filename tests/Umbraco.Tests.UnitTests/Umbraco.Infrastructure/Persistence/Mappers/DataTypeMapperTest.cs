// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DataTypeMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
    }

    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[uniqueId]"));
    }

    [Test]
    public void Can_Map_DatabaseType_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DatabaseType");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[dbType]"));
    }

    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("EditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
    }
}
