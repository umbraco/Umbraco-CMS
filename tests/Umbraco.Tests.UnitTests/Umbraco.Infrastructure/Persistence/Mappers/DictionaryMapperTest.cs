// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DictionaryMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[pk]"));
    }

    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[id]"));
    }

    [Test]
    public void Can_Map_ItemKey_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ItemKey");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsDictionary].[key]"));
    }
}
