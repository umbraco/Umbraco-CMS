// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class PropertyGroupMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[id]"));
    }

    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[sortorder]"));
    }

    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[text]"));
    }
}
