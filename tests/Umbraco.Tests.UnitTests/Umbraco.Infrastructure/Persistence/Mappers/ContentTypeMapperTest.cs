// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class ContentTypeMapperTest
{
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
    }

    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoNode].[text]"));
    }

    [Test]
    public void Can_Map_Thumbnail_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Thumbnail");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsContentType].[thumbnail]"));
    }

    [Test]
    public void Can_Map_Description_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Description");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsContentType].[description]"));
    }
}
