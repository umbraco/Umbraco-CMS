// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Unit tests for the <see cref="DictionaryMapper"/> class.
/// </summary>
[TestFixture]
public class DictionaryMapperTest
{
    /// <summary>
    /// Tests that the DictionaryMapper correctly maps the Id property to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[pk]"));
    }

    /// <summary>
    /// Tests that the Key property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[id]"));
    }

    /// <summary>
    /// Tests that the ItemKey property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_ItemKey_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ItemKey");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[key]"));
    }
}
