// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class RelationTypeMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[id]"));
    }

    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[alias]"));
    }

    [Test]
    public void Can_Map_ChildObjectType_Property()
    {
        // Act
        var column =
            new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildObjectType");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[childObjectType]"));
    }

    [Test]
    public void Can_Map_IsBidirectional_Property()
    {
        // Act
        var column =
            new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("IsBidirectional");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[dual]"));
    }
}
