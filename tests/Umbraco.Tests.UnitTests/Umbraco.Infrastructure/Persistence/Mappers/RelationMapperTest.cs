// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class RelationMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[id]"));
    }

    [Test]
    public void Can_Map_ChildId_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildId");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[childId]"));
    }

    [Test]
    public void Can_Map_Datetime_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("CreateDate");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[datetime]"));
    }

    [Test]
    public void Can_Map_Comment_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Comment");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[comment]"));
    }

    [Test]
    public void Can_Map_RelationType_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("RelationTypeId");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[relType]"));
    }
}
