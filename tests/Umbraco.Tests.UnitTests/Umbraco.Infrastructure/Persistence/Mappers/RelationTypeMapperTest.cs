// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="RelationTypeMapper"/> class, verifying its mapping functionality and behavior.
/// </summary>
[TestFixture]
public class RelationTypeMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the expected database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[id]"));
    }

    /// <summary>
    /// Tests that the Alias property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[alias]"));
    }

    /// <summary>
    /// Tests that the ChildObjectType property is correctly mapped by the RelationTypeMapper.
    /// </summary>
    [Test]
    public void Can_Map_ChildObjectType_Property()
    {
        // Act
        var column =
            new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildObjectType");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoRelationType].[childObjectType]"));
    }

    /// <summary>
    /// Tests that the IsBidirectional property maps correctly to the expected database column.
    /// </summary>
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
