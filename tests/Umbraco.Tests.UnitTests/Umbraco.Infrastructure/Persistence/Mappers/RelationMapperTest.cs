// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="RelationMapper"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class RelationMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[id]"));
    }

    /// <summary>
    /// Tests that the ChildId property is correctly mapped to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_ChildId_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildId");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[childId]"));
    }

    /// <summary>
    /// Tests that the RelationMapper correctly maps the "CreateDate" property to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Datetime_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("CreateDate");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[datetime]"));
    }

    /// <summary>
    /// Tests that the Comment property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Comment_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Comment");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[comment]"));
    }

    /// <summary>
    /// Tests that the RelationMapper correctly maps the RelationTypeId property to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_RelationType_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("RelationTypeId");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelation].[relType]"));
    }
}
